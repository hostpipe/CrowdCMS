/*
 * $Id: IFile.cs 9 2007-05-27 10:47:07Z spocke $
 *
 * Copyright © 2007, Moxiecode Systems AB, All rights reserved. 
 */

using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;

namespace Moxiecode.Manager.FileSystems {
	/// <summary>
	/// 
	/// </summary>
	public enum FileType {
		/// <summary></summary>
		Unknown,

		/// <summary></summary>
		File,

		/// <summary></summary>
		Directory
	}

 	/// <summary>
 	///  This interface is to be implemented by various file systems.
 	/// </summary>
	public interface IFile {
		/// <summary>Absolute path of parent directory.</summary>
		string Parent { get; }

		/// <summary>State if file action events should be fired or not.</summary>
		bool TriggerEvents { get; set; }

		/// <summary>IFile of parent directory.</summary>
		IFile ParentFile { get; }

		/// <summary>File/directory name.</summary>
		string Name { get; }

		/// <summary>Absolute file/directory path.</summary>
		string AbsolutePath { get; }

		/// <summary>Imports a local file to the file system, for example when users upload files.</summary>
		/// <param name="local_absolute_path">Absolute path to local file.</param>
		void ImportFile(string local_absolute_path);

		/// <summary>Exports the file to the local file system.</summary>
		/// <param name="local_absolute_path">Absolute path to local file.</param>
		void ExportFile(string local_absolute_path);

		///<summary>File exists property, if true the file/directory exists.</summary>
		bool Exists { get; }

		/// <summary>Is directory property, is true if the file instance is a directory.</summary>
		bool IsDirectory { get; }

		/// <summary>Is file property, is true if the file instance is a file.</summary>
		bool IsFile { get; }

		/// <summary>Last modification date of file/directory.</summary>
		DateTime LastModified { get; set; }

		/// <summary>Creation date of file/directory.</summary>
		DateTime CreationDate { get; }

		/// <summary>Read access property, is true if the file is readable.</summary>
		bool CanRead { get; }
		
		/// <summary>Write access property, if true if the file is writable.</summary>
		bool CanWrite { get; }

		/// <summary>File size in bytes of file.</summary>
		long Length { get; }

		/// <summary>Copies the file to the destination file.</summary>
		/// <param name="dest">Destination file to copy to.</param>
		bool CopyTo(IFile dest);

		/// <summary>Deletes the file.</summary>
		bool Delete();

		/// <summary>Deletes the file and any files within the directory if it's a directory.</summary>
		/// <param name="deep">If set to true, files within the directory are deleted too.</param>
		bool Delete(bool deep);

		/// <summary>List files in the specified directory.</summary>
		/// <returns>List of files within the directory.</returns>
		IFile[] ListFiles();

		/// <summary>List files by the specified filter.</summary>
		/// <param name="filter">Instance to filter by.</param>
		/// <returns>Filter list of files.</returns>
		IFile[] ListFilesFiltered(IFileFilter filter);

		/// <summary>Lists a tree of files within a directory.</summary>
		/// <returns>List of files within the tree.</returns>
		IFile[] ListTree();

		/// <summary>
		/// Lists a tree of tiles within a directory by the specified filter.
		/// </summary>
		/// <param name="filter">Instance to filter by.</param>
		/// <returns>Returns a filtered list of files.</returns>
		IFile[] ListTreeFiltered(IFileFilter filter);

		/// <summary>
		///  Creates a new directory.
		/// </summary>
		/// <returns>true - success, false - failure</returns>
		bool MkDir();

		/// <summary>
		///  Renames/Moves this file to the specified file instance.
		/// </summary>
		/// <param name="dest">File to rename/move to.</param>
		/// <returns>boolean true - success, false - failure</returns>
		bool RenameTo(IFile dest);

		/// <summary>A merged name/value array of config elements.</summary>
		ManagerConfig Config { get; }

		/// <summary>A merged name/value array of config elements.</summary>
		/// <param name="mode">Mode to open file by, r, rb, w, wb etc.</param>
		/// <returns>File stream implementation for the file system.</returns>
		Stream Open(FileMode mode);
	}
}
