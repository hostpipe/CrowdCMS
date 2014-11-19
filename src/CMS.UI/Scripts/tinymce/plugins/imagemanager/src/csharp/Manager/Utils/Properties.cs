/*
 * $Id: Properties.cs 804 2010-05-06 14:18:30Z spocke $
 *
 * Copyright © 2007, Moxiecode Systems AB, All rights reserved. 
 */

using System;
using System.IO;
using System.Collections.Specialized;
using System.Configuration;

namespace Moxiecode.Manager.Utils {
	/// <summary>
	///  This class is a INI style property parser. Much like the properties class in Java.
	/// </summary>
	public class Properties : NameValueCollection {
		/// <summary>
		///  Loads a property file into internal name/value collection.
		///  The file format of these property files are simmilar to INI files.
		/// </summary>
		/// <param name="filepath">Full path to filename to load.</param>
		public void Load(string filepath) {
			if (!File.Exists(filepath))
				throw new Exception("Could not load properties file \"" + filepath + "\".");

			// Load language file
			StreamReader inStream = File.OpenText(PathUtils.ToOSPath(filepath));
			string line;
			while ((line = inStream.ReadLine()) != null) {
				// Skip comment
				if (line.StartsWith("#"))
					continue;

				// Split line
				int pos = line.IndexOf('=');
				if (pos != -1) {
					this.Add(line.Substring(0, pos), line.Substring(pos + 1));
				}
			}

			inStream.Close();
		}
	}
}
