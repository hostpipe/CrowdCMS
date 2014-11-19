/*
 * $Id: IFileFilter.cs 9 2007-05-27 10:47:07Z spocke $
 *
 * Copyright © 2007, Moxiecode Systems AB, All rights reserved. 
 */

namespace Moxiecode.Manager.FileSystems {
 	/// <summary>
 	///  Implementations of this interface is used to filter out files from a file listning.
 	/// </summary>
	public interface IFileFilter {
		/// <summary>
		///  Returns true or false if the file is accepted or not.
		/// </summary>
		/// <param name="file">File to grant or deny.</param>
		/// <returns>true or false if the file is accepted or not.</returns>
		bool Accept(IFile file);
	}
}
