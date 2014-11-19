/*
 * $Id: IPlugin.cs 9 2007-05-27 10:47:07Z spocke $
 *
 * Copyright © 2007, Moxiecode Systems AB, All rights reserved. 
 */

using System;
using Moxiecode.Manager;
using Moxiecode.Manager.FileSystems;
using Moxiecode.Manager.Utils;
using System.Collections;
using System.Collections.Specialized;

namespace Moxiecode.Manager {
	/// <summary>
	/// Description of IPlugin.
	/// </summary>
	public interface IPlugin {
		/**
		 * Prefix that the events will be filtered on.
		 */
		string Prefix { get; }

		/// <summary>
		/// 
		/// </summary>
		string ShortName { get; }

		/**
		 * Gets called on a authenication request. This method should check sessions or simmilar to
		 * verify that the user has access to the backend.
		 *
		 * This method should return true if the current request is authenicated or false if it's not.
		 *
		 * @param MCManager $man MCManager reference that the plugin is assigned to.
		 * @return bool true/false if the user is authenticated.
		 */
		bool OnAuthenticate(ManagerEngine man);

		/**
		 * Gets called after any authenication is performed and verified.
		 * This method should return false if the execution chain is to be broken.
		 *
		 * @param MCManager $man MCManager reference that the plugin is assigned to.
		 * @return bool true/false if the execution of the event chain should continue.
		 */
		bool OnPreInit(ManagerEngine man, string prefix);

		/**
		 * Gets called after any authenication is performed and verified.
		 * This method should return false if the execution chain is to be broken.
		 *
		 * @param MCManager $man MCManager reference that the plugin is assigned to.
		 * @return bool true/false if the execution of the event chain should continue.
		 */
		bool OnInit(ManagerEngine man);

		/**
		 * Gets called when a user has logged in to the system. This event should be dispatched from the login page.
		 * These events is not fired internaly and should be fired/dispatched externally.
		 *
		 * @param MCManager $man MCManager reference that the plugin is assigned to.
		 * @return bool true/false if the execution of the event chain should continue.
		 */
		bool OnLogin(ManagerEngine man);

		/**
		 * Gets called when a user has logged out from the system. This event should be dispatched from the logout page.
		 * These events is not fired internaly and should be fired/dispatched externally.
		 *
		 * @param MCManager $man MCManager reference that the plugin is assigned to.
		 * @return bool true/false if the execution of the event chain should continue.
		 */
		bool OnLogout(ManagerEngine man);

		/**
		 * Gets called before a file action occurs for example before a rename or copy.
		 *
		 * @param MCManager $man MCManager reference that the plugin is assigned to.
		 * @param int $action File action constant for example DELETE_ACTION.
		 * @param string $file1 File object 1 for example from in a copy operation.
		 * @param string $file2 File object 2 for example to in a copy operation. Might be null in for example a delete.
		 * @return bool true/false if the execution of the event chain should continue.
		 */
		bool OnBeforeFileAction(ManagerEngine man, FileAction action, IFile file1, IFile file2);

		/**
		 * Gets called after a file action was perforem for example after a rename or copy.
		 *
		 * @param MCManager $man MCManager reference that the plugin is assigned to.
		 * @param int $action File action constant for example DELETE_ACTION.
		 * @param string $file1 File object 1 for example from in a copy operation.
		 * @param string $file2 File object 2 for example to in a copy operation. Might be null in for example a delete.
		 * @return bool true/false if the execution of the event chain should continue.
		 */
		bool OnFileAction(ManagerEngine man, FileAction action, IFile file1, IFile file2);

		/**
		 * Gets called before a RPC command is handled.
		 *
		 * @param MCManager $man MCManager reference that the plugin is assigned to.
		 * @param string $cmd RPC Command to be executed.
		 * @param object $input RPC input object data.
		 * @return bool true/false if the execution of the event chain should continue.
		 */
		bool OnBeforeRPC(ManagerEngine man, string cmd, Hashtable input);

		/**
		 * Gets executed when a RPC command is to be executed.
		 *
		 * @param MCManager $man MCManager reference that the plugin is assigned to.
		 * @param string $cmd RPC Command to be executed.
		 * @param object $input RPC input object data.
		 * @return object Result data from RPC call or null if it should be passed to the next handler in chain.
		 */
		object OnRPC(ManagerEngine man, string cmd, Hashtable input);

		/**
		 * Gets called before data is streamed to client.
		 *
		 * @param MCManager $man MCManager reference that the plugin is assigned to.
		 * @param string $cmd Stream command that is to be performed.
		 * @param string $input Array of input arguments.
		 * @return bool true/false if the execution of the event chain should continue.
		 */
		bool OnBeforeStream(ManagerEngine man, string cmd, NameValueCollection input);

		/**
		 * Gets called when data is streamed to client. This method should setup
		 * HTTP headers, content type etc and simply send out the binary data to the client and the return false
		 * ones that is done.
		 *
		 * @param MCManager $man MCManager reference that the plugin is assigned to.
		 * @param string $cmd Stream command that is to be performed.
		 * @param string $input Array of input arguments.
		 * @return bool true/false if the execution of the event chain should continue.
		 */
		bool OnStream(ManagerEngine man, string cmd, NameValueCollection input);

		/**
		 * Gets called after data was streamed to client.
		 *
		 * @param MCManager $man MCManager reference that the plugin is assigned to.
		 * @param string $cmd Stream command that is to was performed.
		 * @param string $input Array of input arguments.
		 * @return bool true/false if the execution of the event chain should continue.
		 */
		bool OnAfterStream(ManagerEngine man, string cmd, NameValueCollection input);

		/**
		 * Gets called before data is streamed/uploaded from client.
		 *
		 * @param MCManager $man MCManager reference that the plugin is assigned to.
		 * @param string $cmd Upload command that is to be performed.
		 * @param string $input Array of input arguments.
		 * @return bool true/false if the execution of the event chain should continue.
		 */
		bool OnBeforeUpload(ManagerEngine man, string cmd, NameValueCollection input);

		/**
		 * Gets called when data is streamed/uploaded from client. This method should take care of
		 * any uploaded files and move them to the correct location.
		 *
		 * @param MCManager $man MCManager reference that the plugin is assigned to.
		 * @param string $cmd Upload command that is to be performed.
		 * @param string $input Array of input arguments.
		 * @return object Result object data or null if the event wasn't handled.
		 */
		object OnUpload(ManagerEngine man, string cmd, NameValueCollection input);

		/**
		 * Gets called before data is streamed/uploaded from client.
		 *
		 * @param MCManager $man MCManager reference that the plugin is assigned to.
		 * @param string $cmd Upload command that is to was performed.
		 * @param string $input Array of input arguments.
		 * @return bool true/false if the execution of the event chain should continue.
		 */
		bool OnAfterUpload(ManagerEngine man, string cmd, NameValueCollection input);

		/**
		 * Gets called when custom data is to be added for a file custom data can for example be
		 * plugin specific name value items that should get added into a file listning.
		 *
		 * @param MCManager $man MCManager reference that the plugin is assigned to.
		 * @param BaseFile $file File reference to add custom info/data to.
		 * @param string $type Where is the info needed for example list or info.
		 * @param Array $custom Name/Value array to add custom items to.
		 * @return bool true/false if the execution of the event chain should continue.
		 */
		bool OnCustomInfo(ManagerEngine man, IFile file, string type, Hashtable custom);

		/**
		 * Gets called when the user selects a file and inserts it into TinyMCE or a form or similar.
		 *
		 * @param MCManager $man MCManager reference that the plugin is assigned to.
		 * @param BaseFile $file Implementation of the BaseFile class that was inserted/returned to external system.
		 * @return bool true/false if the execution of the event chain should continue.
		 */
		bool OnInsertFile(ManagerEngine man, IFile file);
	}
}
