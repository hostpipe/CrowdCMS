/*
 * $Id: MimeTypes.cs 9 2007-05-27 10:47:07Z spocke $
 *
 * Copyright © 2007, Moxiecode Systems AB, All rights reserved. 
 */

using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections;
using System.Configuration;

namespace Moxiecode.Manager.Utils {
	/// <summary>
	///  This class is a mime types parser. The file format that this parser loades is Apache style mime type files.
	/// </summary>
	public class MimeTypes {
		// Private class members
		private ArrayList data;

		/// <summary>
		///  Main constructor.
		/// </summary>
		public MimeTypes(String file_name) {
			this.data = new ArrayList();
			this.Load(file_name);
		}

		/// <summary>
		///  Loads a property file into internal name/value collection.
		///  The file format of these property files are simmilar to INI files.
		/// </summary>
		/// <param name="filepath">Full path to filename to load.</param>
		public void Load(string filepath) {
			if (!File.Exists(filepath))
				throw new Exception("Could not load mime types file \"" + filepath + "\".");

			// Load language file
			StreamReader inStream = File.OpenText(filepath);
			string line;
			while ((line = inStream.ReadLine()) != null) {
				// Skip comment
				if (line.StartsWith("#"))
					continue;

				this.data.Add(line);
			}

			inStream.Close();
		}

		/// <summary>
		///  Returns a content type or null depending in extension.
		/// </summary>
		/// <returns>Language value string.</returns>
		public string this[string extension]  {
			get {
				foreach (string line in this.data) {
					string[] chunks = new Regex("(\t+)|( +)").Split(line);

					for (int i=1; i<chunks.Length; i++) {
						char[] trimChars = {'\t', ' '};

						if (chunks[i].TrimEnd(trimChars) == extension)
							return chunks[0];
					}
				}

				return null;
			}
		}
	}
}
