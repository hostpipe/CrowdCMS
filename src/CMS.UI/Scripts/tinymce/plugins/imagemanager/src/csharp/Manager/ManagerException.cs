/*
 * $Id: ManagerException.cs 9 2007-05-27 10:47:07Z spocke $
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
	///  Error level enum.
	/// </summary>
	public enum ManagerErrorLevel {
		/// <summary>Non critical error.</summary>
		Error,

		/// <summary>Critical error.</summary>
		Fatal
	}

	/// <summary>
	///  Manager exception class.
	/// </summary>
	public class ManagerException : Exception {
		private ManagerErrorLevel level;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="msg"></param>
		public ManagerException(string msg) : this(ManagerErrorLevel.Fatal, msg) {
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="level"></param>
		/// <param name="msg"></param>
		public ManagerException(ManagerErrorLevel level, string msg) : base(msg) {
			this.level = level;
		}

		/// <summary>
		/// 
		/// </summary>
		public ManagerErrorLevel Level {
			get { return level; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string LevelName {
			get {
				switch (this.level) {
					case ManagerErrorLevel.Error:
						return "ERROR";

					case ManagerErrorLevel.Fatal:
						return "FATAL";
				}

				return "UNKNOWN";
			}
		}
	}
}
