/*
 * $Id: IndexedImageUtils.cs 9 2007-05-27 10:47:07Z spocke $
 *
 * Copyright © 2007, Moxiecode Systems AB, All rights reserved. 
 */

using System;
using System.IO;
using System.Web;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Security.Cryptography;

namespace Moxiecode.ImageManager.Utils {
	/// <summary>
	/// ...
	/// </summary>
	public class IndexedImageUtils {

#if (UNSAFE)

		private static void AdjustSizes(Bitmap bitmap, ref int xSize, ref int ySize) {
			if (xSize != 0 && ySize == 0)
				ySize = Math.Abs((int) (xSize * bitmap.Height / bitmap.Width));
			else if (xSize == 0 && ySize != 0)
				xSize = Math.Abs((int) (ySize * bitmap.Width / bitmap.Height));
			else if (xSize == 0 && ySize == 0) {
				xSize = bitmap.Width;
				ySize = bitmap.Height;
			}
		}

		private static byte GetSourceByteAt(IntPtr sourceScan0, double xFactor, double yFactor, int sourceStride, int x, int y) {
			unsafe {
				return ((byte*) ((int)sourceScan0 + (int) (Math.Floor(y * yFactor) * sourceStride) + (int) Math.Floor(x * xFactor)))[0];
			}
		}

		/// <summary>
		///  This method does a indexed resize of a bitmap and returns a new resized bitmap.
		/// </summary>
		/// <param name="path">File to resize.</param>
		/// <param name="xSize">width of new bitmap.</param>
		/// <param name="ySize">height of new bitmap.</param>
		/// <returns>Net resized bitmap.</returns>
		public static Bitmap IndexedResize(string path, int xSize, int ySize) {
			Bitmap bitmap = new Bitmap(path);
			BitmapData sourceBitmapData, targetBitmapData;
			Bitmap scaledBitmap;
			double xFactor, yFactor;
			System.IntPtr sourceScan0;
			int sourceStride;

			AdjustSizes(bitmap, ref xSize, ref ySize);

			scaledBitmap = new Bitmap(xSize, ySize, bitmap.PixelFormat);
			scaledBitmap.Palette = bitmap.Palette;

			sourceBitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
			try {
				targetBitmapData = scaledBitmap.LockBits(new Rectangle(0, 0, xSize, ySize), ImageLockMode.WriteOnly, scaledBitmap.PixelFormat);

				try {
					xFactor = (Double) bitmap.Width / (Double) scaledBitmap.Width;
					yFactor = (Double) bitmap.Height / (Double) scaledBitmap.Height;

					sourceStride = sourceBitmapData.Stride; 
					sourceScan0 = sourceBitmapData.Scan0;

					int targetStride = targetBitmapData.Stride; 
					System.IntPtr targetScan0 = targetBitmapData.Scan0;

					unsafe { 
						byte *p = (byte *)(void *)targetScan0;

						int nOffset = targetStride - scaledBitmap.Width; 
						int nWidth = scaledBitmap.Width;

						for (int y=0; y<scaledBitmap.Height; ++y) {
							for (int x=0; x<nWidth; ++x) {
								p[0] = GetSourceByteAt(sourceScan0, xFactor, yFactor, sourceStride, x, y);
								++p;
							}

							p += nOffset;
						}
					}
				} finally {
					scaledBitmap.UnlockBits(targetBitmapData);
				}
			} finally {
				bitmap.UnlockBits(sourceBitmapData);
				bitmap.Dispose();
			}

			return scaledBitmap;
		}

		/// <summary>
		///  This method does a indexed crop of a bitmap and returns a new croped bitmap.
		/// </summary>
		/// <param name="path">File to crop.</param>
		/// <param name="tx">x pos.</param>
		/// <param name="ty">y pos.</param>
		/// <param name="width">width.</param>
		/// <param name="height">height.</param>
		/// <returns>Net croped bitmap.</returns>
		public static Bitmap IndexedCrop(string path, int tx, int ty, int width, int height) {
			Bitmap bitmap = new Bitmap(path);
			BitmapData sourceBitmapData, targetBitmapData;
			Bitmap cropedBitmap;
			System.IntPtr sourceScan0;
			int sourceStride;

			// Range check
			width = tx + width > bitmap.Width ? bitmap.Width - tx : width;
			height = ty + height > bitmap.Height ? bitmap.Height - ty : height;

			cropedBitmap = new Bitmap(width, height, bitmap.PixelFormat);
			cropedBitmap.Palette = bitmap.Palette;

			sourceBitmapData = bitmap.LockBits(new Rectangle(tx, ty, width, height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
			try {
				targetBitmapData = cropedBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, cropedBitmap.PixelFormat);

				try {
					sourceStride = sourceBitmapData.Stride; 
					sourceScan0 = sourceBitmapData.Scan0;

					int targetStride = targetBitmapData.Stride; 
					System.IntPtr targetScan0 = targetBitmapData.Scan0;

					unsafe { 
						byte *p = (byte *)(void *)targetScan0;
						int nOffset = targetStride - cropedBitmap.Width; 

						for (int y=0; y<cropedBitmap.Height; y++) {
							for (int x=0; x<cropedBitmap.Width; x++) {
								p[0] = ((byte*) ((int)sourceScan0 + y * sourceStride) + x)[0];
								++p;
							}

							p += nOffset;
						}
					}
				} finally {
					cropedBitmap.UnlockBits(targetBitmapData);
				}
			} finally {
				bitmap.UnlockBits(sourceBitmapData);
				bitmap.Dispose();
			}

			return cropedBitmap;
		}

#else

		/// <summary>
		///  This method does a indexed resize of a bitmap and returns a new resized bitmap.
		/// </summary>
		/// <param name="path">Path to image to resize.</param>
		/// <param name="xSize">width of new bitmap.</param>
		/// <param name="ySize">height of new bitmap.</param>
		/// <returns>Net resized bitmap.</returns>
		public static Bitmap IndexedResize(string path, int xSize, int ySize) {
			Rectangle destRect, srcRect;
			Bitmap resizedBitmap = null;
			System.Drawing.Image img = null;

			img = System.Drawing.Image.FromFile(path);

			// Setup new bitmap
			resizedBitmap = new Bitmap(xSize, ySize, PixelFormat.Format32bppArgb);
			resizedBitmap.SetResolution(img.HorizontalResolution, img.VerticalResolution);

			// Draw resized image
			Graphics g = Graphics.FromImage(resizedBitmap);
			destRect = new Rectangle(0, 0, xSize, ySize);
			srcRect = new Rectangle(0, 0, img.Width, img.Height);
			g.DrawImage(img, destRect, srcRect, GraphicsUnit.Pixel);
			g.Dispose();
			img.Dispose();

			return resizedBitmap;
		}

		/// <summary>
		///  This method does a indexed crop of a bitmap and returns a new croped bitmap.
		/// </summary>
		/// <param name="path">Image path to crop.</param>
		/// <param name="tx">x pos.</param>
		/// <param name="ty">y pos.</param>
		/// <param name="width">width.</param>
		/// <param name="height">height.</param>
		/// <returns>Net croped bitmap.</returns>
		public static Bitmap IndexedCrop(string path, int tx, int ty, int width, int height) {
			Rectangle destRect, srcRect;
			Bitmap croppedBitmap = null;
			System.Drawing.Image img = null;

			img = System.Drawing.Image.FromFile(path);

			// Range check
			width = tx + width > img.Width ? img.Width - tx : width;
			height = ty + height > img.Height ? img.Height - ty : height;

			// Setup new bitmap
			croppedBitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			croppedBitmap.SetResolution(img.HorizontalResolution, img.VerticalResolution);

			// Draw resized image
			Graphics g = Graphics.FromImage(croppedBitmap);
			destRect = new Rectangle(0, 0, width, height);
			srcRect = new Rectangle(tx, ty, width, height);
			g.DrawImage(img, destRect, srcRect, GraphicsUnit.Pixel);
			g.Dispose();
			img.Dispose();

			return croppedBitmap;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <param name="rotate_flip"></param>
		/// <returns></returns>
		public static Bitmap RotateFlipImage(string path, RotateFlipType rotate_flip) {
			Rectangle destRect, srcRect;
			Bitmap bitmap = null;
			System.Drawing.Image img = null;

			img = System.Drawing.Image.FromFile(path);
			img.RotateFlip(rotate_flip);

			// Setup new bitmap
			bitmap = new Bitmap(img.Width, img.Height, PixelFormat.Format32bppArgb);
			bitmap.SetResolution(img.HorizontalResolution, img.VerticalResolution);

			// Draw it
			Graphics g = Graphics.FromImage(bitmap);
			destRect = new Rectangle(0, 0, img.Width, img.Height);
			srcRect = new Rectangle(0, 0, img.Width, img.Height);
			g.DrawImage(img, destRect, srcRect, GraphicsUnit.Pixel);

			g.Dispose();
			img.Dispose();

			return bitmap;
		}
#endif
	}
}
