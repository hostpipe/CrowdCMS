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
using System.Web;
using Moxiecode.Manager;
using Moxiecode.Manager.FileSystems;
using Moxiecode.Manager.Utils;

namespace Moxiecode.Manager.Plugins {
	/// <summary>
	/// Description of Favorites plugin.
	/// </summary>
	public class CustomInfoExamplePlugin : Plugin {
		public CustomInfoExamplePlugin() {
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="man"></param>
		/// <param name="file"></param>
		/// <param name="type"></param>
		/// <param name="info"></param>
		/// <returns></returns>
		public override bool OnCustomInfo(ManagerEngine man, IFile file, string type, Hashtable info) {
			switch (type) {
				case "insert":
				// Can be used by the insert_templates like this {$custom.mycustomfield}
				info["mycustomfield"] = file.Name.ToUpper();

				// Will be used as title/alt in TinyMCE link/image dialogs
				info["description"] = file.Name + "(" + StringUtils.GetSizeStr(file.Length) + ")";
				break;
			}

			// Pass to next handler
			return true;
		}
	}
}
