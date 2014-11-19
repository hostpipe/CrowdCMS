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
	public class HistoryPlugin : Plugin {
		public HistoryPlugin() {
		}

		public override string ShortName {
			get {return "History";}
		}

		public override bool OnInit(ManagerEngine man) {
			man.FileSystems.Add("history", new HistoryFileFactory());

			return true;
		}

		public override bool OnInsertFile(ManagerEngine man, IFile file) {
			HttpResponse response = HttpContext.Current.Response;
			HttpRequest request = HttpContext.Current.Request;
			HttpCookie cookie;
			ArrayList chunks;

			if ((cookie = request.Cookies["hist"]) == null)
				cookie = new HttpCookie("hist");

			cookie.Expires = DateTime.Now.AddDays(30);

			if (cookie.Value != null) {
				chunks = new ArrayList(cookie.Value.Split(new char[]{','}));

				if (chunks.IndexOf(man.EncryptPath(file.AbsolutePath)) == -1)
					chunks.Add(man.EncryptPath(file.AbsolutePath));

				cookie.Value = this.Implode(chunks, ",");
			} else
				cookie.Value = man.EncryptPath(file.AbsolutePath);

			response.Cookies.Remove("hist");
			response.Cookies.Add(cookie);

			return true;
		}

		private string Implode(ICollection col, string delim) {
			string outStr = "";

			foreach (object obj in col)
				outStr += obj + delim;

			if (outStr.Length > 0)
				outStr = outStr.Substring(0, outStr.Length - 1);

			return outStr;
		}
	}

	class HistoryFile : BaseFile {
		public HistoryFile(ManagerEngine man, string path, string child, FileType type) : base(man, path, child, type) {
		}

		public override bool IsDirectory {
			get { return true; }
		}

		public override bool IsFile {
			get { return false; }
		}

		public override IFile[] ListFilesFiltered(IFileFilter filter) {
			HttpCookie cookie = HttpContext.Current.Request.Cookies["hist"];
			ArrayList files = new ArrayList();

			if (cookie != null && cookie.Value != null) {
				foreach (string path in cookie.Value.Split(new char[]{','})) {
					try {
						IFile file = this.manager.GetFile(this.manager.DecryptPath(path));

						if (filter.Accept(file))
							files.Add(file);
					} catch {
						// Ignore
					}
				}
			}

			return (IFile[]) files.ToArray(typeof(IFile));
		}
	}

	class HistoryFileFactory : IFileFactory {
		public IFile GetFile(ManagerEngine man, string path, string child, FileType type) {
			return new HistoryFile(man, path, child, type);
		}
	}
}
