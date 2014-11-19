/*
 * $Id: ManagerConfig.cs 9 2007-05-27 10:47:07Z spocke $
 *
 * Copyright © 2007, Moxiecode Systems AB, All rights reserved. 
 */

using System;
using System.Web;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using Moxiecode.Manager.FileSystems;
using Moxiecode.Manager.Utils;
using System.Text.RegularExpressions;

namespace Moxiecode.Manager {
	/// <summary>
	/// 
	/// </summary>
	public class ManagerConfig : NameValueCollection {
		private StringList plugins;

		/// <summary>
		/// 
		/// </summary>
		public ManagerConfig() : base() {
			this.plugins = new StringList();
		}

		/// <summary>
		/// 
		/// </summary>
		public StringList Plugins {
			get {
				return this.plugins;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public string Get(string name, string def) {
			if (this[name] == null || this[name].Length == 0)
				return def;

			return this[name];
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public bool GetBool(string name, bool def) {
			if (this[name] == null || this[name].Length == 0)
				return def;

			return this[name].Trim().ToLower() == "true";
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public int GetInt(string name, int def) {
			if (this[name] == null || this[name].Length == 0)
				return def;

			return (int) Convert.ToInt32(this[name].Trim());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool HasItem(string name, string item) {
			if (this[name] == null)
				return false;

			foreach (string chunk in this[name].Split(new char[]{','})) {
				if (item.Trim() == chunk.Trim())
					return true;
			}

			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public ManagerConfig Clone() {
			ManagerConfig newConf = new ManagerConfig();

			lock (this) {
				foreach (string val in this.plugins)
					newConf.plugins.Add(val);

				foreach (string key in this.AllKeys)
					newConf[key] = this[key];
			}

			return newConf;
		}
	}
}
