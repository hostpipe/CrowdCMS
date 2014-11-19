/*
 * $Id: ConfigHandler.cs 172 2007-12-05 18:07:04Z spocke $
 *
 * Copyright © 2007, Moxiecode Systems AB, All rights reserved. 
 */

using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Configuration;
using System.Xml;
using System.Web;
using System.Reflection;
using Moxiecode.Manager;
using Moxiecode.Manager.Utils;

namespace Moxiecode.Manager {
	/// <summary>
	///  Handles configuration specific for the manager and it's subcomponents.
	/// </summary>
	public class ConfigHandler : IConfigurationSectionHandler {
		object IConfigurationSectionHandler.Create(object parent, object config_context, XmlNode section) {
			ManagerConfig config = new ManagerConfig();
			AssemblyLoader loader = new AssemblyLoader();
			HttpContext context = HttpContext.Current;
			bool found = false;

			// Add all dlls in plugin dir
			foreach (string file in this.FindDLLs(context.Request.MapPath(@"plugins"), new ArrayList())) {
				//System.Web.HttpContext.Current.Trace.Write(file);

				loader.AddFile(file);
				found = true;
			}

			// Add all dlls in plugin dir
			if (!found) {
				foreach (string file in this.FindDLLs(context.Request.MapPath(@"..\plugins"), new ArrayList())) {
					//System.Web.HttpContext.Current.Trace.Write(file);

					loader.AddFile(file);
				}
			}

			foreach (XmlElement pluginElm in section.SelectNodes("plugins/plugin")) {
				// Load plugin dll
				if (pluginElm.GetAttribute("file") != "")
					loader.AddFile(PathUtils.ToUnixPath(pluginElm.GetAttribute("file")));

				// Add class
				config.Plugins.Add(pluginElm.GetAttribute("class"));
			}

			foreach (XmlElement addElm in section.SelectNodes("config/add"))
				config.Add(addElm.GetAttribute("key"), addElm.GetAttribute("value"));

			return config;
		}

		#region private methods
		
		private ArrayList FindDLLs(string path, ArrayList paths) {
			if (!Directory.Exists(path))
				return paths;

			string[] dirs = Directory.GetDirectories(path);
			string[] files = Directory.GetFiles(path, "*.dll");

			foreach (string file in files)
				paths.Add(PathUtils.ToUnixPath(file));

			foreach (string dir in dirs) {
				if (dir.IndexOf(@"\_vti_cnf") == -1 && dir.IndexOf("/_vti_cnf") == -1)
					paths = FindDLLs(dir, paths);
			}

			return paths;
		}

		#endregion
	}
}
