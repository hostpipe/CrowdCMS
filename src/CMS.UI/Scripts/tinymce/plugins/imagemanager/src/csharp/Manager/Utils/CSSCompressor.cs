/*
 * $Id: CSSCompressor.cs 9 2007-05-27 10:47:07Z spocke $
 *
 * Copyright © 2007, Moxiecode Systems AB, All rights reserved. 
 */

using System;
using System.IO;
using System.Collections;
using System.Web;
using System.Text;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Moxiecode.ICSharpCode.SharpZipLib.GZip;

namespace Moxiecode.Manager.Utils {
	/// <summary>
	/// Description of CSSCompressor.
	/// </summary>
	public class CSSCompressor {
		private ArrayList items;
		private bool gzipCompress, diskCache, removeWhiteSpace, convertUrls;
		private int expiresOffset;
		private string cacheDir, charset, cacheFileName;
		private DateTime lastUpdate;

		/// <summary>
		/// 
		/// </summary>
		public CSSCompressor() {
			this.items = new ArrayList();
			this.diskCache = true;
			this.charset = "UTF-8";
			this.cacheDir = "_cache";
			this.expiresOffset = 3600 * 24 * 10; // 10 days
			this.gzipCompress = true;
			this.removeWhiteSpace = true;
			this.lastUpdate = new DateTime(0);
		}

		/// <summary>
		///  ...
		/// </summary>
		public bool ConvertUrls {
			get { return convertUrls; }
			set { convertUrls = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool GzipCompress {
			get { return gzipCompress; }
			set { gzipCompress = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool RemoveWhiteSpace {
			get { return removeWhiteSpace; }
			set { removeWhiteSpace = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool DiskCache {
			get { return diskCache; }
			set { diskCache = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public int ExpiresOffset {
			get { return expiresOffset; }
			set { expiresOffset = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string CharSet {
			get { return charset; }
			set { charset = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string CacheDir {
			get { return cacheDir; }
			set { cacheDir = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string CacheFileName {
			get { return cacheFileName; }
			set { cacheFileName = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="file"></param>
		public void AddFile(string file) {
			this.items.Add(new CSSCompressItem(CSSItemType.File, file, true));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="file"></param>
		/// <param name="remove_whitespace"></param>
		public void AddFile(string file, bool remove_whitespace) {
			this.items.Add(new CSSCompressItem(CSSItemType.File, file, remove_whitespace));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="content"></param>
		public void AddContent(string content) {
			this.items.Add(new CSSCompressItem(CSSItemType.Content, content, true));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="content"></param>
		/// <param name="remove_whitespace"></param>
		public void AddContent(string content, bool remove_whitespace) {
			this.items.Add(new CSSCompressItem(CSSItemType.Content, content, remove_whitespace));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="request"></param>
		/// <param name="response"></param>
		public void Compress(HttpRequest request, HttpResponse response) {
			Encoding encoding = Encoding.GetEncoding("windows-1252");
			string enc, cacheFile = null, cacheKey = null, content = "";
			StringWriter writer = new StringWriter();
			byte[] buff = new byte[1024];
			GZipOutputStream gzipStream;
			bool supportsGzip;

			// Set response headers
			response.ContentType = "text/css";
			response.Charset = this.charset;
			response.Buffer = false;

			// Setup cache
			response.Cache.SetExpires(DateTime.Now.AddSeconds(this.ExpiresOffset));

			// Check if it supports gzip
			enc = Regex.Replace("" + request.Headers["Accept-Encoding"], @"\s+", "").ToLower();
			supportsGzip = enc.IndexOf("gzip") != -1 || request.Headers["---------------"] != null;
			enc = enc.IndexOf("x-gzip") != -1 ? "x-gzip" : "gzip";

			// Setup cache info
			if (this.diskCache) {
				cacheKey = "";

				foreach (CSSCompressItem item in this.items) {
					// Get last mod
					if (item.Type == CSSItemType.File) {
						DateTime fileMod = File.GetLastWriteTime(request.MapPath(item.Value));

						if (fileMod > this.lastUpdate)
							this.lastUpdate = fileMod;
					}

					cacheKey += item.Value;
				}

				cacheKey = this.cacheFileName != null ? this.cacheFileName : MD5(cacheKey);

				if (this.gzipCompress)
					cacheFile = request.MapPath(this.cacheDir + "/" + cacheKey + ".gz");
				else
					cacheFile = request.MapPath(this.cacheDir + "/" + cacheKey + ".css");
			}

			// Use cached file disk cache
			if (this.diskCache && supportsGzip && File.Exists(cacheFile) && this.lastUpdate == File.GetLastWriteTime(cacheFile)) {
				if (this.gzipCompress)
					response.AppendHeader("Content-Encoding", enc);

				response.WriteFile(cacheFile);
				return;
			}

			foreach (CSSCompressItem item in this.items) {
				if (item.Type == CSSItemType.File) {
					StreamReader reader = new StreamReader(File.OpenRead(request.MapPath(item.Value)), System.Text.Encoding.UTF8);
					string data;

					if (item.RemoveWhiteSpace)
						data = this.TrimWhiteSpace(reader.ReadToEnd());
					else
						data = reader.ReadToEnd();

					if (this.convertUrls)
						data = Regex.Replace(data, "url\\(['\"]?(?!\\/|http)", "$0" + PathUtils.ToUnixPath(Path.GetDirectoryName(item.Value)) + "/");

					writer.Write(data);

					reader.Close();
				} else {
					if (item.RemoveWhiteSpace)
						writer.Write(this.TrimWhiteSpace(item.Value));
					else
						writer.Write(item.Value);
				}
			}

			content = writer.ToString();

			// Generate GZIP'd content
			if (supportsGzip) {
				if (this.gzipCompress)
					response.AppendHeader("Content-Encoding", enc);

				if (this.diskCache && cacheKey != null) {
					try {
						// Gzip compress
						if (this.gzipCompress) {
							gzipStream = new GZipOutputStream(File.Create(cacheFile));
							buff = encoding.GetBytes(content.ToCharArray());
							gzipStream.Write(buff, 0, buff.Length);
							gzipStream.Close();

							File.SetLastWriteTime(cacheFile, this.lastUpdate);
						} else {
							StreamWriter sw = File.CreateText(cacheFile);
							sw.Write(content);
							sw.Close();

							File.SetLastWriteTime(cacheFile, this.lastUpdate);
						}

						// Write to stream
						response.WriteFile(cacheFile);
					} catch (Exception) {
						content = "/* Not cached */" + content;
						if (this.gzipCompress) {
							gzipStream = new GZipOutputStream(response.OutputStream);
							buff = encoding.GetBytes(content.ToCharArray());
							gzipStream.Write(buff, 0, buff.Length);
							gzipStream.Close();
						} else {
							response.Write(content);
						}
					}
				} else {
					content = "/* Not cached */" + content;
					gzipStream = new GZipOutputStream(response.OutputStream);
					buff = encoding.GetBytes(content.ToCharArray());
					gzipStream.Write(buff, 0, buff.Length);
					gzipStream.Close();
				}
			} else {
				content = "/* Not cached */" + content;
				response.Write(content);
			}
		}

		#region private

		private string TrimWhiteSpace(string content) {
			// Remove multiple tabs or spaces
			content = Regex.Replace(content, @"[ \t]+", " ");

			// Remove whitespace at beginning and end of lines
			content = Regex.Replace(content, @"^[ \t]*|[ \t]*$", "", RegexOptions.Multiline);

			// Remove comments
			content = Regex.Replace(content, @"(\/\*[^*]*\*+([^\/][^*]*\*+)*\/)", "");

			// Remove whitespace at the beginning and end of CSS
			content = Regex.Replace(content, @"^[\r\n]+", "", RegexOptions.Multiline);
			content = Regex.Replace(content, @"[\r\n]+$", "\n", RegexOptions.Multiline);

			// Remove redundant linebreaks
			content = Regex.Replace(content, @"\r\n", "\n");
			content = Regex.Replace(content, @"\n+", "\n");

			// Remove remove whitespace between style rules and after the last one
			content = Regex.Replace(content, @";\s+", ";");
			content = Regex.Replace(content, @"\{([^\}]+);\}", "{$1}");

			// Remove whitespace between :
			content = Regex.Replace(content, @"\s*\:\s*", ":");

			// Remove whitespace before/after styles inside rules
			content = Regex.Replace(content, @"\{\s*(.*?)\s*\}", "{$1}");

			// Replace 0px with 0
			content = Regex.Replace(content, @"([: ]0)px", "$1");

			return content;
		}

		private string MD5(string str) {
			MD5 md5 = new MD5CryptoServiceProvider();
			byte[] result = md5.ComputeHash(Encoding.ASCII.GetBytes(str));
			str = BitConverter.ToString(result);
	
			return str.Replace("-", "");
		}

		#endregion
	}

	enum CSSItemType {
		File,
		Content
	}

	class CSSCompressItem {
		CSSItemType type;
		string val;
		bool removeWhiteSpace;

		public CSSCompressItem(CSSItemType type, string val, bool remove_whitespace) {
			this.type = type;
			this.val = val;
			this.removeWhiteSpace = remove_whitespace;
		}

		public CSSItemType Type {
			get { return type; }
		}

		public string Value {
			get { return this.val; }
			set { this.val = value; }
		}

		public bool RemoveWhiteSpace {
			get { return removeWhiteSpace; }
		}
	}
}
