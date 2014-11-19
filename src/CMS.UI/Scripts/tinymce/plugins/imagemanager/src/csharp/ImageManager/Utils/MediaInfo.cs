/*
 * $Id: MediaInfo.cs 861 2012-04-11 12:24:30Z spocke $
 *
 * Copyright © 2007, Moxiecode Systems AB, All rights reserved. 
 */

using System;
using System.Collections;
using System.IO;
using System.Text;
using Moxiecode.ICSharpCode.SharpZipLib.Zip.Compression;

namespace Moxiecode.ImageManager.Utils {
	/// <summary>
	///  
	/// </summary>
	public enum MediaFormat {
		/// <summary></summary>
		Unknown,

		/// <summary></summary>
		JPEG,

		/// <summary></summary>
		GIF,

		/// <summary></summary>
		PNG,

		/// <summary></summary>
		BMP,

		/// <summary></summary>
		SWF
	}

	/// <summary>
	/// 
	/// </summary>
	public class MediaInfo {
		private int width;
		private int height;
		private MediaFormat format;
		private Stream stream = null;
		private BinaryReader reader;
        private int bitPos = 0;
        private uint bitBuf = 0;
        private static object lockObj = new Object();

        /// <summary> </summary>
		public int Width {
			get { return width; }
		}

        /// <summary> </summary>
		public int Height {
			get { return height; }
		}

        /// <summary> </summary>
		public MediaFormat Format {
			get { return format; }
		}

         /// <summary> </summary>
		public MediaInfo() {
			this.width = -1;
			this.height = -1;
		}

        /// <summary> </summary>
		public MediaInfo(Stream stream) {
			this.stream = stream;
			this.LoadInfo();
			this.stream.Seek(0, SeekOrigin.Begin);
		}

        /// <summary> </summary>
		public MediaInfo(string path) {
        	lock (lockObj) {
				this.stream = File.OpenRead(path);

				try {
					this.LoadInfo();
				} finally {
					this.stream.Close();
				}
        	}
		}

        #region private methods

		private bool LoadInfo()  {
			if (stream == null)
				return false;

			format = MediaFormat.Unknown;
			width = -1;
			height = -1;
			stream.Position = 0;

			try {
				int b1 = stream.ReadByte() & 0xff;
				int b2 = stream.ReadByte() & 0xff;

				if ((b1 == 0x43 || b1 == 0x46) && b2 == 0x57)
					return CheckSwf(b1 == 0x43);
				if (b1 == 0x47 && b2 == 0x49) 
					return CheckGif();
				else if (b1 == 0x89 && b2 == 0x50) 
					return CheckPng();
				else if (b1 == 0xff && b2 == 0xd8) 
					return CheckJpeg();
				else if (b1 == 0x42 && b2 == 0x4d) 
					return CheckBmp();
				else 
					return false;
			} catch (IOException) {
				return false;
			}
		}

		private bool CheckSwf(bool is_comp) {
        	this.format = MediaFormat.SWF;
			this.reader = new BinaryReader(this.stream);

			if (is_comp) {
				int size = -1;

				this.reader.BaseStream.Position = 4; // Skip head
				size = Convert.ToInt32(this.reader.ReadUInt32());

				// Read swf head
				byte[] uncompressed = new byte[size];
				this.reader.BaseStream.Position = 0;
				this.reader.Read(uncompressed, 0, 8);

				// Read compressed data
				byte[] compressed = this.reader.ReadBytes(size);
				this.stream.Close(); // Close the old stream

				// Uncompress
				Inflater zipInflator = new Inflater();
				zipInflator.SetInput(compressed);
				zipInflator.Inflate(uncompressed, 8, size - 8);

				// Setup new uncompressed reader
				this.reader = new BinaryReader(new MemoryStream(uncompressed));
				this.reader.BaseStream.Position = 0;
			}

			// Skip header signature/version etc etc
			this.reader.BaseStream.Position = 8;

			// Read rect
			uint bits = ReadUBits(5);
            ReadSBits(bits); // Read xmin
            this.width = ReadSBits(bits) / 20; // Read xmax
            ReadSBits(bits); // Read ymin
            this.height = ReadSBits(bits) / 20; // Read ymax

			return true;
		}

        private uint ReadUBits(uint bits) {
            uint v = 0;

            while (true) {
                int s = (int) (bits - bitPos);

                if (s > 0) {
                    v |= bitBuf << s;
                    bits -= (uint) bitPos;

                    bitBuf = this.reader.ReadByte();
                    bitPos = 8;
                } else {
                    v |= bitBuf >> -s;

                    bitPos -= (int) bits;
                    bitBuf &= (uint) (0xff >> (8 - bitPos));

                    return v;
                }
            }
        }

        private int ReadSBits(uint bits) {
            int v = (int) (ReadUBits(bits));

            if ((v & (1L << (int) (bits - 1))) > 0)
                v |= -1 << (int) bits;

            return v;
        }

		private bool CheckBmp() {
			byte[] a = new byte[44];

			if (stream.Read(a, 0, 44) != 44)
				return false;

			width = getLWORD(a, 16);
			height = getLWORD(a, 20);

			if (width < 1 || height < 1) 
				return false;

			format = MediaFormat.BMP;

			return true;
		}

		private bool CheckGif() {
			byte[] GIF_MAGIC_87A = new byte[] { 0x46, 0x38, 0x37, 0x61 };
			byte[] GIF_MAGIC_89A = {0x46, 0x38, 0x39, 0x61};
			byte[] a = new byte[11]; // 4 from the GIF signature + 7 from the global header

			if (stream.Read(a, 0, 11) != 11) 
				return false;

			if ((!isSame(a, 0, GIF_MAGIC_89A, 0, 4)) && (!isSame(a, 0, GIF_MAGIC_87A, 0, 4)))
				return false;

			format = MediaFormat.GIF;
			width = getBYTE(a, 4);
			height = getBYTE(a, 6);

			int flags = a[8] & 0xff;

			// skip global color palette
			if ((flags & 0x80) != 0) {
				int tableSize = (1 << ((flags & 7) + 1)) * 3;

				stream.Position += tableSize;
			}

			int blockType;

			do {
				blockType = stream.ReadByte();

				switch(blockType) {
					case 0x21: // extension
						int extensionType = stream.ReadByte();
						int n3;

						do {
							n3 = stream.ReadByte();
							if (n3 > 0) 
								stream.Position += n3;
							else if (n3 == -1) 
								return false;
						} while (n3 > 0);
						break;

					case 0x3b: // end of file
						break;

					default:
						return false;
				}
			} while (blockType != 0x3b);

			return true;
		}

		private bool CheckJpeg() {
			byte[] data = new byte[12];

			while (true) {
				if (stream.Read(data, 0, 4) != 4) 
					return false;

				int marker = getWORD(data, 0);
				int size = getWORD(data, 2);

				if ((marker & 0xff00) != 0xff00) 
					return false; // not a valid marker

				if (marker >= 0xffc0 && marker <= 0xffcf && marker != 0xffc4 && marker != 0xffc8) {
					if (stream.Read(data, 0, 6) != 6) 
						return false;

					format = MediaFormat.JPEG;

					width = getWORD(data, 3);
					height = getWORD(data, 1);

					return true;
				} else
					stream.Position += size - 2;
			}
		}

		private bool CheckPng() {
			byte[] PNG_MAGIC = new byte[] {0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a};
			byte[] buff = new byte[27];

			if (stream.Read(buff, 0, 27) != 27) 
				return false;

			if (!isSame(buff, 0, PNG_MAGIC, 0, 6)) 
				return false;

			format = MediaFormat.PNG;
			width = getRLWORD(buff, 14);
			height = getRLWORD(buff, 18);

			return true;
		}

		private bool isSame(byte[] buff1, int offset1, byte[] buff2, int offset2, int num) {
			while (num-- > 0) 
				if (buff1[offset1++] != buff2[offset2++]) 
					return false;

			return true;
		}

		private int getRLWORD(byte[] buff, int offset) {
			return
				(buff[offset] & 0xff) << 24 | 
				(buff[offset + 1] & 0xff) << 16 | 
				(buff[offset + 2] & 0xff) << 8 | 
				buff[offset + 3] & 0xff;
		}

		private int getLWORD(byte[] buff, int offset) {
			return
				(buff[offset + 3] & 0xff) << 24 | 
				(buff[offset + 2] & 0xff) << 16 | 
				(buff[offset + 1] & 0xff) << 8 | 
				buff[offset] & 0xff;
		}

		private int getWORD(byte[] buff, int offset) {
			return (buff[offset] & 0xff) << 8 | (buff[offset + 1] & 0xff);
		}

		private int getBYTE(byte[] buff, int offset) {
			return (buff[offset] & 0xff) | (buff[offset + 1] & 0xff) << 8;
		}

        #endregion
	}
}
