/*
 * $Id: FileManagerPlugin.cs 432 2008-10-10 15:24:12Z spocke $
 *
 * Copyright © 2007, Moxiecode Systems AB, All rights reserved. 
 */

using System;
using System.IO;
using System.Collections;
using Moxiecode.Manager;
using Moxiecode.Manager.FileSystems;
using Moxiecode.FileManager.FileSystems;
using Moxiecode.Manager.Utils;

namespace Moxiecode.FileManager {
	/// <summary>
	/// Description of FileManager.
	/// </summary>
	public class FileManagerPlugin : Plugin {
		/// <summary></summary>
		public FileManagerPlugin() {
		}

		/// <summary>
		/// 
		/// </summary>
		public override string Prefix {
			get {
				return "fm";
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="man"></param>
		/// <param name="prefix"></param>
		/// <returns></returns>
		public override bool OnPreInit(ManagerEngine man, string prefix) {
			if (prefix == "fm")
				man.Config = ((ManagerConfig) man.ResolveConfig("Moxiecode.FileManager.FileManagerPlugin", "FileManagerPlugin")).Clone();

			return true; // Pass to next
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="man"></param>
		/// <returns></returns>
		public override bool OnInit(ManagerEngine man) {
			// Register local file system factroy
			man.FileSystems["zip"] = new ZipFileFactory();

			return true; // Pass to next
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="man"></param>
		/// <param name="cmd"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		public override object OnRPC(ManagerEngine man, string cmd, Hashtable input) {
			switch (cmd) {
				case "copyFiles":
					return this.CopyFiles(man, input);

				case "moveFiles":
					return this.MoveFiles(man, input);

				case "createDocs":
					return this.CreateDocs(man, input);

				case "createZip":
					return this.CreateZip(man, input);

				case "loadContent":
					return this.LoadContent(man, input);

				case "saveContent":
					return this.SaveContent(man, input);
			}

			return null;
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
				case "list":
					info["previewable"] = file.IsFile && man.VerifyFile(file, "preview");
					info["editable"] = file.IsFile && man.VerifyFile(file, "edit");
					break;
			}

			return true;
		}

		#region Private methods

		private ResultSet CopyFiles(ManagerEngine man, Hashtable input) {
			ResultSet rs = new ResultSet(new string[]{"status", "fromfile", "tofile", "message"});
			IFile fromFile, toFile;
			ManagerConfig toConfig;
			FileType fromType;

			for (int i=0; input["frompath" + i] != null; i++) {
				fromFile = man.GetFile(man.DecryptPath((string) input["frompath" + i]));
				fromType = fromFile.IsFile ? FileType.File : FileType.Directory;

				if (input["topath" + i] != null)
					toFile = man.GetFile(man.DecryptPath((string) input["topath" + i]), "", fromType);
				else {
					if (input["toname" + i] != null)
						toFile = man.GetFile(fromFile.Parent, (string) input["toname" + i], fromType);
					else
						toFile = man.GetFile(man.DecryptPath((string) input["topath"]), fromFile.Name, fromType);
				}

				toConfig = toFile.Config;

				if (!man.IsToolEnabled("copy", toConfig))
					throw new ManagerException("{#error.no_access}");

				if (!fromFile.Exists) {
					rs.Add("FAILED", man.EncryptPath(fromFile.AbsolutePath), man.EncryptPath(toFile.AbsolutePath), "{#error.no_from_file}");
					continue;
				}

				if (toConfig.GetBool("general.demo", false)) {
					rs.Add("FAILED", man.EncryptPath(fromFile.AbsolutePath), man.EncryptPath(toFile.AbsolutePath), "{#error.demo}");
					continue;
				}

				if (!man.VerifyFile(toFile, "copy")) {
					rs.Add("FAILED", man.EncryptPath(fromFile.AbsolutePath), man.EncryptPath(toFile.AbsolutePath), man.InvalidFileMsg);
					continue;
				}

				if (!toFile.CanWrite) {
					rs.Add("FAILED", man.EncryptPath(fromFile.AbsolutePath), man.EncryptPath(toFile.AbsolutePath), "{#error.no_write_access}");
					continue;
				}

				if (!toConfig.GetBool("filesystem.writable", true)) {
					rs.Add("FAILED", man.EncryptPath(fromFile.AbsolutePath), man.EncryptPath(toFile.AbsolutePath), "{#error.no_write_access}");
					continue;
				}

				if (toFile.Exists) {
					rs.Add("FAILED", man.EncryptPath(fromFile.AbsolutePath), man.EncryptPath(toFile.AbsolutePath), "{#error.tofile_exists}");
					continue;
				}

				// Zip check
				if (toFile is ZipFileImpl) {
					if (!man.VerifyFile(fromFile, "zip")) {
						rs.Add("FAILED", man.EncryptPath(fromFile.AbsolutePath), man.EncryptPath(toFile.AbsolutePath), man.InvalidFileMsg);
						continue;
					}
				}

				// Unzip check
				if (fromFile is ZipFileImpl) {
					if (!man.VerifyFile(toFile, "unzip")) {
						rs.Add("FAILED", man.EncryptPath(fromFile.AbsolutePath), man.EncryptPath(toFile.AbsolutePath), man.InvalidFileMsg);
						continue;
					}
				}

				if (fromFile.CopyTo(toFile))
					rs.Add("OK", man.EncryptPath(fromFile.AbsolutePath), man.EncryptPath(toFile.AbsolutePath), "{#message.copy_success}");
				else
					rs.Add("FAILED", man.EncryptPath(fromFile.AbsolutePath), man.EncryptPath(toFile.AbsolutePath), "{#error.copy_failed}");
			}

			return rs;
		}

		private ResultSet MoveFiles(ManagerEngine man, Hashtable input) {
			ResultSet rs = new ResultSet(new string[]{"status", "fromfile", "tofile", "message"});
			IFile fromFile, toFile;
			ManagerConfig toConfig;
			FileType fromType;

			for (int i=0; input["frompath" + i] != null; i++) {
				fromFile = man.GetFile(man.DecryptPath((string) input["frompath" + i]));
				fromType = fromFile.IsFile ? FileType.File : FileType.Directory;

				if (input["topath" + i] != null)
					toFile = man.GetFile(man.DecryptPath((string) input["topath" + i]), "", fromType);
				else {
					if (input["toname" + i] != null)
						toFile = man.GetFile(fromFile.Parent, (string) input["toname" + i], fromType);
					else
						toFile = man.GetFile(man.DecryptPath((string) input["topath"]), fromFile.Name, fromType);
				}

				toConfig = toFile.Config;

				if (!man.IsToolEnabled("cut", toConfig) && !man.IsToolEnabled("rename", toConfig))
					throw new ManagerException("{#error.no_access}");

				// From file missing
				if (!fromFile.Exists) {
					rs.Add("FAILED", man.EncryptPath(fromFile.AbsolutePath), man.EncryptPath(toFile.AbsolutePath), "{#error.no_from_file}");
					continue;
				}

				// User tried to change extension
				if (fromFile.IsFile && PathUtils.GetExtension(fromFile.Name) != PathUtils.GetExtension(toFile.Name)) {
					rs.Add("FAILED", man.EncryptPath(fromFile.AbsolutePath), man.EncryptPath(toFile.AbsolutePath), "{#error.move_failed}");
					continue;
				}

				if (toConfig.GetBool("general.demo", false)) {
					rs.Add("FAILED", man.EncryptPath(fromFile.AbsolutePath), man.EncryptPath(toFile.AbsolutePath), "{#error.demo}");
					continue;
				}

				if (!man.VerifyFile(toFile, "rename")) {
					rs.Add("FAILED", man.EncryptPath(fromFile.AbsolutePath), man.EncryptPath(toFile.AbsolutePath), man.InvalidFileMsg);
					continue;
				}

				if (!toFile.CanWrite) {
					rs.Add("FAILED", man.EncryptPath(fromFile.AbsolutePath), man.EncryptPath(toFile.AbsolutePath), "{#error.no_write_access}");
					continue;
				}

				if (!toConfig.GetBool("filesystem.writable", true)) {
					rs.Add("FAILED", man.EncryptPath(fromFile.AbsolutePath), man.EncryptPath(toFile.AbsolutePath), "{#error.no_write_access}");
					continue;
				}

				// To file there
				if (toFile.Exists) {
					rs.Add("FAILED", man.EncryptPath(fromFile.AbsolutePath), man.EncryptPath(toFile.AbsolutePath), "{#error.tofile_exists}");
					continue;
				}

				// Zip check
				if (toFile is ZipFileImpl) {
					if (!man.VerifyFile(fromFile, "zip")) {
						rs.Add("FAILED", man.EncryptPath(fromFile.AbsolutePath), man.EncryptPath(toFile.AbsolutePath), man.InvalidFileMsg);
						continue;
					}
				}

				// Unzip check
				if (fromFile is ZipFileImpl) {
					if (!man.VerifyFile(toFile, "unzip")) {
						rs.Add("FAILED", man.EncryptPath(fromFile.AbsolutePath), man.EncryptPath(toFile.AbsolutePath), man.InvalidFileMsg);
						continue;
					}
				}

				string fromPath = fromFile.AbsolutePath;

				if (fromFile.RenameTo(toFile))
					rs.Add("OK", man.EncryptPath(fromPath), man.EncryptPath(toFile.AbsolutePath), "{#message.move_success}");
				else
					rs.Add("FAILED", man.EncryptPath(fromFile.AbsolutePath), man.EncryptPath(toFile.AbsolutePath), "{#error.move_failed}");
			}

			return rs;
		}

		private ResultSet CreateDocs(ManagerEngine man, Hashtable input) {
			ResultSet rs = new ResultSet(new string[]{"status", "fromfile", "tofile", "message"});
			IFile fromFile, toFile;
			ManagerConfig toConfig;
			string ext;
			Hashtable fields;

			for (int i=0; input["frompath" + i] != null && input["toname" + i] != null; i++) {
				fromFile = man.GetFile(man.DecryptPath((string) input["frompath" + i]));
				ext = PathUtils.GetExtension(fromFile.Name);
				toFile = man.GetFile(man.DecryptPath((string) input["topath" + i]), (string) input["toname" + i] + "." + ext, FileType.File);
				toConfig = toFile.Config;

				if (!man.IsToolEnabled("createdoc", toConfig))
					throw new ManagerException("{#error.no_access}");

				if (toConfig.GetBool("general.demo", false)) {
					rs.Add("FAILED", man.EncryptPath(fromFile.AbsolutePath), man.EncryptPath(toFile.AbsolutePath), "{#error.demo}");
					continue;
				}

				if (!fromFile.Exists) {
					rs.Add("FAILED", man.EncryptPath(fromFile.AbsolutePath), man.EncryptPath(toFile.AbsolutePath), "{#error.template_missing}");
					continue;
				}

				if (!man.VerifyFile(toFile, "createdoc")) {
					rs.Add("FAILED", man.EncryptPath(fromFile.AbsolutePath), man.EncryptPath(toFile.AbsolutePath), man.InvalidFileMsg);
					continue;
				}

				if (!toFile.CanWrite) {
					rs.Add("FAILED", man.EncryptPath(fromFile.AbsolutePath), man.EncryptPath(toFile.AbsolutePath), "{#error.no_write_access}");
					continue;
				}

				if (!toConfig.GetBool("filesystem.writable", true)) {
					rs.Add("FAILED", man.EncryptPath(fromFile.AbsolutePath), man.EncryptPath(toFile.AbsolutePath), "{#error.no_write_access}");
					continue;
				}

				if (fromFile.CopyTo(toFile)) {
					// Replace title
					fields = (Hashtable) input["fields"];

					// Replace all fields
					if (fields != null) {
						// Read all data
						StreamReader reader = new StreamReader(toFile.Open(FileMode.Open));
						string fileData = reader.ReadToEnd();
						reader.Close();

						// Replace fields
						foreach (string name in fields.Keys)
							fileData = fileData.Replace("${" + name + "}", (string) fields[name]);

						// Write file data
						StreamWriter writer = new StreamWriter(toFile.Open(FileMode.Create));
						writer.Write(fileData);
						writer.Close();
					}

					rs.Add("OK", man.EncryptPath(fromFile.AbsolutePath), man.EncryptPath(toFile.AbsolutePath), "{#message.createdoc_success}");
				} else
					rs.Add("FAILED", man.EncryptPath(fromFile.AbsolutePath), man.EncryptPath(toFile.AbsolutePath), "{#error.createdoc_failed}");
			}

			return rs;
		}

		private ResultSet CreateZip(ManagerEngine man, Hashtable input) {
			ResultSet rs = new ResultSet(new string[]{"status", "fromfile", "tofile", "message"});

			if (man.GetFile(man.DecryptPath((string) input["topath"]), (string) input["toname"] + ".zip").Exists)
				throw new ManagerException("{#error.tofile_exists}");

			for (int i=0; input["frompath" + i] != null; i++) {
				IFile fromFile = man.GetFile(man.DecryptPath((string) input["frompath" + i]));
				IFile toFile = man.GetFile("zip://" + PathUtils.AddTrailingSlash(man.DecryptPath((string) input["topath"])) + input["toname"] + ".zip", fromFile.Name);

				if (!man.IsToolEnabled("zip", toFile.Config))
					throw new ManagerException("{#error.no_access}");

				if (!fromFile.Exists) {
					rs.Add("FAILED", man.EncryptPath(fromFile.AbsolutePath), man.EncryptPath(toFile.AbsolutePath), "{#error.no_from_file}");
					continue;
				}

				// Zip check
				if (!man.VerifyFile(fromFile, "zip")) {
					rs.Add("FAILED", man.EncryptPath(fromFile.AbsolutePath), man.EncryptPath(toFile.AbsolutePath), man.InvalidFileMsg);
					continue;
				}

				if (fromFile.CopyTo(toFile))
					rs.Add("OK", man.EncryptPath(fromFile.AbsolutePath), man.EncryptPath(toFile.AbsolutePath), "{#message.zip_success}");
			}

			return rs;
		}

		private Hashtable LoadContent(ManagerEngine man, Hashtable input) {
			Hashtable result = new Hashtable();
			IFile file;
			ManagerConfig config;
			StreamReader reader = null;
			Stream stream = null;

			file = man.GetFile(man.DecryptPath((string) input["path"]));
			config = file.Config;

			if (!man.IsToolEnabled("edit", config))
				throw new ManagerException("{#error.no_access}");

			if (config.GetBool("general.demo", false))
				throw new ManagerException("{#error.demo}");

			if (!man.VerifyFile(file, "edit"))
				throw new ManagerException(man.InvalidFileMsg);

			if (!file.CanWrite)
				throw new ManagerException("{#error.no_write_access}");

			if (!config.GetBool("filesystem.writable", true))
				throw new ManagerException("{#error.no_write_access}");

			// Load content
			try {
				stream = file.Open(FileMode.Open);
				if (stream != null) {
					reader = new StreamReader(stream);
					result["content"] = reader.ReadToEnd();
				} else
					throw new ManagerException("{#error.no_access}");
			} finally {
				if (stream != null)
					stream.Close();
			} 

			return result;
		}

		private Hashtable SaveContent(ManagerEngine man, Hashtable input) {
			Hashtable result = new Hashtable();
			IFile file;
			ManagerConfig config;
			StreamWriter writer = null;
			Stream stream = null;

			file = man.GetFile(man.DecryptPath((string) input["path"]));
			config = file.Config;

			if (!man.IsToolEnabled("edit", config))
				throw new ManagerException("{#error.no_access}");

			if (config.GetBool("general.demo", false))
				throw new ManagerException("{#error.demo}");

			if (!man.VerifyFile(file, "edit"))
				throw new ManagerException(man.InvalidFileMsg);

			if (!file.CanWrite)
				throw new ManagerException("{#error.no_write_access}");

			if (!config.GetBool("filesystem.writable", true))
				throw new ManagerException("{#error.no_write_access}");

			// Load content
			try {
				if (file.Exists)
					file.Delete();

				stream = file.Open(FileMode.CreateNew);
				if (stream != null) {
					writer = new StreamWriter(stream);
					writer.Write(input["content"]);
					writer.Close();
				} else
					throw new ManagerException("{#error.no_access}");
			} finally {
				if (stream != null)
					stream.Close();
			} 

			return result;
		}

		#endregion
	}
}
