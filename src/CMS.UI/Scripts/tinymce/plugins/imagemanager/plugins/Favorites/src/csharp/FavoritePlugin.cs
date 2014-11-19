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
	public class FavoritesPlugin : Plugin {
		public FavoritesPlugin() {
		}

		public override string ShortName {
			get {return "Favorites";}
		}

		public override bool OnInit(ManagerEngine man) {
			man.FileSystems.Add("favorite", new FavoriteFileFactory());

			return true;
		}

		public override object OnRPC(ManagerEngine man, string cmd, Hashtable input) {
			switch (cmd) {
				case "addFavorites":
					return this.AddFavorites(man, input);

				case "removeFavorites":
					return this.RemoveFavorites(man, input);
			}

			return null;
		}

		private object AddFavorites(ManagerEngine man, Hashtable input) {
			ResultSet rs = new ResultSet(new string[]{"status", "file", "message"});
			HttpResponse response = HttpContext.Current.Response;
			HttpRequest request = HttpContext.Current.Request;
			HttpCookie cookie;
			ArrayList chunks;

			if ((cookie = request.Cookies["fav"]) == null) {
				cookie = new HttpCookie("fav");
				chunks = new ArrayList();
			} else
				chunks = new ArrayList(cookie.Value.Split(new char[]{','}));

			cookie.Expires = DateTime.Now.AddDays(30);

			for (int i=0; input["path" + i] != null; i++) {
				string path = (string) input["path" + i];

				if (chunks.IndexOf(path) == -1)
					chunks.Add(path);

				rs.Add("OK", man.EncryptPath(path), "Path was added.");
			}

			cookie.Value = this.Implode(chunks, ",");
			response.Cookies.Remove("fav");
			response.Cookies.Add(cookie);

			return rs;
		}

		private object RemoveFavorites(ManagerEngine man, Hashtable input) {
			ResultSet rs = new ResultSet(new string[]{"status", "file", "message"});
			HttpResponse response = HttpContext.Current.Response;
			HttpRequest request = HttpContext.Current.Request;
			HttpCookie cookie;
			ArrayList chunks;

			if ((cookie = request.Cookies["fav"]) == null)
				return rs;

			chunks = new ArrayList(cookie.Value.Split(new char[]{','}));
			cookie.Expires = DateTime.Now.AddDays(30);

			for (int i=0; input["path" + i] != null; i++) {
				string path = (string) input["path" + i];

				chunks.RemoveAt(chunks.IndexOf(path));

				rs.Add("OK", man.EncryptPath(path), "Path was removed.");
			}

			cookie.Value = this.Implode(chunks, ",");
			response.Cookies.Remove("fav");
			response.Cookies.Add(cookie);

			return rs;
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

	class FavoriteFile : BaseFile {
		public FavoriteFile(ManagerEngine man, string path, string child, FileType type) : base(man, path, child, type) {
		}

		public override bool IsDirectory {
			get { return true; }
		}

		public override bool IsFile {
			get { return false; }
		}

		public override IFile[] ListFilesFiltered(IFileFilter filter) {
			HttpCookie cookie = HttpContext.Current.Request.Cookies["fav"];
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

	class FavoriteFileFactory : IFileFactory {
		public IFile GetFile(ManagerEngine man, string path, string child, FileType type) {
			return new FavoriteFile(man, path, child, type);
		}
	}
}
