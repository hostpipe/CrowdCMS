/*
 * $Id: IFileFactory.cs 9 2007-05-27 10:47:07Z spocke $
 *
 * Copyright © 2007, Moxiecode Systems AB, All rights reserved. 
 */

using System;
using Moxiecode.Manager;

namespace Moxiecode.Manager.FileSystems {
	/// <summary>
	/// Description of IFileFactory.
	/// </summary>
	public interface IFileFactory {
		/// <summary>
		///  Returns a file based on path, child path and type.
		/// </summary>
		/// <param name="man">Manager engine reference.</param>
		/// <param name="path">Absolute path to file or directory.</param>
		/// <param name="child">Child path inside directory or empty string.</param>
		/// <param name="type">File type, force it to be a file or directory.</param>
		IFile GetFile(ManagerEngine man, string path, string child, FileType type);
	}
}
