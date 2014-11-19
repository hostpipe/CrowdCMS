/*
 * Created by SharpDevelop.
 * User: spocke
 * Date: 03/10/2008
 * Time: 13:06
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Xml;
using System.IO;
using System.Web;
using System.Collections;
using System.Collections.Specialized;

namespace Moxiecode.Manager.Utils {
	/// <summary>
	/// Description of ClientResources.
	/// </summary>
	public class ClientResources {
		#region privatefields
		private bool debugEnabled;
		private Hashtable packages;
		private string basePath;
		#endregion

		/// <summary></summary>
		public ClientResources() {
			this.packages = new Hashtable();
		}

		#region properties

		/// <summary>
		///  Returns debug state.
		/// </summary>
		public bool IsDebugEnabled {
			get { return this.debugEnabled; }
		}

		/// <summary>
		///  ..
		/// </summary>
		public string BasePath {
			get { return basePath; }
			set { basePath = value; }
		}
		
		#endregion

		/// <summary>
		/// 
		/// </summary>
		/// <param name="package_id"></param>
		/// <param name="file_id"></param>
		/// <returns></returns>
		public ClientResourceFile GetFile(string package_id, string file_id) {
			ArrayList files = (ArrayList) this.packages[package_id];

			if (files != null) {
				foreach (ClientResourceFile file in files) {
					if (file.Id == file_id)
						return file;
				}
			}

			return null;
		}

		/// <summary>
		///  Returns a file list.
		/// </summary>
		/// <param name="package_id"></param>
		/// <returns></returns>
		public ClientResourceFile[] GetFiles(string package_id) {
			return (ClientResourceFile[]) ((ArrayList) this.packages[package_id]).ToArray(typeof(ClientResourceFile));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="xml_file"></param>
		public void TryLoad(string xml_file) {
			if (File.Exists(HttpContext.Current.Request.MapPath(xml_file)))
				this.Load(xml_file);
		}

		/// <summary>
		///  Loads the resource XML file.
		/// </summary>
		/// <param name="xml_file"></param>
		public void Load(string xml_file) {
			XmlDocument doc = new XmlDocument();

			doc.Load(HttpContext.Current.Request.MapPath(xml_file));

			this.basePath = PathUtils.ToUnixPath(Path.GetDirectoryName(xml_file));
			this.debugEnabled = doc.DocumentElement.GetAttribute("debug") == "yes";

			foreach (XmlElement elm in doc.GetElementsByTagName("package")) {
				ArrayList files;

				if (!this.packages.ContainsKey(elm.GetAttribute("id")))
					this.packages[elm.GetAttribute("id")] = new ArrayList();

				files = (ArrayList) this.packages[elm.GetAttribute("id")];

				foreach (XmlElement felm in elm.GetElementsByTagName("file")) {
					files.Add(new ClientResourceFile(
						felm.GetAttribute("id"),
						this.basePath + "/" + felm.GetAttribute("path"),
						!felm.HasAttribute("keepWhiteSpace") || felm.GetAttribute("keepWhiteSpace") != "yes",
						felm.GetAttribute("type")
					));
				}
			}
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class ClientResourceFile {
		private string id, contentType, path;
		private bool removeWhitespace;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		/// <param name="path"></param>
		/// <param name="remove_whitespace"></param>
		/// <param name="content_type"></param>
		public ClientResourceFile(string id, string path, bool remove_whitespace, string content_type) {
			this.id = id;
			this.path = path;
			this.removeWhitespace = remove_whitespace;
			this.contentType = content_type;
		}

		/// <summary>
		/// 
		/// </summary>
		public string Id {
			get { return this.id; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string ContentType {
			get { return this.contentType; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string Path {
			get { return this.path; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool RemoveWhiteSpace {
			get { return this.removeWhitespace; }
		}
	}
}
