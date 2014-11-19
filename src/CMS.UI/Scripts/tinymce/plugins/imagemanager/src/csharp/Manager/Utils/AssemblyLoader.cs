/*
 * $Id: AssemblyLoader.cs 862 2012-04-11 12:33:47Z spocke $
 *
 * Copyright © 2007, Moxiecode Systems AB, All rights reserved. 
 */

using System;
using System.IO;
using System.Collections;
using System.Reflection;

namespace Moxiecode.Manager.Utils {
	/// <summary>
	/// Description of AssemblyLoader.
	/// </summary>
	public class AssemblyLoader {
		private static ArrayList assemblies = new ArrayList();

		/// <summary>
		/// 
		/// </summary>
		public AssemblyLoader() {
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		public void AddFile(string path) {
			foreach (AssemblyReference asmRef in assemblies) {
				if (asmRef.Path == path)
					return;
			}

			assemblies.Add(new AssemblyReference(path));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public object CreateInstance(string name) {
			object obj;
			Type type;
			string[] parts = name.Split(',');

			// Ask normal dll with named assembly
			try {
				if ((type = System.Type.GetType(name)) != null) {
					if ((obj = Activator.CreateInstance(type)) != null)
						return obj;
				}
			} catch {
				// Ignore
			}

			// Ask normal dll with core assembly
			try {
				if ((type = System.Type.GetType(parts[0])) != null) {
					if ((obj = Activator.CreateInstance(type)) != null)
						return obj;
				}
			} catch {
				// Ignore
			}

			// Ask loaded dlls
			foreach (AssemblyReference asmRef in assemblies) {
				asmRef.Reload();

				if ((obj = asmRef.Assembly.CreateInstance(parts[0])) != null)
					return obj;
			}

			return null;
		}
	}

	class AssemblyReference {
		private Assembly assembly;
		private string path;
		private DateTime lastMod;

		public AssemblyReference(string path) {
			this.path = path;
			//this.LoadDLL();
			lastMod = DateTime.Now;
		}

		public string Path {
			get { return path; }
		}

		public Assembly Assembly {
			get {
				return this.assembly;
			}
		}

		public void Reload() {
			if (File.GetLastWriteTime(this.path) != this.lastMod)
				this.LoadDLL();
		}

		private void LoadDLL() {
			Stream stream = null;
			MemoryStream asmBuff = new MemoryStream((int) new FileInfo(this.path).Length);
			byte[] buff = new byte[2048];
			int len;

			try {
				stream = File.OpenRead(this.path);

				while ((len = stream.Read(buff, 0, buff.Length)) != 0)
					asmBuff.Write(buff, 0, (int) len);

				asmBuff.Flush();
				buff = asmBuff.ToArray();
				this.assembly = Assembly.Load(buff);
				this.lastMod = File.GetLastWriteTime(this.path);
			} catch (BadImageFormatException ex) {
				throw new Exception("Failed to load DLL: " + this.path + ", Length: " + buff.Length, ex);
			} finally {
				if (stream != null)
					stream.Close();
			}
		}
	}
}
