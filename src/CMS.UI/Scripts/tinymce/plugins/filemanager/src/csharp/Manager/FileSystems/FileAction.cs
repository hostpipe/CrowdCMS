/*
 * $Id: FileAction.cs 9 2007-05-27 10:47:07Z spocke $
 *
 * Copyright © 2007, Moxiecode Systems AB, All rights reserved. 
 */

using System.Collections.Specialized;

namespace Moxiecode.Manager.FileSystems {
 	/// <summary>
 	///  This enum contains all diffrent file event action types.
 	/// </summary>
 	public enum FileAction {
 		 /// <summary>Delete action contant.</summary>
		Delete,

		/// <summary>Add new directory action contant.</summary>
		Add,

		/// <summary>Update/modify file/directory action contant.</summary>
		Update,

		/// <summary>Rename file/directory action contant.</summary>
		Rename,

		/// <summary>Copy file/directory action contant.</summary>
		Copy,

		/// <summary>Make directory action contant.</summary>
		MkDir,

		/// <summary>Remove directory action contant.</summary>
		RmDir
 	}
}
