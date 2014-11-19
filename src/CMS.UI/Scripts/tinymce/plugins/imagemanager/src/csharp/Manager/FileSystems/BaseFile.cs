/*
 * $Id: BaseFile.cs 11 2007-05-27 14:47:18Z spocke $
 *
 * Copyright © 2007, Moxiecode Systems AB, All rights reserved. 
 */

using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using Moxiecode.Manager;
using Moxiecode.Manager.Utils;

namespace Moxiecode.Manager.FileSystems {
 	/// <summary>
 	///  This class is the local file system implementation of IFile.
 	/// </summary>
 	public class BaseFile : IFile {
 		/// <summary></summary>
		protected string absPath = "";

		/// <summary></summary>
		protected ManagerEngine manager;

		/// <summary></summary>
		protected bool triggerEventsState;

		/// <summary></summary>
		protected ArrayList fileTree;

 		/// <summary>
 		///  Initializes a file instance by a absolute path.
 		/// </summary>
 		/// <param name="man">Reference to manager that requested the file.</param>
 		/// <param name="absolute_path">Absolute file/directory path.</param>
 		public BaseFile(ManagerEngine man, string absolute_path) : this(man, absolute_path, "", FileType.Unknown) {
		}
 
  		/// <summary>
 		///  Initializes a file instance by a absolute path and child name.
 		/// </summary>
 		/// <param name="man">Reference to manager that requested the file.</param>
 		/// <param name="absolute_path">Absolute file/directory path.</param>
 		/// <param name="child_name">Name of child file for the directory.</param>
		public BaseFile(ManagerEngine man, string absolute_path, string child_name) : this(man, absolute_path, child_name, FileType.Unknown) {
		}

  		/// <summary>
 		///  Initializes a file instance by a absolute path, child name and type.
 		///  This can be userful when filtering non existing files.
 		/// </summary>
 		/// <param name="man">Reference to manager that requested the file.</param>
 		/// <param name="absolute_path">Absolute file/directory path.</param>
 		/// <param name="child_name">Name of child file for the directory.</param>
 		/// <param name="type">Type of file to create.</param>
		public BaseFile(ManagerEngine man, string absolute_path, string child_name, FileType type) {
			this.manager = man;

			if (child_name != null && child_name != "")
				this.absPath = PathUtils.ToUnixPath(absolute_path + "/" + child_name);
			else
				this.absPath = PathUtils.ToUnixPath(absolute_path);
		}

		/// <summary>Absolute path of parent directory.</summary>
		public virtual string Parent {
			get {
				Match match = Regex.Match(this.absPath, @"(.*)/[^/]*$");

				if (match.Success)
					return match.Groups[1].Value;

				return null;
			}
		}

		/// <summary>IFile of parent directory.</summary>
		public virtual IFile ParentFile {
		 	get {
				string parentPath = this.Parent;

				if (parentPath == null)
					return null;

		 		return new BaseFile(this.manager, parentPath);
		 	}
		}

		/// <summary>State if file action events should be fired or not.</summary>
		public virtual bool TriggerEvents {
			get { return this.triggerEventsState; }
			set { this.triggerEventsState = value; }
		}

		/// <summary>File/directory name.</summary>
		public virtual string Name {
		 	get {
				Match match = Regex.Match(this.absPath, @"[^/]+/([^/]+)$");

				if (match.Success)
					return match.Groups[1].Value;

				return null;
			}
		}

		///<summary>File exists property, if true the file/directory exists.</summary>
		public virtual bool Exists {
		 	get { return true; }
		}

		/// <summary>Absolute file/directory path.</summary>
		public virtual string AbsolutePath {
			get { return this.absPath; }
		}

		/// <summary>Is directory property, is true if the file instance is a directory.</summary>
		public virtual bool IsDirectory {
		 	get { return true; }
		}

		/// <summary>Is file property, is true if the file instance is a file.</summary>
		public virtual bool IsFile {
		 	get { return !this.IsDirectory; }
		}

		/// <summary>Last modification date of file/directory.</summary>
		public virtual DateTime LastModified {
		 	get { return DateTime.Now; }

		 	set {
		 	}
		}

		/// <summary>Creation date of file/directory.</summary>
		public virtual DateTime CreationDate {
			get { return DateTime.Now; }
		}

		/// <summary>Read access property, is true if the file is readable.</summary>
		public virtual bool CanRead {
			get { return true; }
		}

		/// <summary>Write access property, if true if the file is writable.</summary>
		public virtual bool CanWrite {
			get { return true; }
		}

		/// <summary>File size in bytes of file.</summary>
		public virtual long Length {
			get { return 0; }
		}

		/// <summary>Imports a local file to the file system, for example when users upload files.</summary>
		/// <param name="local_absolute_path">Absolute path to local file.</param>
		public virtual void ImportFile(string local_absolute_path) {
		}

		/// <summary>Exports the file to the local file system.</summary>
		/// <param name="local_absolute_path">Absolute path to local file.</param>
		public virtual void ExportFile(string local_absolute_path) {
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dest"></param>
		/// <returns></returns>
		public virtual bool CopyTo(IFile dest) {
			// Copy to local FS
			if (dest is LocalFile) {
				this.ExportFile(dest.AbsolutePath);
				return true;
			}

			// Copy to other fs
			if (this.IsDirectory) {
				if (!dest.Exists)
					dest.MkDir();

				foreach (IFile fromFile in this.ListTree()) {
					IFile toFile = this.manager.GetFile(dest.AbsolutePath, fromFile.AbsolutePath.Substring(this.AbsolutePath.Length + 1));

					if (fromFile.IsFile)
						IOUtils.StreamFromTo(fromFile.Open(FileMode.Open), toFile.Open(FileMode.Create), 1024);
					else
						toFile.MkDir();
				}
			} else
				IOUtils.StreamFromTo(this.Open(FileMode.Open), dest.Open(FileMode.Create), 1024);

			return true;
		}

		/// <summary>
		///  Renames/Moves this file to the specified file instance.
		/// </summary>
		/// <param name="dest">File to rename/move to.</param>
		/// <returns>boolean true - success, false - failure</returns>
		public virtual bool RenameTo(IFile dest) {
			// Copy to local FS
			if (dest is LocalFile) {
				this.ExportFile(dest.AbsolutePath);
				this.Delete(true);
				return true;
			}

			// Move to other fs
			if (this.IsDirectory) {
				if (!dest.Exists)
					dest.MkDir();

				foreach (IFile fromFile in this.ListTree()) {
					IFile toFile = this.manager.GetFile(dest.AbsolutePath, fromFile.AbsolutePath.Substring(this.AbsolutePath.Length + 1));

					if (fromFile.IsFile)
						IOUtils.StreamFromTo(fromFile.Open(FileMode.Open), toFile.Open(FileMode.Create), 1024);
					else
						toFile.MkDir();
				}
			} else
				IOUtils.StreamFromTo(this.Open(FileMode.Open), dest.Open(FileMode.Create), 1024);

			this.Delete(true);

			return true;
		}

		/// <summary>Deletes the file.</summary>
		public virtual bool Delete() {
		 	return false;
		}

		/// <summary>Deletes the file and any files within the directory if it's a directory.</summary>
		/// <param name="deep">If set to true, files within the directory are deleted too.</param>
		public virtual bool Delete(bool deep) {
			return false;
		}

		/// <summary>List files in the specified directory.</summary>
		/// <returns>List of files within the directory.</returns>
		public virtual IFile[] ListFiles() {
			return this.ListFilesFiltered(null);
		}

		/// <summary>List files by the specified filter.</summary>
		/// <param name="filter">Instance to filter by.</param>
		/// <returns>Filter list of files.</returns>
		public virtual IFile[] ListFilesFiltered(IFileFilter filter) {
			return null;
		}

		/// <summary>Lists a tree of files within a directory.</summary>
		/// <returns>List of files within the tree.</returns>
		public virtual IFile[] ListTree() {
		 	return this.ListTreeFiltered(null);
		}

		/// <summary>
		/// Lists a tree of tiles within a directory by the specified filter.
		/// </summary>
		/// <param name="filter">Instance to filter by.</param>
		/// <returns>Returns a filtered list of files.</returns>
		public virtual IFile[] ListTreeFiltered(IFileFilter filter) {
		 	this.fileTree = new ArrayList();

			this.ListTree(this, filter, 0);

			return (IFile[]) this.fileTree.ToArray(typeof(LocalFile));
		}

		/// <summary>
		///  Creates a new directory.
		/// </summary>
		/// <returns>true - success, false - failure</returns>
		public virtual bool MkDir() {
			return false;
		}

		/// <summary>A merged name/value array of config elements.</summary>
		public virtual ManagerConfig Config {
		 	get {
		 		return this.manager.Config;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="mode"></param>
		/// <returns></returns>
		public virtual Stream Open(FileMode mode) {
			return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="action"></param>
		public void DispatchBeforeFileAction(FileAction action) {
			DispatchBeforeFileAction(action, null);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="action"></param>
		/// <param name="file2"></param>
		public void DispatchBeforeFileAction(FileAction action, IFile file2) {
			if (this.triggerEventsState)
				this.manager.DispatchEvent(EventType.BeforeFileAction, action, this, file2);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="action"></param>
		public void DispatchFileAction(FileAction action) {
			DispatchFileAction(action, null);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="action"></param>
		/// <param name="file2"></param>
		public void DispatchFileAction(FileAction action, IFile file2) {
			if (this.triggerEventsState)
				this.manager.DispatchEvent(EventType.FileAction, action, this, file2);
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

		#endregion
	}
}
