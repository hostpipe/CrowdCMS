/*
 * $Id: Plugin.cs 9 2007-05-27 10:47:07Z spocke $
 *
 * Copyright © 2007, Moxiecode Systems AB, All rights reserved. 
 */

using System;
using Moxiecode.Manager.FileSystems;
using Moxiecode.Manager.Utils;
using System.Collections;
using System.Collections.Specialized;

namespace Moxiecode.Manager {
	/// <summary>
	/// Description of Plugin.
	/// </summary>
	public abstract class Plugin : IPlugin {
		/// <summary>
		///  Prefix that the events will be filtered on. Defaults to null.
		/// </summary>
		public virtual string Prefix {
			get {
				return null;
			}
		}

		/// <summary>
		///  Short name for the plugin, used in the authenticator config option for example
		///  so that you don't need to write the long name for it namespace.classname.
		/// </summary>
		public virtual string ShortName {
			get {
				return null;
			}
		}

		/// <summary>
		///  Gets called on a authenication request. This method should check sessions or simmilar to verify that the user has access to the backend.
		///  This method should return true if the current request is authenicated or false if it's not.
		/// </summary>
		/// <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		/// <returns>true/false if the user is authenticated</returns>
		public virtual bool OnAuthenticate(ManagerEngine man) {
			return false;
		}

		/// <summary>
		///  Gets called before the ManagerEngine is initialized. This method
		///  can setup config and language pack data based on the prefix.
		/// </summary>
		/// <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		/// <param name="prefix">Prefix to use when setting up config etc.</param>
		/// <returns>true/false if the user is authenticated</returns>
		public virtual bool OnPreInit(ManagerEngine man, string prefix) {
			return true;
		}

		/// <summary>
		///  Gets called after any authenication is performed and verified.
		///  This method should return false if the execution chain is to be broken.
		/// </summary>
		/// <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		/// <returns>true/false if the execution of the event chain should continue execution.</returns>
		public virtual bool OnInit(ManagerEngine man) {
			return true;
		}

		/// <summary>
		///  Gets called when a user has logged in to the system. This event should be dispatched from the login page.
		///  These events is not fired internaly and should be fired/dispatched externally.
		/// </summary>
		/// <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		/// <returns>true/false if the execution of the event chain should continue execution.</returns>
		public virtual bool OnLogin(ManagerEngine man) {
			return true;
		}

		/// <summary>
		///  Gets called when a user has logged out from the system. This event should be dispatched from the logout page.
		///  These events is not fired internaly and should be fired/dispatched externally.
		/// </summary>
		/// <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		/// <returns>true/false if the execution of the event chain should continue execution.</returns>
		public virtual bool OnLogout(ManagerEngine man) {
			return true;
		}

		/// <summary>
		///  Gets called before a file action occurs for example before a rename or copy.
		/// </summary>
		/// <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		/// <param name="action">File action type.</param>
		/// <param name="file1">File object 1 for example from in a copy operation.</param>
		/// <param name="file2">File object 2 for example to in a copy operation. Might be null in for example a delete.</param>
		/// <returns>true/false if the execution of the event chain should continue execution.</returns>
		public virtual bool OnBeforeFileAction(ManagerEngine man, FileAction action, IFile file1, IFile file2) {
			return true;
		}

		/// <summary>
		///  Gets called after a file action was perforem for example after a rename or copy.
		/// </summary>
		/// <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		/// <param name="action">File action type.</param>
		/// <param name="file1">File object 1 for example from in a copy operation.</param>
		/// <param name="file2">File object 2 for example to in a copy operation. Might be null in for example a delete.</param>
		/// <returns>true/false if the execution of the event chain should continue execution.</returns>
		public virtual bool OnFileAction(ManagerEngine man, FileAction action, IFile file1, IFile file2) {
			return true;
		}

		/// <summary>
		///  Gets called before a RPC command is handled.
		/// </summary>
		/// <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		/// <param name="cmd">RPC Command to be executed.</param>
		/// <param name="input">RPC input object data.</param>
		/// <returns>true/false if the execution of the event chain should continue execution.</returns>
		public virtual bool OnBeforeRPC(ManagerEngine man, string cmd, Hashtable input) {
			return true;
		}

		/// <summary>
		///  Gets executed when a RPC command is to be executed.
		/// </summary>
		/// <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		/// <param name="cmd">RPC Command to be executed.</param>
		/// <param name="input">RPC input object data.</param>
		/// <returns>true/false if the execution of the event chain should continue execution.</returns>
		public virtual object OnRPC(ManagerEngine man, string cmd, Hashtable input) {
			return null;
		}

		/// <summary>
		///  Gets called before data is streamed to client.
		/// </summary>
		/// <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		/// <param name="cmd">Stream command that is to be performed.</param>
		/// <param name="input">Array of input arguments.</param>
		/// <returns>true/false if the execution of the event chain should continue execution.</returns>
		public virtual bool OnBeforeStream(ManagerEngine man, string cmd, NameValueCollection input) {
			return true;
		}

		/// <summary>
		///  Gets called when data is streamed to client. This method should setup HTTP headers, content type
		///  etc and simply send out the binary data to the client and the return false ones that is done.
		/// </summary>
		/// <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		/// <param name="cmd">Stream command that is to be performed.</param>
		/// <param name="input">Array of input arguments.</param>
		/// <returns>true/false if the execution of the event chain should continue execution.</returns>
		public virtual bool OnStream(ManagerEngine man, string cmd, NameValueCollection input) {
			return true;
		}

		/// <summary>
		///  Gets called after data was streamed to client.
		/// </summary>
		/// <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		/// <param name="cmd">Stream command that is to was performed.</param>
		/// <param name="input">Array of input arguments.</param>
		/// <returns>true/false if the execution of the event chain should continue execution.</returns>
		public virtual bool OnAfterStream(ManagerEngine man, string cmd, NameValueCollection input) {
			return true;
		}

		/// <summary>
		///  Gets called before data is streamed/uploaded from client.
		/// </summary>
		/// <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		/// <param name="cmd">Upload command that is to be performed.</param>
		/// <param name="input">Array of input arguments.</param>
		/// <returns>true/false if the execution of the event chain should continue execution.</returns>
		public virtual bool OnBeforeUpload(ManagerEngine man, string cmd, NameValueCollection input) {
			return true;
		}

		/// <summary>
		///  Gets called when data is streamed/uploaded from client. This method should take care of
		///  any uploaded files and move them to the correct location.
		/// </summary>
		/// <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		/// <param name="cmd">Upload command that is to be performed.</param>
		/// <param name="input">Array of input arguments.</param>
		/// <returns>true/false if the execution of the event chain should continue execution.</returns>
		public virtual object OnUpload(ManagerEngine man, string cmd, NameValueCollection input) {
			return null;
		}

		/// <summary>
		///  Gets called before data is streamed/uploaded from client.
		/// </summary>
		/// <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		/// <param name="cmd">Upload command that is to was performed.</param>
		/// <param name="input">Array of input arguments.</param>
		/// <returns>true/false if the execution of the event chain should continue execution.</returns>
		public virtual bool OnAfterUpload(ManagerEngine man, string cmd, NameValueCollection input) {
			return true;
		}

		/// <summary>
		///  Gets called when custom data is to be added for a file custom data can for example be
		///  plugin specific name value items that should get added into a file listning.
		/// </summary>
		/// <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		/// <param name="file">File reference to add custom info/data to.</param>
		/// <param name="type">Where is the info needed for example list or info.</param>
		/// <param name="custom">Name/Value array to add custom items to.</param>
		/// <returns>true/false if the execution of the event chain should continue execution.</returns>
		public virtual bool OnCustomInfo(ManagerEngine man, IFile file, string type, Hashtable custom) {
			return true;
		}

		/// <summary>
		///  Gets called when the user selects a file and inserts it into TinyMCE or a form or similar.
		/// </summary>
		/// <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		/// <param name="file">Implementation of the BaseFile class that was inserted/returned to external system.</param>
		/// <returns>true/false if the execution of the event chain should continue execution.</returns>
		public virtual bool OnInsertFile(ManagerEngine man, IFile file) {
			return true;
		}
	}
}
