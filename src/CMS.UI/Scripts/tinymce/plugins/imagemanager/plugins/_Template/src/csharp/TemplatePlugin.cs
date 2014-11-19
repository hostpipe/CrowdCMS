/*
 * Created by SharpDevelop.
 * User: spocke
 * Date: 2007-03-13
 * Time: 12:45
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Web;
using Moxiecode.Manager;
using Moxiecode.Manager.FileSystems;
using Moxiecode.Manager.Utils;

namespace Moxiecode.Manager.Plugins {
	/// <summary>
	///  This is a template plugin to be used to create new plugins. Rename all Template references below to your plugins name
	///  and implement the methods you need.
	/// </summary>
	public class TemplatePlugin : Plugin {
		public TemplatePlugin() {
		}

		/// <summary>
		///  Short name for the plugin, used in the authenticator config option for example
		///  so that you don't need to write the long name for it namespace.classname.
		/// </summary>
		public override string ShortName {
			get {
				return "Template";
			}
		}

		/// <summary>
		///  Gets called on a authenication request. This method should check sessions or simmilar to verify that the user has access to the backend.
		///  This method should return true if the current request is authenicated or false if it's not.
		/// </summary>
		/// <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		/// <returns>true/false if the user is authenticated</returns>
		public override bool OnAuthenticate(ManagerEngine man) {
			return false;
		}

		/// <summary>
		///  Gets called after any authenication is performed and verified.
		///  This method should return false if the execution chain is to be broken.
		/// </summary>
		/// <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		/// <returns>true/false if the execution of the event chain should continue execution.</returns>
		public override bool OnInit(ManagerEngine man) {
			ManagerConfig config = man.Config;

			// Override a config option
			config["somegroup.someoption"] = "somevalue";

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
		public override bool OnBeforeFileAction(ManagerEngine man, FileAction action, IFile file1, IFile file2) {
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
		public override bool OnFileAction(ManagerEngine man, FileAction action, IFile file1, IFile file2) {
			return true;
		}

		/// <summary>
		///  Gets called before a RPC command is handled.
		/// </summary>
		/// <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		/// <param name="cmd">RPC Command to be executed.</param>
		/// <param name="input">RPC input object data.</param>
		/// <returns>true/false if the execution of the event chain should continue execution.</returns>
		public override bool OnBeforeRPC(ManagerEngine man, string cmd, Hashtable input) {
			return true;
		}

		/// <summary>
		///  Gets executed when a RPC command is to be executed.
		/// </summary>
		/// <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		/// <param name="cmd">RPC Command to be executed.</param>
		/// <param name="input">RPC input object data.</param>
		/// <returns>true/false if the execution of the event chain should continue execution.</returns>
		public override object OnRPC(ManagerEngine man, string cmd, Hashtable input) {
			return null;
		}

		/// <summary>
		///  Gets called before data is streamed to client.
		/// </summary>
		/// <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		/// <param name="cmd">Stream command that is to be performed.</param>
		/// <param name="input">Array of input arguments.</param>
		/// <returns>true/false if the execution of the event chain should continue execution.</returns>
		public override bool OnBeforeStream(ManagerEngine man, string cmd, NameValueCollection input) {
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
		public override bool OnStream(ManagerEngine man, string cmd, NameValueCollection input) {
			return true;
		}

		/// <summary>
		///  Gets called after data was streamed to client.
		/// </summary>
		/// <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		/// <param name="cmd">Stream command that is to was performed.</param>
		/// <param name="input">Array of input arguments.</param>
		/// <returns>true/false if the execution of the event chain should continue execution.</returns>
		public override bool OnAfterStream(ManagerEngine man, string cmd, NameValueCollection input) {
			return true;
		}

		/// <summary>
		///  Gets called before data is streamed/uploaded from client.
		/// </summary>
		/// <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		/// <param name="cmd">Upload command that is to be performed.</param>
		/// <param name="input">Array of input arguments.</param>
		/// <returns>true/false if the execution of the event chain should continue execution.</returns>
		public override bool OnBeforeUpload(ManagerEngine man, string cmd, NameValueCollection input) {
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
		public override object OnUpload(ManagerEngine man, string cmd, NameValueCollection input) {
			return null;
		}

		/// <summary>
		///  Gets called before data is streamed/uploaded from client.
		/// </summary>
		/// <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		/// <param name="cmd">Upload command that is to was performed.</param>
		/// <param name="input">Array of input arguments.</param>
		/// <returns>true/false if the execution of the event chain should continue execution.</returns>
		public override bool OnAfterUpload(ManagerEngine man, string cmd, NameValueCollection input) {
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
		public override bool OnCustomInfo(ManagerEngine man, IFile file, string type, Hashtable custom) {
			return true;
		}

		/// <summary>
		///  Gets called when the user selects a file and inserts it into TinyMCE or a form or similar.
		/// </summary>
		/// <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		/// <param name="file">Implementation of the BaseFile class that was inserted/returned to external system.</param>
		/// <returns>true/false if the execution of the event chain should continue execution.</returns>
		public override bool OnInsertFile(ManagerEngine man, IFile file) {
			return true;
		}
	}
}
