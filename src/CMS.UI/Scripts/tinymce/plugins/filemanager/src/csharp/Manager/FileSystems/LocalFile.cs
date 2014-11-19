/*
 * $Id: LocalFile.cs 821 2011-01-17 13:18:04Z spocke $
 *
 * Copyright © 2007, Moxiecode Systems AB, All rights reserved. 
 */

using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using Moxiecode.Manager;
using Moxiecode.Manager.Utils;

namespace Moxiecode.Manager.FileSystems {
	/// <summary>
	///  LocalFileFactroy, generates new file instances.
	/// </summary>
	public class LocalFileFactory : IFileFactory {
		/// <summary>
		///  Returns a file based on path, child path and type.
		/// </summary>
		/// <param name="man">Manager engine reference.</param>
		/// <param name="path">Absolute path to file or directory.</param>
		/// <param name="child">Child path inside directory or empty string.</param>
		/// <param name="type">File type, force it to be a file or directory.</param>
		public IFile GetFile(ManagerEngine man, string path, string child, FileType type) {
			return new LocalFile(man, path, child, type);
		}
	}

 	/// <summary>
 	///  This class is the local file system implementation of IFile.
 	/// </summary>
 	public class LocalFile : IFile {
		// Private fields
		private string absPath = "";
		private ManagerEngine manager;
		private ManagerConfig config;
		private FileInfo fileInfo;
		private DirectoryInfo dirInfo;
		private ArrayList fileTree;
		private bool configResolved;
		private bool triggerEvents;

 		/// <summary>
 		///  Initializes a file instance by a absolute path.
 		/// </summary>
 		/// <param name="man">Reference to manager that requested the file.</param>
 		/// <param name="absolute_path">Absolute file/directory path.</param>
 		public LocalFile(ManagerEngine man, string absolute_path) : this(man, absolute_path, "", FileType.Unknown) {
		}
 
  		/// <summary>
 		///  Initializes a file instance by a absolute path and child name.
 		/// </summary>
 		/// <param name="man">Reference to manager that requested the file.</param>
 		/// <param name="absolute_path">Absolute file/directory path.</param>
 		/// <param name="child_name">Name of child file for the directory.</param>
		public LocalFile(ManagerEngine man, string absolute_path, string child_name) : this(man, absolute_path, child_name, FileType.Unknown) {
		}

  		/// <summary>
 		///  Initializes a file instance by a absolute path, child name and type.
 		///  This can be userful when filtering non existing files.
 		/// </summary>
 		/// <param name="man">Reference to manager that requested the file.</param>
 		/// <param name="absolute_path">Absolute file/directory path.</param>
 		/// <param name="child_name">Name of child file for the directory.</param>
 		/// <param name="type">Type of file to create.</param>
		public LocalFile(ManagerEngine man, string absolute_path, string child_name, FileType type) {
			this.manager = man;

			if (child_name != "")
				this.absPath = PathUtils.ToUnixPath(absolute_path + "/" + child_name);
			else
				this.absPath = PathUtils.ToUnixPath(absolute_path);

			if (type == FileType.Directory)
				this.dirInfo = new DirectoryInfo(PathUtils.ToOSPath(this.absPath));
			else if (type == FileType.File)
				this.fileInfo = new FileInfo(PathUtils.ToOSPath(this.absPath));
			else {
				// Create file info or dir info
				this.fileInfo = new FileInfo(PathUtils.ToOSPath(this.absPath));
				if (!this.fileInfo.Exists) {
					this.dirInfo = new DirectoryInfo(PathUtils.ToOSPath(this.absPath));
					this.fileInfo = null;
				}

				if (this.fileInfo != null)
					this.absPath = PathUtils.ToUnixPath(this.fileInfo.FullName);

				if (this.dirInfo != null)
					this.absPath = PathUtils.RemoveTrailingSlash(PathUtils.ToUnixPath(this.dirInfo.FullName));
			}

			this.config = this.manager.Config;
			this.configResolved = false;
			this.triggerEvents = true;
		}

		/// <summary>Absolute path of parent directory.</summary>
		public string Parent {
			get {
				try {
					if (this.fileInfo != null)
						return PathUtils.ToUnixPath(this.fileInfo.Directory.FullName);
					else
						return this.dirInfo.Parent == null ? null : PathUtils.ToUnixPath(this.dirInfo.Parent.FullName);
				} catch {
					// FileIOPermission will be thrown when getting the parent our side the root in medium trust
					return null;
				}
			}
		}

		/// <summary>IFile of parent directory.</summary>
		public IFile ParentFile {
		 	get {
				string parentPath = this.Parent;

				if (parentPath == null)
					return null;

		 		return new LocalFile(this.manager, parentPath);
		 	}
		}

		/// <summary>State if file action events should be fired or not.</summary>
		public bool TriggerEvents {
			get { return this.triggerEvents; }
			set { this.triggerEvents = value; }
		}

		/// <summary>File/directory name.</summary>
		public string Name {
		 	get { return this.IsFile ? this.fileInfo.Name : this.dirInfo.Name; }
		}

		/// <summary>Absolute file/directory path.</summary>
		public string AbsolutePath {
			get { return PathUtils.ToUnixPath(this.IsFile ? this.fileInfo.FullName : this.dirInfo.FullName); }
		}

		///<summary>File exists property, if true the file/directory exists.</summary>
		public bool Exists {
		 	get { return this.IsFile ? this.fileInfo.Exists : this.dirInfo.Exists; }
		}

		/// <summary>Is directory property, is true if the file instance is a directory.</summary>
		public bool IsDirectory {
		 	get { return this.dirInfo != null && this.dirInfo.Exists; }
		}

		/// <summary>Is file property, is true if the file instance is a file.</summary>
		public bool IsFile {
		 	get { return this.fileInfo != null; }
		}

		/// <summary>Last modification date of file/directory.</summary>
		public DateTime LastModified {
		 	get { return this.IsFile ? this.fileInfo.LastWriteTime : this.dirInfo.LastWriteTime; }

		 	set {
		 		if (this.IsFile)
		 			this.fileInfo.LastWriteTime = value;
		 		else
		 			this.dirInfo.LastWriteTime = value;
		 	}
		}

		/// <summary>Creation date of file/directory.</summary>
		public DateTime CreationDate {
			get { return this.IsFile ? this.fileInfo.CreationTime : this.dirInfo.CreationTime; }
		}

		/// <summary>Read access property, is true if the file is readable.</summary>
		public bool CanRead {
			get { return true; }
		}

		/// <summary>Write access property, if true if the file is writable.</summary>
		public bool CanWrite {
			get { return true; }
		}

		/// <summary>File size in bytes of file.</summary>
		public long Length {
			get { return this.Exists && this.IsFile ? this.fileInfo.Length : -1; }
		}

		/// <summary>Imports a local file to the file system, for example when users upload files.</summary>
		/// <param name="local_absolute_path">Absolute path to local file.</param>
		public void ImportFile(string local_absolute_path) {
		 	this.DispatchBeforeFileAction(FileAction.Add);
		 	this.DispatchFileAction(FileAction.Add);
		}

		/// <summary>Exports the file to the local file system.</summary>
		/// <param name="local_absolute_path">Absolute path to local file.</param>
		public void ExportFile(string local_absolute_path) {
		}

		/// <summary>Copies the file to the destination file.</summary>
		/// <param name="dest">Destination file to copy to.</param>
		public bool CopyTo(IFile dest) {
			if (dest is LocalFile) {
				if (dest.Exists)
					return false;

				if (dest.AbsolutePath.IndexOf(this.AbsolutePath) == 0)
					return false;

				this.DispatchBeforeFileAction(FileAction.Copy, dest);

				if (this.IsFile)
					this.fileInfo.CopyTo(PathUtils.ToOSPath(dest.AbsolutePath));
				else
					this.CopyDirectory(PathUtils.ToOSPath(this.AbsolutePath), PathUtils.ToOSPath(dest.AbsolutePath));

				this.DispatchFileAction(FileAction.Copy, dest);
			} else
				dest.ImportFile(this.AbsolutePath);

			return true;
		}

		/// <summary>Deletes the file.</summary>
		public bool Delete() {
		 	return this.Delete(false);
		}

		/// <summary>Deletes the file and any files within the directory if it's a directory.</summary>
		/// <param name="deep">If set to true, files within the directory are deleted too.</param>
		public bool Delete(bool deep) {
			if (!this.Exists)
				return false;

			if (this.IsFile) {
				this.DispatchBeforeFileAction(FileAction.Delete);
				this.fileInfo.Delete();
				this.DispatchFileAction(FileAction.Delete);
				this.fileInfo.Refresh();
			} else {
				this.DispatchBeforeFileAction(FileAction.RmDir);
				this.dirInfo.Delete(deep);
				this.DispatchFileAction(FileAction.RmDir);
				this.dirInfo.Refresh();
			}

			return true;
		}

		/// <summary>List files in the specified directory.</summary>
		/// <returns>List of files within the directory.</returns>
		public IFile[] ListFiles() {
			return this.ListFilesFiltered(null);
		}

		/// <summary>List files by the specified filter.</summary>
		/// <param name="filter">Instance to filter by.</param>
		/// <returns>Filter list of files.</returns>
		public IFile[] ListFilesFiltered(IFileFilter filter) {
		 	DirectoryInfo dirInfo = new DirectoryInfo(PathUtils.ToOSPath(this.AbsolutePath));
		 	DirectoryInfo[] dirInfos = dirInfo.GetDirectories();
		 	FileInfo[] fileInfos = dirInfo.GetFiles();
		 	string accessFile = this.Config["filesystem.local.access_file_name"];
			ArrayList files = new ArrayList();

			// Add directories
			foreach (DirectoryInfo info in dirInfos) {
				LocalFile file = (LocalFile) this.manager.GetFile(PathUtils.ToUnixPath(info.FullName));

				if (filter != null && !filter.Accept(file))
					continue;

				files.Add(file);
			}

			// Add files
			foreach (FileInfo info in fileInfos) {
				if (info.Name == accessFile)
					continue;

				LocalFile file = (LocalFile) this.manager.GetFile(PathUtils.ToUnixPath(info.FullName));

				if (filter != null && !filter.Accept(file))
					continue;

				files.Add(file);
			}

			return (IFile[]) files.ToArray(typeof(LocalFile));
		}

		/// <summary>Lists a tree of files within a directory.</summary>
		/// <returns>List of files within the tree.</returns>
		public IFile[] ListTree() {
		 	return this.ListTreeFiltered(null);
		}

		/// <summary>
		/// Lists a tree of tiles within a directory by the specified filter.
		/// </summary>
		/// <param name="filter">Instance to filter by.</param>
		/// <returns>Returns a filtered list of files.</returns>
		public IFile[] ListTreeFiltered(IFileFilter filter) {
		 	this.fileTree = new ArrayList();

			this.ListTree(this, filter, 0);

			return (IFile[]) this.fileTree.ToArray(typeof(LocalFile));
		}

		/// <summary>
		///  Creates a new directory.
		/// </summary>
		/// <returns>true - success, false - failure</returns>
		public bool MkDir() {
			this.DispatchBeforeFileAction(FileAction.MkDir);

			if (!IOUtils.CreateDirectory(PathUtils.ToOSPath(this.absPath), IntPtr.Zero))
				return false;

			this.DispatchFileAction(FileAction.MkDir);

			return true;
		}

		/// <summary>
		///  Renames/Moves this file to the specified file instance.
		/// </summary>
		/// <param name="dest">File to rename/move to.</param>
		/// <returns>boolean true - success, false - failure</returns>
		public bool RenameTo(IFile dest) {
			if (dest is LocalFile) {
				bool status = true;
	
				// Allready exists
				if (dest.Exists)
					return false;
	
				// Hmm, invalid rename
				string srcPath = this.IsFile ? this.AbsolutePath : this.AbsolutePath + "/";
				string destPath = dest.IsFile ? dest.AbsolutePath : dest.AbsolutePath + "/";
	
				if (destPath.IndexOf(srcPath) == 0)
					return false;

				this.DispatchBeforeFileAction(FileAction.Rename, dest);

				if (this.fileInfo != null) {
					this.fileInfo.MoveTo(dest.AbsolutePath);
					this.fileInfo.Refresh();
				} else {
					this.dirInfo.MoveTo(dest.AbsolutePath);
					this.dirInfo.Refresh();
				}

				this.DispatchFileAction(FileAction.Rename, dest);

				return status;
			} else {
				dest.ImportFile(this.AbsolutePath);
				this.Delete(true);
				return true;
			}
		}

		/// <summary>A merged name/value array of config elements.</summary>
		public ManagerConfig Config {
		 	get {
		 		// Use resolved config
		 		if (this.configResolved)
		 			return this.config;

		 		string accessFileName = this.config["filesystem.local.access_file_name"];
				ArrayList accessFiles = new ArrayList();
				IFile dirFile;

				// Get file list stop at root
				dirFile = this.IsFile ? this.ParentFile : this;
				do {
					IFile accessFile;

					accessFile = this.manager.GetFile(dirFile.AbsolutePath, accessFileName);

					if (accessFile.Exists && accessFile.IsFile)
						accessFiles.Add(accessFile);
				} while (this.manager.VerifyPath(dirFile.Parent) && (dirFile = dirFile.ParentFile) != null);

				// Any files at all
				if (accessFiles.Count > 0) {
					ManagerConfig newConfig = new ManagerConfig();
					ArrayList allowKeys = new ArrayList();

					// Clone the config
					foreach (string configKey in config)
						newConfig[configKey] = config[configKey];

					GetAllowKeys(allowKeys, newConfig, newConfig);

					// Merge all access files
					for (int i=accessFiles.Count-1; i>=0; i--) {
						IFile accessFile = (IFile) accessFiles[i];

						Properties accessProperties = new Properties();
						accessProperties.Load(accessFile.AbsolutePath);

						// Override values
						foreach (string propKey in accessProperties.AllKeys) {
							if (allowKeys.IndexOf(propKey) != -1)
								newConfig[propKey] = accessProperties[propKey];
						}

						// Current directory access file
						IFile dir = this.IsFile ? this.ParentFile : this;
						if (accessFile.ParentFile.AbsolutePath == dir.AbsolutePath) {
							foreach (string propKey in accessProperties.AllKeys) {
								if (propKey[0] == '_') {
									string tmpKey = propKey.Substring(1);

									if (allowKeys.IndexOf(tmpKey) != -1)
										newConfig[tmpKey] = accessProperties[propKey];
								}
							}
						}

						GetAllowKeys(allowKeys, accessProperties, newConfig);
					}

					//foreach (string k in allowKeys)
					//	Debug("Allow: " + k);

					// Set new config
					this.config = newConfig;
				}

				// Variable substitute the values
				foreach (string key in this.config.AllKeys) {
					string val = this.config[key];
					string path;

					if (val.IndexOf('{') != -1) {
						if (this.IsFile)
							path = this.AbsolutePath;
						else
							path = this.Parent;

						val = val.Replace("${path}", path);

						if (this.manager.RootPaths.Count > 0)
							val = val.Replace("${rootpath}", PathUtils.ToUnixPath(this.manager.RootPaths[0]));

						for (int i=0; i<this.manager.RootPaths.Count; i++)
							val = val.Replace("${rootpath" + i + "}", PathUtils.ToUnixPath(this.manager.RootPaths[i]));

						this.config[key] = val;
					}
				}

				this.configResolved = true;

				return this.config;
		 	}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="mode"></param>
		/// <returns></returns>
		public Stream Open(FileMode mode) {
			// Prevent access denied exception of no user write rights
			if (mode == FileMode.Open)
				return File.OpenRead(this.absPath);

			return new FileStream(this.absPath, mode);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override string ToString() {
			return this.AbsolutePath;
		}

		#region private methods

		private int ListTree(IFile target_file, IFileFilter file_filter, int level) {
		 	IFile[] files = target_file.ListFilesFiltered(file_filter);

		 	foreach (IFile file in files) {
		 		// Go deeper
		 		if (file.IsDirectory) {
		 			level++;
		 			this.ListTree(file, file_filter, level);
		 			level--;
		 		}

		 		this.fileTree.Add(file);
		 	}

		 	return 0;
		}

		private void GetAllowKeys(ArrayList allow_keys, NameValueCollection config, NameValueCollection full_config) {
			// Setup allowkeys
			foreach (string configKey in config) {
				string value = config[configKey];
				int pos;

				if ((pos = configKey.IndexOf(".allow_override")) != -1) {
					string prefix = configKey.Substring(0, pos);

					// Reset all previous
					foreach (string resetKey in (ArrayList) allow_keys.Clone()) {
						if (resetKey.IndexOf(prefix) == 0) {
							allow_keys.Remove(resetKey);
						}
					}

					if (value == "*") {
						foreach (string allConfigKey in full_config) {
							if (allConfigKey.IndexOf(prefix) == 0)
								allow_keys.Add(allConfigKey);
						}
					} else {
						string[] keys = value.Split(',');
						foreach (string key in keys) {
							allow_keys.Add(prefix + "." + key);
						}
					}
				}
			}
	 	}

		private void CopyDirectory(string src, string dst){
			String[] files;

			if (dst[dst.Length-1] != Path.DirectorySeparatorChar) 
				dst += Path.DirectorySeparatorChar;

			if (!Directory.Exists(dst))
				new DirectoryInfo(dst).Create();

			files = Directory.GetFileSystemEntries(src);

			foreach( string element in files){
				if (Directory.Exists(element)) 
					CopyDirectory(element, dst + Path.GetFileName(element));
				else 
					File.Copy(element, dst + Path.GetFileName(element), true);
			}
		}

		private void DispatchBeforeFileAction(FileAction action) {
			DispatchBeforeFileAction(action, null);
		}

		private void DispatchBeforeFileAction(FileAction action, IFile file2) {
			if (this.triggerEvents)
				this.manager.DispatchEvent(EventType.BeforeFileAction, action, this, file2);
		}

		private void DispatchFileAction(FileAction action) {
			DispatchFileAction(action, null);
		}

		private void DispatchFileAction(FileAction action, IFile file2) {
			if (this.triggerEvents)
				this.manager.DispatchEvent(EventType.FileAction, action, this, file2);
		}

		#endregion
	}
}
