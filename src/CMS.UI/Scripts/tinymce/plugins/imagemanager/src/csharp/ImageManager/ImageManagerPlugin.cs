/*
 * $Id: ImageManagerPlugin.cs 799 2010-04-06 14:00:52Z joakim $
 *
 * Copyright © 2007, Moxiecode Systems AB, All rights reserved. 
 */

using System;
using System.IO;
using System.Web;
using System.Drawing;
using System.Collections;
using System.Collections.Specialized;
using Moxiecode.Manager;
using Moxiecode.Manager.Utils;
using Moxiecode.ImageManager.Utils;
using Moxiecode.Manager.FileSystems;

namespace Moxiecode.ImageManager {
	/// <summary>
	/// Description of ImageManager.
	/// </summary>
	public class ImageManagerPlugin : Plugin {
		/// <summary></summary>
		public ImageManagerPlugin() {
		}

		/// <summary>
		///  
		/// </summary>
		public override string Prefix {
			get {
				return "im";
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="man"></param>
		/// <param name="prefix"></param>
		/// <returns></returns>
		public override bool OnPreInit(ManagerEngine man, string prefix) {
			if (prefix == "im")
				man.Config = ((ManagerConfig) man.ResolveConfig("Moxiecode.ImageManager.ImageManagerPlugin", "ImageManagerPlugin")).Clone();

			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="man"></param>
		/// <param name="cmd"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		public override object OnRPC(ManagerEngine man, string cmd, Hashtable input) {
			switch (cmd) {
				case "getMediaInfo":
					return this.GetMediaInfo(man, input);

				case "cropImage":
					return this.CropImage(man, input);

				case "resizeImage":
					return this.ResizeImage(man, input);

				case "rotateImage":
					return this.RotateImage(man, input);

				case "flipImage":
					return this.FlipImage(man, input);

				case "saveImage":
					return this.SaveImage(man, input);
			}

			return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="man"></param>
		/// <param name="cmd"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		public override bool OnStream(ManagerEngine man, string cmd, NameValueCollection input) {
			switch (cmd) {
				case "thumb":
					return this.StreamThumb(man, input);
			}

			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="man"></param>
		/// <param name="file"></param>
		/// <param name="type"></param>
		/// <param name="info"></param>
		/// <returns></returns>
		public override bool OnCustomInfo(ManagerEngine man, IFile file, string type, Hashtable info) {
			string ext = PathUtils.GetExtension(file.Name).ToLower();

			info["thumbnail"] = false;

			switch (ext) {
				case "gif":
				case "jpeg":
				case "jpg":
				case "png":
					MediaInfo imageSize = new MediaInfo(file.AbsolutePath);
					ManagerConfig config = file.Config;
					int thumbWidth, thumbHeight;
					double scale;

					// Constrain proportions
					scale = Math.Min(config.GetInt("thumbnail.width", 90) / (double) imageSize.Width, config.GetInt("thumbnail.height", 90) / (double) imageSize.Height);

					if (config.Get("thumbnail.scale_mode", "percentage") == "percentage") {
						thumbWidth = scale > 1 ? imageSize.Width : (int) Math.Floor(imageSize.Width * scale);
						thumbHeight = scale > 1 ? imageSize.Height : (int) Math.Floor(imageSize.Height * scale);
					} else {
						thumbWidth = config.GetInt("thumbnail.width", 90);
						thumbHeight = config.GetInt("thumbnail.height", 90);
					}

					info["width"] = imageSize.Width;
					info["height"] = imageSize.Height;
					info["editable"] = true;
					info["twidth"] = thumbWidth;
					info["theight"] = thumbHeight;
					info["thumbnail"] = true;

					// Get thumbnail URL
					if (type == "insert") {
						IFile thumbFile = man.GetFile(file.Parent + "/" + config.Get("thumbnail.folder", "mcith") + "/" + config.Get("thumbnail.prefix", "mcith") + file.Name);

						if (thumbFile.Exists)
							info["thumbnail_url"] = man.ConvertPathToURI(thumbFile.AbsolutePath);
					}

					break;
			}

			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="man"></param>
		/// <param name="action"></param>
		/// <param name="file1"></param>
		/// <param name="file2"></param>
		/// <returns></returns>
		public override bool OnBeforeFileAction(ManagerEngine man, FileAction action, IFile file1, IFile file2) {
			ManagerConfig config;

			if (action == FileAction.Delete) {
				config = file1.Config;

				if (config.GetBool("filesystem.delete_format_images", false)) {
					ImageUtils.DeleteFormatImages(file1.AbsolutePath, config.Get("upload.format"));
					ImageUtils.DeleteFormatImages(file1.AbsolutePath, config.Get("edit.format"));
				}
			}

			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="man"></param>
		/// <param name="action"></param>
		/// <param name="file1"></param>
		/// <param name="file2"></param>
		/// <returns></returns>
		public override bool OnFileAction(ManagerEngine man, FileAction action, IFile file1, IFile file2) {
			IFile thumbnailFolder, thumbnail;
			ManagerConfig config;

			switch (action) {
				case FileAction.Add:
					config = file1.Config;

					if (config.Get("upload.format") != null)
						ImageUtils.FormatImage(file1.AbsolutePath, config.Get("upload.format"), config.GetInt("upload.autoresize_jpeg_quality", 90));

					if (config.GetBool("upload.create_thumbnail", true))
						thumbnail = this.MakeThumb(man, file1);

					if (config.GetBool("upload.autoresize", false)) {
						string ext = PathUtils.GetExtension(file1.Name).ToLower();
						int newWidth, newHeight, configWidth, configHeight;
						double scale;

						// Validate format
						if (ext != "gif" && ext != "jpeg" && ext != "jpg" && ext != "png")
							return true;

						MediaInfo imageInfo = new MediaInfo(file1.AbsolutePath);

						configWidth = config.GetInt("upload.max_width", 1024);
						configHeight = config.GetInt("upload.max_height", 768);

						// Needs scaling?
						if (imageInfo.Width > configWidth || imageInfo.Height > configHeight) {
							scale = Math.Min(configWidth / (double) imageInfo.Width, configHeight / (double) imageInfo.Height);
							newWidth = scale > 1 ? imageInfo.Width : (int) Math.Floor(imageInfo.Width * scale);
							newHeight = scale > 1 ? imageInfo.Height : (int) Math.Floor(imageInfo.Height * scale);

							ImageUtils.ResizeImage(file1.AbsolutePath, file1.AbsolutePath, newWidth, newHeight, config.GetInt("upload.autoresize_jpeg_quality", 90));
						}
					}

					break;

				case FileAction.Delete:
					config = file1.Config;

					if (config.GetBool("thumbnail.delete", true)) {
						thumbnailFolder = man.GetFile(file1.Parent, config["thumbnail.folder"]);
						thumbnail = man.GetFile(thumbnailFolder.AbsolutePath, config.Get("thumbnail.prefix", "mcith") + file1.Name);

						if (thumbnail.Exists)
							thumbnail.Delete();

						// Delete empty thumbnail folder
						if (thumbnailFolder.Exists && thumbnailFolder.ListFiles().Length == 0)
							thumbnailFolder.Delete();
					}
					break;
			}
			
			return true;
		}

		#region private methods

		private ResultSet GetMediaInfo(ManagerEngine man, Hashtable input) {
			ResultSet rs = new ResultSet(new string[]{"name", "path", "url", "size", "type", "created", "modified", "width", "height", "attribs", "next", "prev", "custom"});
			BasicFileFilter fileFilter;
			IFile file, parent;
			IFile[] files;
			string prev, next, attribs, url, ext;
			int width, height;
			bool match;
			Hashtable customInfo = new Hashtable();
			ManagerConfig config;

			if (input["url"] != null)
				input["path"] = man.ConvertURIToPath(new Uri((string) input["url"]).AbsolutePath);

			file = man.GetFile(man.DecryptPath((string) input["path"]));
			config = file.Config;
			parent = file.ParentFile;

			if (parent.IsDirectory) {
				// Setup file filter
				fileFilter = new BasicFileFilter();
				//fileFilter->setDebugMode(true);
				fileFilter.IncludeDirectoryPattern = config["filesystem.include_directory_pattern"];
				fileFilter.ExcludeDirectoryPattern = config["filesystem.exclude_directory_pattern"];
				fileFilter.IncludeFilePattern = config["filesystem.include_file_pattern"];
				fileFilter.ExcludeFilePattern = config["filesystem.exclude_file_pattern"];
				fileFilter.IncludeExtensions = config["filesystem.extensions"];
				fileFilter.OnlyFiles = true;

				// List files
				files = parent.ListFilesFiltered(fileFilter);
			} else
				throw new ManagerException("{#error.file_not_exists}");

			match = false;
			prev = "";
			next = "";

			// Find next and prev
			foreach (IFile curFile in files) {
				if (curFile.AbsolutePath == file.AbsolutePath) {
					match = true;
					continue;
				} else if (!match)
					prev = curFile.AbsolutePath;

				if (match) {
					next = curFile.AbsolutePath;
					break;
				}
			}

			ext = PathUtils.GetExtension(file.Name).ToLower();

			// Input default size?
			MediaInfo size = new MediaInfo(file.AbsolutePath);
			width = size.Width != -1 ? size.Width : 425;
			height = size.Height != -1 ? size.Height : 350;

			// Get custom info
			man.DispatchEvent(EventType.CustomInfo, file, "info", customInfo);

			attribs = (file.CanRead && config.GetBool("filesystem.readable", true) ? "R" : "-") + (file.CanWrite && config.GetBool("filesystem.writable", true) ? "W" : "-");
			url = PathUtils.RemoveTrailingSlash(config["preview.urlprefix"]) + man.ConvertPathToURI(file.AbsolutePath);
			rs.Add(file.Name,
			           man.EncryptPath(file.AbsolutePath),
			           url,
			           file.Length,
			           ext,
			           StringUtils.GetDate(file.CreationDate, config.Get("filesystem.datefmt", "yyyy-MM-dd HH:mm")),
			           StringUtils.GetDate(file.LastModified, config.Get("filesystem.datefmt", "yyyy-MM-dd HH:mm")),
			           width,
			           height,
			           attribs,
			           man.EncryptPath(next),
			           man.EncryptPath(prev),
			           customInfo);

			return rs;
		}

		private ResultSet CropImage(ManagerEngine man, Hashtable input) {
			ResultSet rs = new ResultSet(new string[]{"status", "file", "message"});
			IFile file, targetFile;
			ManagerConfig config;
			string tempName;
			int x, y, w, h;

			file = man.GetFile(man.DecryptPath((string) input["path"]));
			config = file.Config;

			if (!man.IsToolEnabled("edit", config))
				throw new ManagerException("{#error.no_access}");
			
			if (!file.Exists) {
				rs.Add("FAILED", man.EncryptPath(file.AbsolutePath), "{#error.file_not_exists}");
				return rs;
			}

			if (file.Name.IndexOf("mcic_") != 0 && !man.VerifyFile(file, "edit")) {
				rs.Add("FATAL", man.EncryptPath(file.AbsolutePath), man.InvalidFileMsg);
				return rs;
			}

			if (input["temp"] != null && (bool) input["temp"]) {
				tempName = "mcic_" + HttpContext.Current.Session.SessionID + "." + PathUtils.GetExtension(file.Name);

				if (input["target"] != null)
					targetFile = man.GetFile(man.DecryptPath((string) input["target"]), tempName);
				else
					targetFile = man.GetFile(file.Parent, tempName);
			} else
				targetFile = file;

			x = Convert.ToInt32(input["left"]);
			y = Convert.ToInt32(input["top"]);
			w = Convert.ToInt32(input["width"]);
			h = Convert.ToInt32(input["height"]);

			//man.Logger.Debug(x, y, w, h, file.AbsolutePath, targetFile.AbsolutePath);

			try {
				ImageUtils.CropImage(file.AbsolutePath, targetFile.AbsolutePath, x, y, w, h, config.GetInt("edit.jpeg_quality", 90));
				rs.Add("OK", man.EncryptPath(targetFile.AbsolutePath), "{#message.crop_success}");
			} catch (Exception ex) {
				man.Logger.Error(ex.ToString());
				rs.Add("FAILED", man.EncryptPath(targetFile.AbsolutePath), "{#error.crop_failed}");
			}

			return rs;
		}

		private ResultSet ResizeImage(ManagerEngine man, Hashtable input) {
			ResultSet rs = new ResultSet(new string[]{"status", "file", "message"});
			IFile file, targetFile;
			ManagerConfig config;
			string tempName;
			int w, h;

			file = man.GetFile(man.DecryptPath((string) input["path"]));
			config = file.Config;

			if (!man.IsToolEnabled("edit", config))
				throw new ManagerException("{#error.no_access}");
			
			if (!file.Exists) {
				rs.Add("FAILED", man.EncryptPath(file.AbsolutePath), "{#error.file_not_exists}");
				return rs;
			}

			if (file.Name.IndexOf("mcic_") != 0 && !man.VerifyFile(file, "edit")) {
				rs.Add("FATAL", man.EncryptPath(file.AbsolutePath), man.InvalidFileMsg);
				return rs;
			}

			if (input["temp"] != null && (bool) input["temp"]) {
				tempName = "mcic_" + HttpContext.Current.Session.SessionID + "." + PathUtils.GetExtension(file.Name);

				if (input["target"] != null)
					targetFile = man.GetFile(man.DecryptPath((string) input["target"]), tempName);
				else
					targetFile = man.GetFile(file.Parent, tempName);
			} else
				targetFile = file;

			w = Convert.ToInt32(input["width"]);
			h = Convert.ToInt32(input["height"]);

			try {
				ImageUtils.ResizeImage(file.AbsolutePath, targetFile.AbsolutePath, w, h, config.GetInt("edit.jpeg_quality", 90));
				rs.Add("OK", man.EncryptPath(targetFile.AbsolutePath), "{#message.resize_success}");
			} catch (Exception ex) {
				man.Logger.Error(ex.ToString());
				rs.Add("FAILED", man.EncryptPath(targetFile.AbsolutePath), "{#error.resize_failed}");
			}

			return rs;
		}

		private ResultSet FlipImage(ManagerEngine man, Hashtable input) {
			ResultSet rs = new ResultSet(new string[]{"status", "file", "message"});
			IFile file, targetFile;
			ManagerConfig config;
			string tempName;
			RotateFlipType rotateFlipType = RotateFlipType.RotateNoneFlipX;

			file = man.GetFile(man.DecryptPath((string) input["path"]));
			config = file.Config;

			if (!man.IsToolEnabled("edit", config))
				throw new ManagerException("{#error.no_access}");

			if (!file.Exists) {
				rs.Add("FAILED", man.EncryptPath(file.AbsolutePath), "{#error.file_not_exists}");
				return rs;
			}

			if (file.Name.IndexOf("mcic_") != 0 && !man.VerifyFile(file, "edit")) {
				rs.Add("FAILED", man.EncryptPath(file.AbsolutePath), man.InvalidFileMsg);
				return rs;
			}

			if (file.Name.IndexOf("mcic_") != 0 && !man.VerifyFile(file, "edit")) {
				rs.Add("FATAL", man.EncryptPath(file.AbsolutePath), man.InvalidFileMsg);
				return rs;
			}

			if (input["temp"] != null && (bool) input["temp"]) {
				tempName = "mcic_" + HttpContext.Current.Session.SessionID + "." + PathUtils.GetExtension(file.Name);

				if (input["target"] != null)
					targetFile = man.GetFile(man.DecryptPath((string) input["target"]), tempName);
				else
					targetFile = man.GetFile(file.Parent, tempName);
			} else
				targetFile = file;

			if (input["horizontal"] != null && (bool) input["horizontal"])
				 rotateFlipType = RotateFlipType.RotateNoneFlipX;

			if (input["vertical"] != null && (bool) input["vertical"])
				 rotateFlipType = RotateFlipType.RotateNoneFlipY;

			try {
				ImageUtils.RotateFlipImage(file.AbsolutePath, targetFile.AbsolutePath, rotateFlipType, config.GetInt("edit.jpeg_quality", 90));
				rs.Add("OK", man.EncryptPath(targetFile.AbsolutePath), "{#message.flip_success}");
			} catch (Exception ex) {
				man.Logger.Error(ex.ToString());
				rs.Add("FAILED", man.EncryptPath(targetFile.AbsolutePath), "{#error.flip_failed}");
			}

			return rs;
		}

		private ResultSet RotateImage(ManagerEngine man, Hashtable input) {
			ResultSet rs = new ResultSet(new string[]{"status", "file", "message"});
			IFile file, targetFile;
			ManagerConfig config;
			string tempName;
			RotateFlipType rotateFlipType = RotateFlipType.Rotate90FlipNone;

			file = man.GetFile(man.DecryptPath((string) input["path"]));
			config = file.Config;

			if (!man.IsToolEnabled("edit", config))
				throw new ManagerException("{#error.no_access}");

			if (!file.Exists) {
				rs.Add("FAILED", man.EncryptPath(file.AbsolutePath), "{#error.file_not_exists}");
				return rs;
			}

			if (file.Name.IndexOf("mcic_") != 0 && !man.VerifyFile(file, "edit")) {
				rs.Add("FATAL", man.EncryptPath(file.AbsolutePath), man.InvalidFileMsg);
				return rs;
			}

			if (input["temp"] != null && (bool) input["temp"]) {
				tempName = "mcic_" + HttpContext.Current.Session.SessionID + "." + PathUtils.GetExtension(file.Name);

				if (input["target"] != null)
					targetFile = man.GetFile(man.DecryptPath((string) input["target"]), tempName);
				else
					targetFile = man.GetFile(file.Parent, tempName);
			} else
				targetFile = file;

			switch (Convert.ToInt32(input["angle"])) {
				case 90:
					rotateFlipType = RotateFlipType.Rotate90FlipNone;
					break;

				case 180:
					rotateFlipType = RotateFlipType.Rotate180FlipNone;
					break;

				case 270:
					rotateFlipType = RotateFlipType.Rotate270FlipNone;
					break;
			}

			try {
				ImageUtils.RotateFlipImage(file.AbsolutePath, targetFile.AbsolutePath, rotateFlipType, config.GetInt("edit.jpeg_quality", 90));
				rs.Add("OK", man.EncryptPath(targetFile.AbsolutePath), "{#message.rotate_success}");
			} catch (Exception ex) {
				man.Logger.Error(ex.ToString());
				rs.Add("FAILED", man.EncryptPath(targetFile.AbsolutePath), "{#error.rotate_failed}");
			}

			return rs;
		}

		private ResultSet SaveImage(ManagerEngine man, Hashtable input) {
			ResultSet rs = new ResultSet(new string[]{"status", "file", "message"});
			IFile file, targetFile;
			ManagerConfig config;

			file = man.GetFile(man.DecryptPath((string) input["path"]));
			config = file.Config;

			if (!man.IsToolEnabled("edit", config))
				throw new ManagerException("{#error.no_access}");

			if (config.GetBool("general.demo", false)) {
				rs.Add("FAILED", man.EncryptPath(file.AbsolutePath), "{#error.demo}");
				this.Cleanup(man, file.ParentFile);
				return rs;
			}

			if (file.Name.IndexOf("mcic_") != 0 && !man.VerifyFile(file, "edit")) {
				rs.Add("FAILED", man.EncryptPath(file.AbsolutePath), man.InvalidFileMsg);
				this.Cleanup(man, file.ParentFile);
				return rs;
			}

			targetFile = man.GetFile(file.Parent, (string) input["target"]);
			if (file.AbsolutePath != targetFile.AbsolutePath) {
				
				string fileExt = PathUtils.GetExtension(file.Name).ToLower();
				string targetExt = PathUtils.GetExtension(targetFile.Name).ToLower();
				
				if (fileExt != targetExt) {
					rs.Add("FAILED", man.EncryptPath(targetFile.AbsolutePath), "{#error.invalid_filename}");
					this.Cleanup(man, file.ParentFile);
					return rs;
				}
				
				if (config.GetBool("filesystem.delete_format_images", false))
					ImageUtils.DeleteFormatImages(targetFile.AbsolutePath, config.Get("edit.format"));

				if (targetFile.Exists)
					targetFile.Delete();

				file.RenameTo(targetFile);

				if (config.Get("edit.format") != null)
					ImageUtils.FormatImage(targetFile.AbsolutePath, config.Get("edit.format"), config.GetInt("edit.jpeg_quality", 90));

				this.Cleanup(man, file.ParentFile);
			}

			rs.Add("OK", man.EncryptPath(file.AbsolutePath), "{#message.save_success}");

			return rs;
		}

		private void Cleanup(ManagerEngine man, IFile dir) {
			IFile[] files = dir.ListFiles();

			foreach (IFile file in files) {
				if (file.Name.StartsWith("mcic_") && file.LastModified < DateTime.Now.AddMinutes(-30))
					file.Delete();
			}
		}

		private IFile MakeThumb(ManagerEngine man, IFile file) {
			ManagerConfig config = file.Config;
			IFile thumbFile;
			int thumbWidth, thumbHeight;
			int configWidth, configHeight;
			double scale;
			MediaInfo thumbSize = new MediaInfo();

			// Is not enabled
			if (!config.GetBool("thumbnail.enabled", true))
				return file;

			// Setup thumbnail path
			IFile thumbDir = man.GetFile(file.Parent, config.Get("thumbnail.folder", "mcith"));
			if (!thumbDir.Exists)
				thumbDir.MkDir();

			configWidth = config.GetInt("thumbnail.width", 90);
			configHeight = config.GetInt("thumbnail.height", 90);

			// Make thumbnail
			thumbFile = man.GetFile(thumbDir.AbsolutePath, config.Get("thumbnail.prefix", "mcith") + file.Name);
			MediaInfo imageSize = new MediaInfo(file.AbsolutePath);

			// Need to scale?
			if (imageSize.Width < configWidth && imageSize.Height < configHeight)
				return file;

			// To large
			if (imageSize.Width > config.GetInt("thumbnail.max_width", 65535) || imageSize.Height > config.GetInt("thumbnail.max_height", 65535))
				return file;

			// Constrain proportions
			scale = Math.Min(configWidth / (double) imageSize.Width, configHeight / (double) imageSize.Height);

			if (config.Get("thumbnail.scale_mode", "percentage") == "percentage") {
				thumbWidth = scale > 1 ? imageSize.Width : (int) Math.Floor(imageSize.Width * scale);
				thumbHeight = scale > 1 ? imageSize.Height : (int) Math.Floor(imageSize.Height * scale);
			} else {
				thumbWidth = config.GetInt("thumbnail.width", 90);
				thumbHeight = config.GetInt("thumbnail.height", 90);
			}

			if (thumbFile.Exists)
				thumbSize = new MediaInfo(thumbFile.AbsolutePath);

			if (!thumbFile.Exists || thumbSize.Width != thumbWidth || thumbSize.Height != thumbHeight || file.LastModified != thumbFile.LastModified)
				ImageUtils.MakeThumbnail(file.AbsolutePath, thumbFile.AbsolutePath, thumbWidth, thumbHeight, file.LastModified, config.GetInt("thumbnail.jpeg_quality", 75));

			return thumbFile;
		}

		private bool StreamThumb(ManagerEngine man, NameValueCollection input) {
			HttpContext context = HttpContext.Current;
			string path, contentType;
			ManagerConfig config;
			IFile file;

			// Get input
			path = man.DecryptPath((string) input["path"]);

			// Find mime type
			contentType = man.MimeTypes[PathUtils.GetExtension(path).ToLower()];
			if (contentType != null)
				context.Response.ContentType = contentType;

			// Get and stream file
			file = man.GetFile(path);
			config = file.Config;
			try {
				path = MakeThumb(man, file).AbsolutePath;
			} catch (Exception) {
				// Ignore
			}

			// Stream thumbnail
			if (file is LocalFile)
				context.Response.WriteFile(path);
			else
				IOUtils.StreamFromTo(file.Open(FileMode.Open), context.Response.OutputStream, 1024);

			return false;
		}

		#endregion
	}
}
