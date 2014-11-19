/*
 * $Id: ZipFile.cs 861 2012-04-11 12:24:30Z spocke $
 *
 * Copyright © 2007, Moxiecode Systems AB, All rights reserved. 
 */

using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using Moxiecode.Manager;
using Moxiecode.Manager.FileSystems;
using Moxiecode.Manager.Utils;
using System.Text.RegularExpressions;
using Moxiecode.ICSharpCode.SharpZipLib.Zip;

namespace Moxiecode.FileManager.FileSystems {
	/// <summary>
	///  LocalFileFactroy, generates new file instances.
	/// </summary>
	public class ZipFileFactory : IFileFactory {
		/// <summary>
		///  Creates a new file instance.
		/// </summary>
		/// <param name="man">Associated ManagerEngine.</param>
		/// <param name="path">Absolute path to the file.</param>
		/// <param name="child">Child name inside the path. Could be null or empty string.</param>
		/// <param name="type">Default file type, if the file doesn't exist yet.</param>
		public IFile GetFile(ManagerEngine man, string path, string child, FileType type) {
			return new ZipFileImpl(man, path, child, type);
		}
	}

 	/// <summary>
 	///  This class is the local file system implementation of IFile.
 	/// </summary>
 	public class ZipFileImpl : BaseFile, IDisposable {
		// Private fields
		private string innerPath;
		private string zipPath;
		private ZipEntry zipEntry;

		/// <summary>
		///  Initializes a file instance by a absolute path, child name and type.
		///  This can be userful when filtering non existing files.
		/// </summary>
		/// <param name="man">Reference to manager that requested the file.</param>
		/// <param name="absolute_path">Absolute file/directory path.</param>
		/// <param name="child_name">Name of child file for the directory.</param>
		/// <param name="type">Type of file to create.</param>
		public ZipFileImpl(ManagerEngine man, string absolute_path, string child_name, FileType type) : base(man, absolute_path, child_name, type) {
			this.absPath = this.absPath.Replace("zip://", "");

			Match match = Regex.Match(this.absPath, @"^(.*?.zip)(.*?)$", RegexOptions.IgnoreCase);
 
			this.zipPath = match.Groups[1].Value;
			this.innerPath = match.Groups[2].Value;

			if (this.innerPath == "")
				this.innerPath = "/";
		}

		/// <summary>Absolute path of parent directory.</summary>
		public override string Parent {
			get {
				if (this.innerPath == "/")
					return this.manager.GetFile(this.zipPath).Parent;

				string parentPath = this.innerPath.Substring(0, this.innerPath.LastIndexOf('/'));

				if (parentPath == "")
					parentPath = "/";

				return "zip://" + PathUtils.RemoveTrailingSlash(this.zipPath + parentPath);
			}
		}

		/// <summary>IFile of parent directory.</summary>
		public override IFile ParentFile {
		 	get {
				return this.manager.GetFile(this.Parent);
		 	}
		}

		/// <summary>Absolute file/directory path.</summary>
		public override string AbsolutePath {
			get { return "zip://" + this.absPath; }
		}

		/// <summary>
		///  File/Directory name without the path.
		/// </summary>
		public override string Name {
			get { return PathUtils.FileName(this.absPath); }
		}

		/// <summary>
		///  File size in bytes.
		/// </summary>
		public override long Length {
			get {
				if (this.ZipEntry == null)
					return -1;

				return this.ZipEntry.Size;
			}
		}

		/// <summary>
		///  Exists state.
		/// </summary>
		public override bool Exists {
			get {
				if (this.innerPath == "/")
					return true;

				return this.ZipEntry != null;
			}
		}

		/// <summary>
		///  Is file state.
		/// </summary>
		public override bool IsFile {
			get {
				if (this.innerPath == "/")
					return false;

				return this.Exists && this.ZipEntry.IsFile;
			}
		}

		/// <summary>
		///  Is directory state.
		/// </summary>
		public override bool IsDirectory {
			get {
				if (this.innerPath == "/")
					return true;

				return this.Exists && this.ZipEntry.IsDirectory;
			}
		}

		/// <summary>
		///  Last modification date of the file/directory.
		/// </summary>
		public override DateTime LastModified {
			get {
				if (this.innerPath == "/")
					return DateTime.Now;

				return this.ZipEntry.DateTime;
			}

			set {
				ZipFile zipFile = this.OpenZipFile();
				ZipEntry entry;

				try {
					zipFile.BeginUpdate();

					entry = zipFile.GetEntry(this.innerPath.Substring(0) + (this.IsDirectory ? "/" : ""));

					if (entry != null)
						entry.DateTime = value;

					zipFile.CommitUpdate();
				} finally {
					zipFile.Close();
				}
			}
		}

		/// <summary>
		///  Creation date of the file/directory.
		/// </summary>
		public override DateTime CreationDate {
			get {
				if (this.innerPath == "/")
					return DateTime.Now;

				return this.ZipEntry.DateTime;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override ManagerConfig Config {
			get {
				return this.manager.GetFile(this.zipPath).Config;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="local_absolute_path"></param>
		public override void ExportFile(string local_absolute_path) {
			ZipFile zipFile = this.OpenZipFile();
			string localPath;
			int pos;

			local_absolute_path = PathUtils.ToUnixPath(local_absolute_path);

			try {
				if (this.IsDirectory) {
					foreach (ZipEntry entry in zipFile) {
						if (entry.Name.StartsWith(this.InnerPath.Substring(1) + "/") || this.innerPath == "/") {
							localPath = PathUtils.AddTrailingSlash(local_absolute_path) + entry.Name;

							if ((pos = entry.Name.IndexOf('/')) != -1)
								localPath = PathUtils.AddTrailingSlash(local_absolute_path) + entry.Name.Substring(pos);

							if (entry.IsDirectory) {
								localPath = localPath.Substring(0, localPath.Length - 1);
								Directory.CreateDirectory(localPath);
							} else
								IOUtils.StreamFromTo(zipFile.GetInputStream(entry), new FileStream(localPath, FileMode.Create), 1024);
						}
					}
				} else {
					ZipEntry entry = zipFile.GetEntry(this.innerPath.Substring(1));
					localPath = local_absolute_path;

					if (entry != null)
						IOUtils.StreamFromTo(zipFile.GetInputStream(entry), new FileStream(localPath, FileMode.Create), 1024);
				}
			} finally {
				zipFile.Close();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="local_absolute_path"></param>
		public override void ImportFile(string local_absolute_path) {
			ZipFile zipFile = this.OpenZipFile();
			ZipEntry entry;
			string entryPath;

			try {
				this.DispatchBeforeFileAction(FileAction.Add);

				zipFile.BeginUpdate();

				if (Directory.Exists(local_absolute_path)) {
					ArrayList paths = new ArrayList();

					entryPath = this.InnerPath.Substring(1) + "/";

					entry = new ZipEntry(entryPath);
					entry.ExternalFileAttributes = 16;
					entry.Size = 0;
					entry.CompressedSize = 0;

					if (zipFile.GetEntry(entryPath) == null)
						zipFile.Add(entry);

					this.ListTree(local_absolute_path, paths);

					foreach (string path in paths) {
						entryPath = this.InnerPath.Substring(1) + path.Substring(local_absolute_path.Length);

						if (this.InnerPath == "/")
							entryPath = entryPath.Substring(1);

						if (Directory.Exists(path)) {
							entry = new ZipEntry(entryPath + "/");
							entry.ExternalFileAttributes = 16;
							entry.Size = 0;
							entry.CompressedSize = 0;

							if (zipFile.GetEntry(entryPath) == null)
								zipFile.Add(entry);

							//Debug(path + "," + entryPath);
						} else {
							if (zipFile.GetEntry(entryPath) == null)
								zipFile.Add(new ZipDataSource(new FileStream(path, FileMode.Open)), entryPath);

							//Debug(path + "," + entryPath);
						}
					}
				} else if(File.Exists(local_absolute_path)) {
					entryPath = this.InnerPath.Substring(1);

					if (zipFile.GetEntry(entryPath) == null)
						zipFile.Add(new ZipDataSource(new FileStream(local_absolute_path, FileMode.Open)), entryPath);
				}

				zipFile.CommitUpdate();

				this.DispatchFileAction(FileAction.Add);
			} finally {
				zipFile.Close();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="mode"></param>
		/// <returns></returns>
		public override Stream Open(FileMode mode) {
			return new ZipFileStream(this, mode);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override bool MkDir() {
			ZipFile zipFile = this.OpenZipFile();
			string entryPath = this.InnerPath.Substring(1) + "/";

			try {
				this.DispatchBeforeFileAction(FileAction.MkDir);

				zipFile.BeginUpdate();

				ZipEntry entry = new ZipEntry(entryPath);

				entry.ExternalFileAttributes = 16;
				entry.Size = 0;
				entry.CompressedSize = 0;

				if (zipFile.GetEntry(entryPath) == null)
					zipFile.Add(entry);

				zipFile.CommitUpdate();

				this.DispatchFileAction(FileAction.MkDir);
			} finally {
				zipFile.Close();
			}

			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override bool Delete() {
			ZipFile zipFile = this.OpenZipFile();

			try {
				this.DispatchBeforeFileAction(FileAction.Delete);

				zipFile.BeginUpdate();

				if (this.IsDirectory)
					zipFile.Delete(this.InnerPath.Substring(1) + "/");
				else
					zipFile.Delete(this.InnerPath.Substring(1));

				zipFile.CommitUpdate();

				this.DispatchFileAction(FileAction.Delete);
			} finally {
				zipFile.Close();
			}

			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="deep"></param>
		/// <returns></returns>
		public override bool Delete(bool deep) {
			ZipFile zipFile = this.OpenZipFile();

			try {
				this.DispatchBeforeFileAction(FileAction.Delete);

				zipFile.BeginUpdate();

				if (this.IsDirectory) {
					foreach (ZipEntry entry in zipFile) {
						if (entry.Name.StartsWith(this.InnerPath.Substring(1) + "/"))
							zipFile.Delete(entry);
					}
				} else
					zipFile.Delete(this.InnerPath.Substring(1));

				zipFile.CommitUpdate();

				this.DispatchFileAction(FileAction.Delete);
			} finally {
				zipFile.Close();
			}

			return true;
		}
		
		/// <summary>List files in the specified directory.</summary>
		/// <returns>List of files within the directory.</returns>
		public override IFile[] ListFiles() {
			return this.ListFilesFiltered(null);
		}

		/// <summary>List files by the specified filter.</summary>
		/// <param name="filter">Instance to filter by.</param>
		/// <returns>Filter list of files.</returns>
		public override IFile[] ListFilesFiltered(IFileFilter filter) {
			ArrayList files = new ArrayList();
			int slashes;
			string entryPath, listPrefix = "/";

			if (this.innerPath != "/")
				listPrefix = this.innerPath + "/";

			slashes = CountChar(listPrefix, '/');

			ZipFile zipFile = this.OpenZipFile();
			foreach (ZipEntry entry in zipFile) {
				entryPath = "/" + (entry.IsDirectory ? entry.Name.Substring(0, entry.Name.Length - 1) : entry.Name);

				//Debug("entryPath: " + entryPath +",listPrefix: "+ listPrefix +",CountChar:"+ CountChar(entryPath, '/') +",slashes:"+ slashes + ",indexof:" + entryPath.IndexOf(listPrefix));

				if (entryPath.StartsWith(listPrefix) && CountChar(entryPath, '/') == slashes) {
					ZipFileImpl file = new ZipFileImpl(this.manager, this.zipPath, entryPath.Substring(1), FileType.Unknown);

					file.ZipEntry = entry;

					files.Add(file);
				}
			}

			zipFile.Close();

			return (IFile[]) files.ToArray(typeof(ZipFileImpl));
		}
		/// <summary>Lists a tree of files within a directory.</summary>
		/// <returns>List of files within the tree.</returns>
		public override IFile[] ListTree() {
		 	return this.ListTreeFiltered(null);
		}

		/// <summary>
		/// Lists a tree of tiles within a directory by the specified filter.
		/// </summary>
		/// <param name="filter">Instance to filter by.</param>
		/// <returns>Returns a filtered list of files.</returns>
		public override IFile[] ListTreeFiltered(IFileFilter filter) {
			ArrayList files = new ArrayList();
			ZipFile zipFile = this.OpenZipFile();

			try {
				if (this.IsDirectory) {
					foreach (ZipEntry entry in zipFile) {
						if (entry.Name.StartsWith(this.InnerPath.Substring(1) + "/") && entry.Name != this.InnerPath.Substring(1) + "/") {
							if (entry.IsDirectory)
								files.Add(new ZipFileImpl(this.manager, "zip://" + this.zipPath + "/" + entry.Name.Substring(0, entry.Name.Length - 1), null, FileType.Unknown));
							else
								files.Add(new ZipFileImpl(this.manager, "zip://" + this.zipPath + "/" + entry.Name, null, FileType.Unknown));
						}
					}
				}
			} finally {
				zipFile.Close();
			}

			return (IFile[]) files.ToArray(typeof(ZipFileImpl));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public ZipFile OpenZipFile() {
			if (File.Exists(this.zipPath))
				return new ZipFile(this.zipPath);

			return ZipFile.Create(this.zipPath);
		}

		/// <summary>
		/// 
		/// </summary>
		public ZipEntry ZipEntry {
			get {
				ZipFile zipFile = this.OpenZipFile();

				// Look for file
				if (this.zipEntry == null)
					this.zipEntry = zipFile.GetEntry(this.innerPath.Substring(1));

				// Look for dir
				if (this.zipEntry == null)
					this.zipEntry = zipFile.GetEntry(this.innerPath.Substring(1) + "/");

				zipFile.Close();

				return this.zipEntry;
			}

			set {
				this.zipEntry = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void Dispose() {
			if (this.zipEntry != null)
				this.zipEntry = null;
		}

		/// <summary>
		/// 
		/// </summary>
		public string InnerPath {
			get {
				return this.innerPath;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string ZipPath {
			get {
				return this.zipPath;
			}
		}

		#region private methods

		private void ListTree(string path, ArrayList paths) {
			string[] dirs = Directory.GetDirectories(path);
			string[] files = Directory.GetFiles(path);

			foreach (string file in files)
				paths.Add(PathUtils.ToUnixPath(file));

			foreach (string dir in dirs) {
				paths.Add(PathUtils.ToUnixPath(dir));

				ListTree(dir, paths);
			}
		}

		private int CountChar(string str, char chr) {
			int count = 0;

			foreach (char chkChr in str.ToCharArray()) {
				if (chr == chkChr)
					count++;
			}

			return count;
		}

		#endregion
	}

 	/// <summary>
 	/// 
 	/// </summary>
 	public class ZipFileStream : MemoryStream {
		private ZipFileImpl zipFile;
		private FileMode mode;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="file"></param>
		/// <param name="mode"></param>
		public ZipFileStream(ZipFileImpl file, FileMode mode) : base() {
			this.zipFile = file;
			this.mode = mode;

			// Read in whole zip entry
			if (this.mode == FileMode.Open || this.mode == FileMode.Append) {
				// Can't open non existing zip
				if (!File.Exists(this.zipFile.ZipPath))
					return;

				ZipFile zipFile = file.OpenZipFile();

				try {
					if (file.ZipEntry == null) {
						zipFile.Close();
						return;
					}

					Stream stream = zipFile.GetInputStream(file.ZipEntry);
					byte[] buff = new byte[1024];
					int len;

					// Read and stream chunks
					try {
						while ((len = stream.Read(buff, 0, 1024)) > 0)
							this.Write(buff, 0, len);
					} finally {
						stream.Close();
					}

					if (this.mode == FileMode.Open)
						this.Seek(0, SeekOrigin.Begin);
				} finally {
					zipFile.Close();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Close() {
			base.Close();

			if (this.mode == FileMode.Append || this.mode == FileMode.Create || this.mode == FileMode.CreateNew) {
				ZipFile zipFile = this.zipFile.OpenZipFile();
				string innerPath = this.zipFile.InnerPath.Substring(1);

				try {
					this.zipFile.DispatchBeforeFileAction(FileAction.Update);

					zipFile.BeginUpdate();

					if (zipFile.GetEntry(innerPath) != null)
						zipFile.Delete(this.zipFile.InnerPath.Substring(1));

					zipFile.Add(new ZipDataSource(new MemoryStream(this.ToArray())), innerPath);

					zipFile.CommitUpdate();

					this.zipFile.DispatchFileAction(FileAction.Update);
				} finally {
					zipFile.AbortUpdate();
					zipFile.Close();
				}
			}
		}
 	}
 
 	/// <summary>
 	/// 
 	/// </summary>
 	public class ZipDataSource : IStaticDataSource {
 		private Stream stream;
 
 		/// <summary>
 		/// 
 		/// </summary>
 		/// <param name="stream"></param>
 		public ZipDataSource(Stream stream) {
 			this.stream = stream;
 		}
 
 		/// <summary>
 		/// 
 		/// </summary>
 		/// <returns></returns>
 		public Stream GetSource() {
 			return this.stream;
 		}
 	}
}
