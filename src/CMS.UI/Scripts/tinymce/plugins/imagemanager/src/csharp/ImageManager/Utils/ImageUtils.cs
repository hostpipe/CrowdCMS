/*
 * $Id: ImageUtils.cs 860 2012-04-10 14:29:31Z spocke $
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
using System.Collections;
using System.Text.RegularExpressions;
using Moxiecode.Manager.Utils;

namespace Moxiecode.ImageManager.Utils {
	/// <summary>
	/// Description of Class1.
	/// </summary>
	public class ImageUtils {
		/// <summary></summary>
		/// <param name="input_path"></param>
		/// <param name="response"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="quality"></param>
		public static void MakeThumbnail(string input_path, HttpResponse response, int width, int height, int quality) {
			MakeThumbnail(input_path, width, height, false, null, response, DateTime.Now, quality);
		}

		/// <summary></summary>
		/// <param name="input_path"></param>
		/// <param name="output_path"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="quality"></param>
		public static void MakeThumbnail(string input_path, string output_path, int width, int height, int quality) {
			MakeThumbnail(input_path, width, height, true, output_path, null, DateTime.Now, quality);
		}

		/// <summary></summary>
		/// <param name="input_path"></param>
		/// <param name="output_path"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="last_mod"></param>
		/// <param name="quality"></param>
		public static void MakeThumbnail(string input_path, string output_path, int width, int height, DateTime last_mod, int quality) {
			MakeThumbnail(input_path, width, height, true, output_path, null, last_mod, quality);
		}

		/// <summary>
	 	///  This class is the image list item constructor.
	 	/// </summary>
		/// <param name="path">Path of file to make thumbnail for.</param>
		/// <param name="width">Width of thumbnail.</param>
		/// <param name="height">Height of thumbnail.</param>
		/// <param name="save">Save or stream.</param>
		/// <param name="output_file">Output file to save to.</param>
		/// <param name="response">Page Response context.</param>
		/// <param name="set_time">Thouch time of file.</param>
		/// <param name="quality">Image quality in percent.</param>
		private static void MakeThumbnail(string path, int width, int height, bool save, string output_file, HttpResponse response, DateTime set_time, int quality) {
			FileInfo imageFileInfo;
			Bitmap imageBitMap = null;
			System.Drawing.Image imageThumbnail = null;
			System.Drawing.Image.GetThumbnailImageAbort thumbNailCallBack;

			try {
				imageFileInfo = new FileInfo(path);
				string ext = imageFileInfo.Extension.ToLower();

				// Output nice indexed not rastered image
				if (ext == ".gif" && save) {
					ResizeImage(path, output_file, width, height, quality);
					FileInfo thumbFileInfo = new FileInfo(output_file);
					thumbFileInfo.LastWriteTime = set_time;
					return;
				}

				thumbNailCallBack = new System.Drawing.Image.GetThumbnailImageAbort(ThumbnailCallback);

				imageBitMap = new Bitmap(path);
				imageThumbnail = imageBitMap.GetThumbnailImage(width, height, thumbNailCallBack, IntPtr.Zero);

				ImageFormat format = null;
				string contentType = "";

				if (ext == ".png") {
					contentType = "image/png";
					format = ImageFormat.Png;
				} else if (ext == ".jpg" || ext == ".jpeg") {
					contentType = "image/jpeg";
					format = ImageFormat.Jpeg;
				} else if (ext == ".gif") {
					contentType = "image/gif";
					format = ImageFormat.Gif;
				} else if (ext == ".tif" || ext == ".tiff") {
					contentType = "image/tiff";
					format = ImageFormat.Tiff;
				} else if (ext == ".bmp") {
					contentType = "image/bmp";
					format = ImageFormat.Bmp;
				}

				if (ext == ".jpg" || ext == ".jpeg") {
				    EncoderParameters encps = new EncoderParameters(1);
					EncoderParameter encp = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long) quality);
					encps.Param[0] = encp;

					if (!save) {
						response.ContentType = contentType;
						imageThumbnail.Save(response.OutputStream, format);
					} else {
						imageThumbnail.Save(output_file, GetEncoderInfo("image/jpeg"), encps);
						FileInfo thumbFileInfo = new FileInfo(output_file);
						thumbFileInfo.LastWriteTime = set_time;
					}
				} else {
					if (save) {
						imageThumbnail.Save(output_file, format);
						FileInfo thumbFileInfo = new FileInfo(output_file);
						thumbFileInfo.LastWriteTime = set_time;
					} else {
						response.ContentType = contentType;
						imageThumbnail.Save(response.OutputStream, format);
					}
				}

				imageThumbnail.Dispose();
				imageBitMap.Dispose();
			} catch (Exception ex) {
				if (imageBitMap != null)
					imageBitMap.Dispose();

				if (imageThumbnail != null)
					imageThumbnail.Dispose();

				// Throw it again
				throw new Exception("Image thumbnail failed.", ex);
			}
		}

	 	/// <summary>
	 	/// Resizes image.
	 	/// </summary>
	 	/// <param name="input_file">File to resize.</param>
	 	/// <param name="output_file">File to output to.</param>
	 	/// <param name="width">Width of new image</param>
	 	/// <param name="height">Height of new image</param>
	 	/// <param name="quality">Image quality in percent.</param>
		public static void ResizeImage(string input_file, string output_file, int width, int height, int quality) {
			FileInfo inputFileInfo, outputFileInfo;
			Rectangle destRect, srcRect;
			Bitmap bitmap = null;
			System.Drawing.Image img = null;

			try {
				// Load image
				inputFileInfo = new FileInfo(input_file);
				outputFileInfo = new FileInfo(output_file);
				string ext = outputFileInfo.Extension.ToLower();

				// Use indexed resize on GIF images
				if (ext == ".gif" && inputFileInfo.Extension.ToLower() == ".gif") {
					bitmap = IndexedImageUtils.IndexedResize(inputFileInfo.FullName, width, height);
					bitmap.Save(outputFileInfo.FullName, ImageFormat.Gif);
					bitmap.Dispose();
					return;
				}

				img = System.Drawing.Image.FromFile(inputFileInfo.FullName);

				// Setup new bitmap
				bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
				bitmap.SetResolution(img.HorizontalResolution, img.VerticalResolution);

				// Draw resized image
				Graphics g = Graphics.FromImage(bitmap);
				destRect = new Rectangle(0, 0, width, height);
				srcRect = new Rectangle(0, 0, img.Width, img.Height);
				g.DrawImage(img, destRect, srcRect, GraphicsUnit.Pixel);
				g.Dispose();
				img.Dispose();

				// Save image
				if (ext == ".png")
					bitmap.Save(outputFileInfo.FullName, ImageFormat.Png);
				else if (ext == ".gif") // Keep this when you uncomment unsafe code
					bitmap.Save(outputFileInfo.FullName, ImageFormat.Gif);
				else if (ext == ".jpg" || ext == ".jpeg") {
				    EncoderParameters encps = new EncoderParameters(1);
					EncoderParameter encp = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long) quality);
					encps.Param[0] = encp;

					bitmap.Save(outputFileInfo.FullName, GetEncoderInfo("image/jpeg"), encps);
				} else if (ext == ".tif" || ext == ".tiff")
					bitmap.Save(outputFileInfo.FullName, ImageFormat.Tiff);
				else if (ext == ".bmp")
					bitmap.Save(outputFileInfo.FullName, ImageFormat.Bmp);

				// Clean up
				bitmap.Dispose();
			} catch (Exception ex) {
				if (bitmap != null)
					bitmap.Dispose();

				if (img != null)
					img.Dispose();

				// Throw it again
				throw new Exception("Image resize failed.", ex);
			}
		}

	 	/// <summary>
	 	/// 
	 	/// </summary>
	 	/// <param name="input_file"></param>
	 	/// <param name="output_file"></param>
	 	/// <param name="rotate_flip"></param>
	 	/// <param name="quality"></param>
	 	public static void RotateFlipImage(string input_file, string output_file, RotateFlipType rotate_flip, int quality) {
			FileInfo inputFileInfo, outputFileInfo;
			System.Drawing.Image img = null;

			try {
				// Load image
				inputFileInfo = new FileInfo(input_file);
				outputFileInfo = new FileInfo(output_file);
				string ext = outputFileInfo.Extension.ToLower();

				img = System.Drawing.Image.FromFile(inputFileInfo.FullName);
				img.RotateFlip(rotate_flip);

				// Save image
				if (ext == ".png")
					img.Save(outputFileInfo.FullName, ImageFormat.Png);
				else if (ext == ".gif") // Keep this when you uncomment unsafe code
					img.Save(outputFileInfo.FullName, ImageFormat.Gif);
				else if (ext == ".jpg" || ext == ".jpeg") {
				    EncoderParameters encps = new EncoderParameters(1);
					EncoderParameter encp = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long) quality);
					encps.Param[0] = encp;

					img.Save(outputFileInfo.FullName, GetEncoderInfo("image/jpeg"), encps);
				} else if (ext == ".tif" || ext == ".tiff")
					img.Save(outputFileInfo.FullName, ImageFormat.Tiff);
				else if (ext == ".bmp")
					img.Save(outputFileInfo.FullName, ImageFormat.Bmp);

				img.Dispose();
			} catch (Exception ex) {
				if (img != null)
					img.Dispose();

				// Throw it again
				throw new Exception("Image resize failed.", ex);
			}
	 	}

		/// <summary>
		/// 	Crop's an image.
		/// </summary>
		/// <param name="input_file">Input file path.</param>
		/// <param name="output_file">Output file path.</param>
		/// <param name="x">Start position x for crop.</param>
		/// <param name="y">Start position y for crop.</param>
		/// <param name="width">Width of crop area.</param>
		/// <param name="height">Height of crop area.</param>
		/// <param name="quality">Image quality in percent.</param>
		public static void CropImage(string input_file, string output_file, int x, int y, int width, int height, int quality) {
			FileInfo inputFileInfo, outputFileInfo;
			Rectangle destRect, srcRect;
			Bitmap bitmap = null;
			System.Drawing.Image img = null;

			try {
				// Load image
				inputFileInfo = new FileInfo(input_file);
				outputFileInfo = new FileInfo(output_file);
				string ext = outputFileInfo.Extension.ToLower();

				// Use indexed crop on GIF images
				if (ext == ".gif") {
					bitmap = IndexedImageUtils.IndexedCrop(inputFileInfo.FullName, x, y, width, height);
					bitmap.Save(outputFileInfo.FullName, ImageFormat.Gif);
					bitmap.Dispose();
					return;
				}

				img = System.Drawing.Image.FromFile(inputFileInfo.FullName);

				bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
				bitmap.SetResolution(img.HorizontalResolution, img.VerticalResolution);

				// Draw resized image
				Graphics g = Graphics.FromImage(bitmap);
				destRect = new Rectangle(0, 0, width, height);
				srcRect = new Rectangle(x, y, width, height);
				g.DrawImage(img, destRect, srcRect, GraphicsUnit.Pixel);
				g.Dispose();
				img.Dispose();

				if (ext == ".png")
					bitmap.Save(outputFileInfo.FullName, ImageFormat.Png);
				else if (ext == ".jpg" || ext == ".jpeg") {
				    EncoderParameters encps = new EncoderParameters(1);
					EncoderParameter encp = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long) quality);
					encps.Param[0] = encp;

					bitmap.Save(outputFileInfo.FullName, GetEncoderInfo("image/jpeg"), encps);
				} else if (ext == ".gif") // Keep this when you uncomment unsafe code
					bitmap.Save(outputFileInfo.FullName, ImageFormat.Gif);
				else if (ext == ".tif" || ext == ".tiff")
					bitmap.Save(outputFileInfo.FullName, ImageFormat.Tiff);
				else if (ext == ".bmp")
					bitmap.Save(outputFileInfo.FullName, ImageFormat.Bmp);

				// Clean up
				bitmap.Dispose();
			} catch (Exception ex) {
				if (bitmap != null)
					bitmap.Dispose();

				if (img != null)
					img.Dispose();

				// Throw it again
				throw new Exception("Image resize failed.", ex);
			}
		}

		/// <summary>
		///  Formats a image based in the input parameter.
		///
		///  Format parameters:
		///  %f - Filename.
		///  %e - Extension.
		///  %w - Image width.
		///  %h - Image height.
		///  %tw - Target width.
		///  %th - Target height.
		///  %ow - Original width.
		///  %oh - Original height.
		///
		///  Examples:
		///  320x240|gif=%f_%w_%h.gif,320x240=%f_%w_%h.%e
		/// </summary>
		/// <param name="path">File name to format.</param>
		/// <param name="format">Format string to process.</param>
		/// <param name="quality">JPEG quality in percent.</param>
		public static void FormatImage(string path, string format, int quality) {
			int targetWidth, targetHeight, newWidth, newHeight;
			string fileName, extension, outPath;
			MediaInfo info = new MediaInfo(path);
			ArrayList actions = new ArrayList();
			double scale;

			foreach (string chunk in format.Split(new char[] {','})) {
				Match chunkMatch = Regex.Match(chunk, @"\s?([^=]+)\s?=(.+)\s?");

				if (chunkMatch.Success) {
					fileName = Path.GetFileNameWithoutExtension(path);
					extension = Path.GetExtension(path).Replace(".", "");
					targetWidth = newWidth = info.Width;
					targetHeight = newHeight = info.Height;

					// Parse all items
					foreach (string item in chunkMatch.Groups[1].Value.Split(new char[] {'|'})) {
						switch (item.ToLower()) {
							case "gif":
							case "jpeg":
							case "jpg":
							case "png":
							case "bmp":
								extension = item;
								break;

							default:
								Match match = Regex.Match(item, @"\s?([0-9*]+)\s?x([0-9*]+)\s?");

								if (match.Success) {
									actions.Add("resize");

									try {
										targetWidth = Convert.ToInt32(match.Groups[1].Value);
									} catch {
										// Ignore
									}

									try {
										targetHeight = Convert.ToInt32(match.Groups[2].Value);
									} catch {
										// Ignore
									}

									if (match.Groups[1].Value == "*") {
										// Width is omitted
										targetWidth = (int) Math.Floor((double) info.Width / (info.Height / targetHeight));
									}

									if (match.Groups[2].Value == "*") {
										// Height is omitted
										targetHeight = (int) Math.Floor((double) info.Height / (info.Width / targetWidth));
									}
								}

								break;
						}
					}

					// Add default action
					if (actions.Count == 0)
						actions.Add("resize");

					// Scale it
					if (targetWidth != info.Width || targetHeight != info.Height) {
						scale = Math.Min(targetWidth / (double) info.Width, targetHeight / (double) info.Height);
						newWidth = scale > 1 ? info.Width : (int) Math.Floor((double) info.Width * scale);
						newHeight = scale > 1 ? info.Height : (int) Math.Floor((double) info.Height * scale);
					}

					// Build output path
					outPath = chunkMatch.Groups[2].Value;
					outPath = outPath.Replace("%f", fileName);
					outPath = outPath.Replace("%e", extension);
					outPath = outPath.Replace("%ow", "" + info.Width);
					outPath = outPath.Replace("%oh", "" + info.Height);
					outPath = outPath.Replace("%tw", "" + targetWidth);
					outPath = outPath.Replace("%th", "" + targetHeight);
					outPath = outPath.Replace("%w", "" + newWidth);
					outPath = outPath.Replace("%h", "" + newHeight);
					outPath = PathUtils.AddTrailingSlash(PathUtils.ToUnixPath(Path.GetDirectoryName(path))) + outPath;
					CreateDirectories(Path.GetDirectoryName(outPath));

					foreach (string action in actions) {
						switch (action) {
							case "resize":
								ResizeImage(path, outPath, newWidth, newHeight, quality);
								break;
						}
					}
				}
			}
		}

		/// <summary>
		///  Delete formats for the specified image based.
		///
		///  Format parameters:
		///  %f - Filename.
		///  %e - Extension.
		///  %w - Image width.
		///  %h - Image height.
		///  %tw - Target width.
		///  %th - Target height.
		///  %ow - Original width.
		///  %oh - Original height.
		///
		///  Examples:
		///  320x240|gif=%f_%w_%h.gif,320x240=%f_%w_%h.%e
		/// </summary>
		/// <param name="path">File name to format.</param>
		/// <param name="format">Format string to process.</param>
		public static void DeleteFormatImages(string path, string format) {
			int targetWidth, targetHeight, newWidth, newHeight;
			string fileName, extension, outPath;
			MediaInfo info;
			double scale;

			if (!File.Exists(path))
				return;

			info = new MediaInfo(path);
			
			foreach (string chunk in format.Split(new char[] {','})) {
				Match chunkMatch = Regex.Match(chunk, @"\s?([^=]+)\s?=(.+)\s?");

				if (chunkMatch.Success) {
					fileName = Path.GetFileNameWithoutExtension(path);
					extension = Path.GetExtension(path).Replace(".", "");
					targetWidth = newWidth = info.Width;
					targetHeight = newHeight = info.Height;

					// Parse all items
					foreach (string item in chunkMatch.Groups[1].Value.Split(new char[] {'|'})) {
						switch (item.ToLower()) {
							case "gif":
							case "jpeg":
							case "jpg":
							case "png":
							case "bmp":
								extension = item;
								break;

							default:
								Match match = Regex.Match(item, @"\s?([0-9\*]+)\s?x([0-9\*]+)\s?");

								if (match.Success) {
									try {
										targetWidth = Convert.ToInt32(match.Groups[1].Value);
									} catch {
										// Ignore
									}

									try {
										targetHeight = Convert.ToInt32(match.Groups[2].Value);
									} catch {
										// Ignore
									}

									try {
										if (match.Groups[1].Value == "*") {
											// Width is omitted
											targetWidth = (int) Math.Floor((double) info.Width / (info.Height / targetHeight));
										}

										if (match.Groups[2].Value == "*") {
											// Height is omitted
											targetHeight = (int) Math.Floor((double) info.Height / (info.Width / targetWidth));
										}
									} catch {
										// Ignore
									}
								}

								break;
						}
					}

					// Scale it
					if (targetWidth != info.Width || targetHeight != info.Height) {
						scale = Math.Min(targetWidth / (double) info.Width, targetHeight / (double) info.Height);
						newWidth = scale > 1 ? info.Width : (int) Math.Floor((double) info.Width * scale);
						newHeight = scale > 1 ? info.Height : (int) Math.Floor((double) info.Height * scale);
					}

					// Build output path
					outPath = chunkMatch.Groups[2].Value;
					outPath = outPath.Replace("%f", fileName);
					outPath = outPath.Replace("%e", extension);
					outPath = outPath.Replace("%ow", "" + info.Width);
					outPath = outPath.Replace("%oh", "" + info.Height);
					outPath = outPath.Replace("%tw", "" + targetWidth);
					outPath = outPath.Replace("%th", "" + targetHeight);
					outPath = outPath.Replace("%w", "" + newWidth);
					outPath = outPath.Replace("%h", "" + newHeight);
					outPath = PathUtils.AddTrailingSlash(PathUtils.ToUnixPath(Path.GetDirectoryName(path))) + outPath;

					if (File.Exists(outPath))
						File.Delete(outPath);
				}
			}
		}
		
		private static void CreateDirectories(string path) {
			ArrayList paths = new ArrayList();
			DirectoryInfo dir = new DirectoryInfo(path);

			paths.Add(dir);

			while ((dir = dir.Parent) != null)
				paths.Add(dir);

			paths.Reverse();

			foreach (DirectoryInfo dirInfo in paths) {
				if (!dirInfo.Exists)
					dirInfo.Create();
			}
		}
		
		/// <summary>
		/// Returns the image codec with the given mime type
		/// </summary>
		private static ImageCodecInfo GetEncoderInfo(string mime_type) {
			ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

			for (int i=0; i<codecs.Length; i++) {
				if (codecs[i].MimeType == mime_type)
					return codecs[i];
			}

			return null;
		}

		/// <summary>
	 	///  Some stupid thing for thumbnail generation.
	 	/// </summary>
		private static bool ThumbnailCallback() {
			return false;
		}
	}
}
