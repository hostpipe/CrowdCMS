/*
 * $Id: CorePlugin.cs 875 2012-06-12 12:06:15Z spocke $
 *
 * Copyright © 2007, Moxiecode Systems AB, All rights reserved. 
 */

using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using Moxiecode.Manager.FileSystems;
using Moxiecode.Manager.Utils;
using System.Web;

namespace Moxiecode.Manager {
	/// <summary>
	///  LocalFileFactroy, generates new file instances.
	/// </summary>
	public class RootFileFactory : IFileFactory {
		/// <summary>
		///  Returns a file based on path, child path and type.
		/// </summary>
		/// <param name="man">Manager engine reference.</param>
		/// <param name="path">Absolute path to file or directory.</param>
		/// <param name="child">Child path inside directory or empty string.</param>
		/// <param name="type">File type, force it to be a file or directory.</param>
		public IFile GetFile(ManagerEngine man, string path, string child, FileType type) {
			return new RootFile(man, path, child, type);
		}
	}

	/// <summary>
	///  Root file system file.
	/// </summary>
	public class RootFile : BaseFile, IFile {
		/// <summary>
		///  
		/// </summary>
		/// <param name="man"></param>
		/// <param name="path"></param>
		/// <param name="child"></param>
		/// <param name="type"></param>
		public RootFile(ManagerEngine man, string path, string child, FileType type) : base(man, path, child, type) {
		}

		/// <summary>
		///  
		/// </summary>
		/// <param name="filter"></param>
		/// <returns></returns>
		public new IFile[] ListFilesFiltered(IFileFilter filter) {
			ArrayList files = new ArrayList();

			foreach (string rootPath in base.manager.RootPaths) {
				IFile rootFile = this.manager.GetFile(rootPath);

				if (!rootFile.Exists)
					throw new Exception("Root path: " + rootPath + ", does not exists.");

				files.Add(rootFile);
			}

			return (IFile[]) files.ToArray(typeof(IFile));
		}
	}
	
	/// <summary>
	/// Description of ManagerCore.
	/// </summary>
	public class CorePlugin : Plugin {
		/// <summary>
		///  
		/// </summary>
		public CorePlugin() {
		}

		/// <summary>
		///  
		/// </summary>
		/// <param name="man"></param>
		/// <returns></returns>
		public override bool OnInit(ManagerEngine man) {
			string fs = man.Config.Get("filesystem", "LocalFileFactory");
			IFileFactory factory = null;

			// Register local file system factroy
			if (fs != "LocalFileFactory") {
				if (fs.IndexOf('.') == -1)
					factory = (IFileFactory) InstanceFactory.CreateInstance("Moxiecode.Manager.FileSystems." + fs);

				if (factory == null)
					factory = (IFileFactory) InstanceFactory.CreateInstance(fs);

				man.FileSystems["file"] = factory;
			} else
				man.FileSystems["file"] = new LocalFileFactory();

			man.FileSystems["root"] = new RootFileFactory();

			return true; // Pass to next
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
				case "deleteFiles":
					return this.DeleteFiles(man, input);

				case "listFiles":
					return this.ListFiles(man, cmd, input);

				case "createDirs":
					return this.CreateDirs(man, input);

				case "getConfig":
					return this.GetConfig(man, input);

				case "insertFiles":
					return this.InsertFiles(man, input);

				case "keepAlive":
					return this.KeepAlive(man, input);

				case "loopBack":
					return this.LoopBack(man, input);
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
			HttpContext context = HttpContext.Current;
			string path, contentType;
			ManagerConfig config;
			IFile file;

			switch (cmd) {
				case "streamFile":
					// Get input
					path = man.DecryptPath(input["path"]);

					// Get and stream file
					file = man.GetFile(path);
					config = file.Config;

					if (file is LocalFile) {
						string url = PathUtils.RemoveTrailingSlash(config["preview.urlprefix"]) + man.ConvertPathToURI(file.AbsolutePath) + config["preview.urlsuffix"];

						// Pass through rnd
						if (input["rnd"] != null)
							context.Response.Redirect(url + (url.IndexOf('?') == -1 ? ("?rnd=" + input["rnd"]) : ("&rnd" + input["rnd"])), true);

						context.Response.Redirect(url, true);
					} else {
						// Verify that we can stream the file
						if (!man.VerifyFile(file, "stream"))
							throw new ManagerException("Requested resource could not be found. Or access was denied.");

						// Set content type
						contentType = man.MimeTypes[PathUtils.GetExtension(path).ToLower()];
						if (contentType != null)
							context.Response.ContentType = contentType;

						IOUtils.StreamFromTo(file.Open(FileMode.Open), context.Response.OutputStream, 1024);
					}

					break;

				case "download":
					// Get input
					path = man.DecryptPath(input["path"]);

					// Get and stream file
					file = man.GetFile(path);
					config = file.Config;

					// Verify that we can stream the file
					if (!man.VerifyFile(file, "download"))
						throw new ManagerException("Requested resource could not be found. Or access was denied.");

					// Set content type
					//contentType = man.MimeTypes[PathUtils.GetExtension(path).ToLower()];
					context.Response.ContentType = "application/octet-stream";
					context.Response.AddHeader("Content-Disposition:", "attachment; filename=\"" + file.Name + "\"");
					//IOUtils.StreamFromTo(file.Open(FileMode.Open), context.Response.OutputStream, 4096);

					byte[] buff = new byte[4096];
					int len;
					Stream inStream = file.Open(FileMode.Open), outStream = context.Response.OutputStream;

					try {
						while ((len = inStream.Read(buff, 0, buff.Length)) > 0) {
							outStream.Write(buff, 0, len);
							outStream.Flush();
							context.Response.Flush();
						}
					} finally {
						if (inStream != null)
							inStream.Close();

						if (outStream != null)
							outStream.Close();
					}

					break;
			}

			// Devkit commands
			switch (cmd) {
				case "viewServerInfo":
					if (!man.Config.GetBool("general.debug", false))
						throw new ManagerException("You have to enable debugging in config by setting general.debug to true.");

					context.Response.ContentType = "text/html";
					context.Trace.IsEnabled = true;

					context.Response.Write("<pre># Config from Web.config\r\n\r\n");

					foreach (string key in man.Config.Keys)
						context.Response.Write(key + "=" + man.Config[key] + "\r\n");

					context.Response.Write("</pre>");

					break;

				case "downloadServerInfo":
					if (!man.Config.GetBool("general.debug", false))
						throw new ManagerException("You have to enable debugging in config by setting general.debug to true.");

					context.Response.AppendHeader("Content-Disposition", "attachment; filename=trace.htm");
					context.Response.ContentType = "text/html";
					context.Trace.IsEnabled = true;

					context.Response.Write("<pre># Config from Web.config\r\n\r\n");

					foreach (string key in man.Config.Keys)
						context.Response.Write(key + "=" + man.Config[key] + "\r\n");

					context.Response.Write("</pre>");

					break;

				case "viewLog":
					if (!man.Config.GetBool("general.debug", false))
						throw new ManagerException("You have to enable debugging in config by setting general.debug to true.");

					if (input["level"] == "debug") {
						if (File.Exists(man.MapPath("logs/debug.log")))
							context.Response.WriteFile(man.MapPath("logs/debug.log"));
					} else {
						if (File.Exists(man.MapPath("logs/error.log")))
							context.Response.WriteFile(man.MapPath("logs/error.log"));
					}

					break;

				case "clearLog":
					if (!man.Config.GetBool("general.debug", false))
						throw new ManagerException("You have to enable debugging in config by setting general.debug to true.");

					if (input["level"] == "debug")
						File.Delete(man.MapPath("logs/debug.log"));
					else
						File.Delete(man.MapPath("logs/error.log"));

					context.Response.Write("Log cleared.");

					break;
			}
			
			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="man"></param>
		/// <param name="cmd"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		public override object OnUpload(ManagerEngine man, string cmd, NameValueCollection input) {
			if (cmd == "upload") {
				ResultSet rs = new ResultSet(new string[]{"status", "file", "message"});
				HttpRequest request = HttpContext.Current.Request;
				ManagerConfig config;
				IFile targetDir;

				// Setup
				targetDir = man.GetFile(man.DecryptPath(input["path"]));
				config = targetDir.Config;

				// Check access agains config
				if (config.GetBool("general.demo", false)) {
					rs.Add("FAILED", man.EncryptPath(targetDir.AbsolutePath), "{#error.demo}");
					return rs;
				}

				if (!config.GetBool("filesystem.writable", true)) {
					rs.Add("FAILED", man.EncryptPath(targetDir.AbsolutePath), "{#error.no_write_access}");
					return rs;
				}

				if (config.HasItem("general.disabled_tools", "upload")) {
					rs.Add("FAILED", man.EncryptPath(targetDir.AbsolutePath), "{#error.demo}");
					return rs;
				}

				// Handle flash upload
				if (request["html4"] == null) {
					this.HandleChunkedUpload(man, targetDir, request, request["name"], rs);
					return rs;
				}

				// Ok lets check the files array out.
				for (int i=0; request.Files["file" + i] != null; i++)
					this.HandleUploadedFile(man, targetDir, request.Files["file" + i], input["name" + i], rs);

				return rs;
			}

			return null;
		}

		#region private methods

		private ResultSet DeleteFiles(ManagerEngine man, Hashtable input) {
			ResultSet rs = new ResultSet(new string[]{"status", "file", "message"});
			ManagerConfig config;
			IFile file;

			for (int i=0; input["path" + i] != null; i++) {
				file = man.GetFile(man.DecryptPath((string) input["path" + i]));
				config = file.Config;

				if (!man.IsToolEnabled("delete", config))
					throw new ManagerException("{#error.no_access}");

				// File exists
				if (!file.Exists) {
					rs.Add("FAILED", man.EncryptPath(file.AbsolutePath), "{#error.file_not_exists}");
					continue;
				}

				if (config.GetBool("general.demo", false)) {
					rs.Add("FAILED", man.EncryptPath(file.AbsolutePath), "{#error.demo}");
					continue;
				}

				if (!man.VerifyFile(file, "delete")) {
					rs.Add("FAILED", man.EncryptPath(file.AbsolutePath), man.InvalidFileMsg);
					continue;
				}

				if (!file.CanWrite) {
					rs.Add("FAILED", man.EncryptPath(file.AbsolutePath), "{#error.no_write_access}");
					continue;
				}

				if (!config.GetBool("filesystem.writable", true)) {
					rs.Add("FAILED", man.EncryptPath(file.AbsolutePath), "{#error.no_write_access}");
					continue;
				}

				if (file.Delete(config.GetBool("filesystem.delete_recursive", true)))
					rs.Add("OK", man.EncryptPath(file.AbsolutePath), "{#message.delete_success}");
				else
					rs.Add("FAILED", man.EncryptPath(file.AbsolutePath), "{#error.delete_failed}");
			}

			return rs;
		}

		private void HandleChunkedUpload(ManagerEngine man, IFile target_dir, HttpRequest req, string file_name, ResultSet rs) {
			long maxSizeBytes;
			IFile file;
			ManagerConfig config;
			int chunk = req["chunk"] == null ? 0 : Convert.ToInt32(req["chunk"]), chunks = req["chunks"] == null ? 1 : Convert.ToInt32(req["chunks"]);

			try {
				maxSizeBytes = StringUtils.GetSizeLong(target_dir.Config.Get("upload.maxsize", "10mb"), "10mb");
				file = man.GetFile(target_dir.AbsolutePath, file_name, FileType.File);

				config = file.Config;

				if (!man.IsToolEnabled("upload", config))
					throw new ManagerException("{#error.no_access}");

				if (!man.VerifyFile(file, "upload")) {
					rs.Add("FILTER_ERROR", man.EncryptPath(file.AbsolutePath), man.InvalidFileMsg);
					return;
				}

				// File exists
				if (file.Exists && chunk == 0) {
					if (!config.GetBool("upload.overwrite", false)) {
						rs.Add("OVERWRITE_ERROR", man.EncryptPath(file.AbsolutePath), "{#error.file_exists}");
						return;
					} else if (file.Exists)
						file.Delete();
				}

				Stream outStream = file.Open(chunk == 0 ? FileMode.Create : FileMode.Append);
				byte[] buff = new byte[1024];
				int len;

				if (req.Files["file"] != null) {
					while ((len = req.Files["file"].InputStream.Read(buff, 0, buff.Length)) > 0)
						outStream.Write(buff, 0, len);
				} else {
					while ((len = req.InputStream.Read(buff, 0, buff.Length)) > 0)
						outStream.Write(buff, 0, len);
				}

				outStream.Close();

				// To large
				if (file.Length > maxSizeBytes) {
					rs.Add("SIZE_ERROR", man.EncryptPath(file.AbsolutePath), "{#error.error_to_large}");
					file.Delete();
					return;
				}

				rs.Add("OK", man.EncryptPath(file.AbsolutePath), "{#message.upload_ok}");

				if (chunk == chunks - 1)
					file.ImportFile(file.AbsolutePath); // Dispatch add
			} catch (Exception e) {
				man.Logger.Error(e.ToString());
				rs.Add("FAILED", file_name, "{#error.upload_failed}");
			}
		}
		
		private void HandleUploadedFile(ManagerEngine man, IFile target_dir, HttpPostedFile uploaded_file, string file_name, ResultSet rs) {
			long maxSizeBytes;
			IFile file;
			ManagerConfig config;

			try {
				maxSizeBytes = StringUtils.GetSizeLong(target_dir.Config.Get("upload.maxsize", "10mb"), "10mb");

				if (file_name != null)
					file = man.GetFile(target_dir.AbsolutePath, file_name + "." + PathUtils.GetExtension(uploaded_file.FileName), FileType.File);
				else
					file = man.GetFile(target_dir.AbsolutePath, uploaded_file.FileName, FileType.File);

				config = file.Config;

				if (!man.IsToolEnabled("upload", config))
					throw new ManagerException("{#error.no_access}");

				if (!man.VerifyFile(file, "upload")) {
					rs.Add("FILTER_ERROR", man.EncryptPath(file.AbsolutePath), man.InvalidFileMsg);
					return;
				}

				// To large
				if (uploaded_file.ContentLength > maxSizeBytes) {
					rs.Add("SIZE_ERROR", man.EncryptPath(file.AbsolutePath), "{#error.error_to_large}");
					return;
				}
				
				// File exists
				if (file.Exists) {
					if (!config.GetBool("upload.overwrite", false)) {
						rs.Add("OVERWRITE_ERROR", man.EncryptPath(file.AbsolutePath), "{#error.file_exists}");
						return;
					} else
						file.Delete();
				}

				if (file is LocalFile) {
					uploaded_file.SaveAs(file.AbsolutePath);
					file.ImportFile(file.AbsolutePath); // Dispatch add
				} else {
					Stream outStream = file.Open(FileMode.Create);
					byte[] buff = new byte[1024];
					int len;

					while ((len = uploaded_file.InputStream.Read(buff, 0, buff.Length)) > 0)
						outStream.Write(buff, 0, len);

					outStream.Close();
				}
				
				rs.Add("OK", man.EncryptPath(file.AbsolutePath), "{#message.upload_ok}");
			} catch (Exception e) {
				man.Logger.Error(e.ToString());
				rs.Add("FAILED", file_name, "{#error.upload_failed}");
			}
		}

		private ResultSet ListFiles(ManagerEngine man, string cmd, Hashtable input) {
			ResultSet rs = new ResultSet(new string[] {"name","path","size","type","created","modified","attribs","custom"});
			Hashtable customInfo;
			ArrayList files = new ArrayList();
			BasicFileFilter filter;
			IFile listDir;
			ManagerConfig config = man.Config;
			bool onlyDirs, onlyFiles, remember, hasParent = false;
			string type, attribs, path, rootPath, configPrefixes, name, tmpPath;
			int pages, pageSize;
			HttpRequest req = HttpContext.Current.Request;
			HttpResponse resp = HttpContext.Current.Response;

			// Handle remember_last_path state
			if (input["remember_last_path"] != null) {
				// URL specified
				if (((string) input["path"]) == "{default}") {
					if (input["url"] != null && (string) input["url"] != "")
						input["path"] = man.ConvertURIToPath((string) input["url"]);
				}

				path = (string) input["path"];

				if (input["remember_last_path"] is bool)
					remember = (bool) input["remember_last_path"];
				else if (((string) input["remember_last_path"]) != "auto")
					remember = StringUtils.CheckBool((string) input["remember_last_path"]);
				else
					remember = config.GetBool("general.remember_last_path", false);

				// Get/set cookie
				if (remember) {
					if (path == "{default}" && req.Cookies["MCManager_" + man.Prefix + "_lastPath"] != null) {
						tmpPath = req.Cookies["MCManager_" + man.Prefix + "_lastPath"].Value;

						if (tmpPath != null && man.GetFSFromPath(tmpPath) == "file")
							input["path"] = tmpPath;
					} else {
						HttpCookie cookie = new HttpCookie("MCManager_" + man.Prefix + "_lastPath");

						cookie.Expires = DateTime.Now.AddDays(30);
						cookie.Value = path;

						resp.Cookies.Remove("MCManager_" + man.Prefix + "_lastPath");

						if (man.GetFSFromPath(path) == "file")
							resp.Cookies.Add(cookie);
					}
				}
			}

			path = man.DecryptPath((string) input["path"]);
			rootPath = man.DecryptPath((string) input["root_path"]);
			onlyDirs = input["only_dirs"] != null && (bool) input["only_dirs"];
			onlyFiles = input["only_files"] != null && (bool) input["only_files"];
			configPrefixes = (string) input["config"];

			if (man.GetFSFromPath(path) == "file" && !man.VerifyPath(path))
				path = man.Config.Get("filesystem.path");

			// Move path into rootpath
			if (rootPath != null && !PathUtils.IsChildPath(rootPath, path) && man.GetFSFromPath(path) == "file")
				path = rootPath;

			listDir = man.GetFile(path);

			// Use default path instead
			if (!listDir.Exists) {
				path = config["filesystem.path"];
				listDir = man.GetFile(path);
			}

			rs.Header["path"] = man.EncryptPath(path);
			rs.Header["visual_path"] = man.ToVisualPath(path, rootPath);
			rs.Header["attribs"] = (listDir.CanRead ? "R" : "-") + (listDir.CanWrite ? "W" : "-");

			// List files
			if (listDir.IsDirectory) {
				config = listDir.Config;

				// Return part of the config
				if (configPrefixes != null)
					rs.Config = man.GetJSConfig(config, configPrefixes);

				// Verify filesystem config
				filter = new BasicFileFilter();
				filter.IncludeDirectoryPattern = config["filesystem.include_directory_pattern"];
				filter.ExcludeDirectoryPattern = config["filesystem.exclude_directory_pattern"];
				filter.IncludeFilePattern = config["filesystem.include_file_pattern"];
				filter.ExcludeFilePattern = config["filesystem.exclude_file_pattern"];
				filter.IncludeExtensions = config["filesystem.extensions"];
				filter.OnlyDirs = onlyDirs;

				// Directory is hidden use parent dir
				if (!filter.Accept(listDir)) {
					listDir = listDir.ParentFile;

					rs.Header["path"] = man.EncryptPath(listDir.AbsolutePath);
					rs.Header["visual_path"] = man.ToVisualPath(listDir.AbsolutePath, rootPath);
				}

				if (input["filter"] != null)
					filter.IncludeWildcardPattern = (string) input["filter"];

				if (input["only_files"] != null)
					filter.OnlyFiles = onlyFiles;
				else if (!onlyDirs)
					filter.OnlyFiles = !config.GetBool("filesystem.list_directories", false);

				// Add parent
				if (path != rootPath && input["only_files"] == null && (input["only_dirs"] != null || man.Config.GetBool("filesystem.list_directories", true))) {
					if (man.VerifyPath(listDir.Parent)) {
						hasParent = true;
						rs.Add("..", man.EncryptPath(listDir.Parent), -1, "parent", "", "", "", new NameValueCollection());
					}
				}

				// Setup input filter
				BasicFileFilter inputFilter = new BasicFileFilter();

				if (input["include_directory_pattern"] != null)
					filter.IncludeDirectoryPattern = (string) input["include_directory_pattern"];

				if (input["exclude_directory_pattern"] != null)
					filter.ExcludeDirectoryPattern = (string) input["exclude_directory_pattern"];

				if (input["include_file_pattern"] != null)
					filter.IncludeFilePattern = (string) input["include_file_pattern"];

				if (input["exclude_file_pattern"] != null)
					filter.ExcludeFilePattern = (string) input["exclude_file_pattern"];

				if (input["extensions"] != null)
					filter.IncludeExtensions = (string) input["extensions"];

				// Combine the filters
				CombinedFileFilter combinedFilter = new CombinedFileFilter();

				combinedFilter.AddFilter(inputFilter);
				combinedFilter.AddFilter(filter);

				files.AddRange(listDir.ListFilesFiltered(combinedFilter));

				if (input["page_size"] != null) {
					if (hasParent)
						pageSize = Convert.ToInt32(input["page_size"]) - 1;
					else
						pageSize = Convert.ToInt32(input["page_size"]);

					pages = (int) Math.Ceiling(files.Count / (double) pageSize);

					// Setup response
					rs.Header["pages"] = (pages > 1 ? pages : 1);
					rs.Header["count"] = files.Count;

					// Remove non visible files
					int start = Convert.ToInt32(input["page"]) * pageSize;
					int len = pageSize;
					len = start + len > files.Count ? len - ((start + len) - files.Count) : len;
					files = files.GetRange(start, len);
				}

				// Sort Files
				files.Sort(new FileComparer());

				// Output folders
				foreach (IFile file in files) {
					if (file.IsDirectory) {
						// Setup attribs
						attribs = "RW";

						type = "folder";

						// Fill custom info
						customInfo = new Hashtable();
						man.DispatchEvent(EventType.CustomInfo, file, "list", customInfo);

						// Special treatment of roots
						name = file.Name;
						if (path == "root:///") {
							if (man.RootNames[file.AbsolutePath] != null)
								name = man.RootNames[file.AbsolutePath];
						}

						// Add to resultset
						rs.Add(
							name,
							man.EncryptPath(file.AbsolutePath),
							file.IsDirectory ? -1 : file.Length,
							type,
							StringUtils.GetDate(file.CreationDate, listDir.Config.Get("filesystem.datefmt", "yyyy-MM-dd HH:mm")),
							StringUtils.GetDate(file.LastModified, listDir.Config.Get("filesystem.datefmt", "yyyy-MM-dd HH:mm")),
							attribs,
							customInfo
						);
					}
				}

				// Output files
				foreach (IFile file in files) {
					if (file.IsFile) {
						// Setup attribs
						attribs = "RW";

						type = PathUtils.GetExtension(file.AbsolutePath).ToLower();

						// Fill custom info
						customInfo = new Hashtable();
						man.DispatchEvent(EventType.CustomInfo, file, "list", customInfo);

						// Add to resultset
						rs.Add(
							file.Name,
							man.EncryptPath(file.AbsolutePath),
							file.Length,
							type,
							StringUtils.GetDate(file.CreationDate, listDir.Config.Get("filesystem.datefmt", "yyyy-MM-dd HH:mm")),
							StringUtils.GetDate(file.LastModified, listDir.Config.Get("filesystem.datefmt", "yyyy-MM-dd HH:mm")),
							attribs,
							customInfo
						);
					}
				}
			} else
				throw new ManagerException("{#error.file_not_exists}");

			return rs;
		}

		private ResultSet CreateDirs(ManagerEngine man, Hashtable input) {
			ResultSet rs = new ResultSet(new string[]{"status", "file", "message"});
			string path, name, template = null;
			IFile dir, file, templateFile;
			ManagerConfig config;

			path = man.DecryptPath((string) input["path"]);
			dir = man.GetFile(path);
			config = dir.Config;

			if (!man.IsToolEnabled("createdir", config))
				throw new ManagerException("{#error.no_access}");

			// Handle demo mode
			if (config.GetBool("general.demo", false)) {
				rs.Add("DEMO_ERROR", man.EncryptPath(dir.AbsolutePath), "{#error.demo}");
				return rs;
			}

			for (int i=0; input["name" + i] != null; i++) {
				// Get dir info
				name = (string) input["name" + i];

				if (input["template" + i] != null && ((string) input["template" + i]) != "")
					template = man.DecryptPath((string) input["template" + i]);

				// Setup target file
				file = man.GetFile(path, name, FileType.Directory);

				// Check if valid target file
				if (!man.VerifyFile(file, "createdir")) {
					rs.Add("FAILED", man.EncryptPath(file.AbsolutePath), man.InvalidFileMsg);
					continue;
				}

				// Setup template dir
				if (template != null) {
					templateFile = man.GetFile(template);

					if (!man.VerifyFile(templateFile, "createdir")) {
						rs.Add("ACCESS_ERROR", man.EncryptPath(file.AbsolutePath), man.InvalidFileMsg);
						continue;
					}

					if (!templateFile.Exists) {
						rs.Add("TEMPLATE_ERROR", man.EncryptPath(templateFile.AbsolutePath), "{#error.template_missing}");
						continue;
					}
				} else
					templateFile = null;

				// Check if target exists
				if (file.Exists) {
					rs.Add("EXISTS_ERROR", man.EncryptPath(file.AbsolutePath), "{#error.folder_exists}");
					continue;
				}

				// Create directory
				if (templateFile != null)
					templateFile.CopyTo(file);
				else
					file.MkDir();

				rs.Add("OK", man.EncryptPath(file.AbsolutePath), "{#message.directory_ok}");
			}

			return rs;
		}

		private object GetConfig(ManagerEngine man, Hashtable input) {
			string prefixes, path;
			IFile file;

			// Get input
			prefixes = (string) input["prefixes"];
			path = man.DecryptPath((string) input["path"]);

			// Request all
			if (prefixes == null)
				prefixes = "*";

			// Return config
			file = man.GetFile(path);

			if (!file.Exists)
				throw new ManagerException("{#error.file_not_exists}");

			return man.GetJSConfig(file.Config, prefixes);
		}

		private ResultSet InsertFiles(ManagerEngine man, Hashtable input) {
			ResultSet rs = new ResultSet(new string[]{"name","path","url","size","type","created","modified","attribs","custom"});

			for (int i=0; input["path" + i] != null; i++) {
				Hashtable customInfo = new Hashtable();
				IFile file;
				string url, attribs;

				file = man.GetFile(man.DecryptPath((string) input["path" + i]));
				if (!file.Exists)
					throw new ManagerException("{#error.file_not_exists}");

				man.DispatchEvent(EventType.CustomInfo, file, "insert", customInfo);
				man.DispatchEvent(EventType.InsertFile, file);

				url = PathUtils.RemoveTrailingSlash(file.Config["preview.urlprefix"]) + man.ConvertPathToURI(file.AbsolutePath) + file.Config["preview.urlsuffix"];
				attribs = "RW";

				rs.Add(file.Name,
				       man.EncryptPath(file.AbsolutePath),
				       url,
				       file.Length,
				       PathUtils.GetExtension(file.Name).ToLower(),
				       StringUtils.GetDate(file.CreationDate, file.Config.Get("filesystem.datefmt", "yyyy-MM-dd HH:mm")),
				       StringUtils.GetDate(file.LastModified, file.Config.Get("filesystem.datefmt", "yyyy-MM-dd HH:mm")),
				       attribs,
				       customInfo);
			}

			return rs;
		}

		private object LoopBack(ManagerEngine man, Hashtable input) {
			return input;
		}

		private ResultSet KeepAlive(ManagerEngine man, Hashtable input) {
			ResultSet rs = new ResultSet(new string[]{"status", "time", "message"});

			man.DispatchEvent(EventType.KeepAlive);

			rs.Add("KEEPALIVE", DateTime.Now.Ticks, "{#message.keepalive}");

			return rs;
		}

		#endregion
	}

	class FileComparer : IComparer {
		public int Compare(object a, object b) {
			IFile file1 = (IFile) a;
			IFile file2 = (IFile) b;

			return file1.Name.ToLower().CompareTo(file2.Name.ToLower());
		}
	}
}
