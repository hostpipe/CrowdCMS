/*
 * Created by SharpDevelop.
 * User: joakim
 * Date: 2010-03-01
 * Time: 12:45
 * 
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
	/// Description of Uploaded plugin.
	/// </summary>
	public class UploadedPlugin : Plugin {
		public UploadedPlugin() {
		}

		public override string ShortName {
			get {return "Uploaded";}
		}

		public override bool OnInit(ManagerEngine man) {
			man.FileSystems.Add("uploaded", new UploadedFileFactory());

			return true;
		}

		/// <summary>
		///  Gets called after a file action was perforem for example after a rename or copy.
		/// </summary>
		/// <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		/// <param name="action">File action type.</param>
		/// <param name="file1">File object 1 for example from in a copy operation.</param>
		/// <param name="file2">File object 2 for example to in a copy operation. Might be null in for example a delete.</param>
		/// <returns>true/false if the execution of the event chain should continue execution.</returns>
		public override bool OnFileAction(ManagerEngine man, FileAction action, IFile file1, IFile file2) {
			if (action != FileAction.Add)
				return true;
				
			HttpResponse response = HttpContext.Current.Response;
			HttpRequest request = HttpContext.Current.Request;
			HttpCookie cookie;
			ArrayList chunks;

			if ((cookie = request.Cookies["upl"]) == null)
				cookie = new HttpCookie("upl");

			cookie.Expires = DateTime.Now.AddDays(30);

			if (cookie.Value != null) {
				chunks = new ArrayList(cookie.Value.Split(new char[]{','}));

				if (chunks.IndexOf(man.EncryptPath(file1.AbsolutePath)) == -1)
					chunks.Add(man.EncryptPath(file1.AbsolutePath));

				cookie.Value = this.Implode(chunks, ",");
			} else
				cookie.Value = man.EncryptPath(file1.AbsolutePath);

			response.Cookies.Remove("upl");
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

	class UploadedFile : BaseFile {
		public UploadedFile(ManagerEngine man, string path, string child, FileType type) : base(man, path, child, type) {
		}

		public override bool IsDirectory {
			get { return true; }
		}

		public override bool IsFile {
			get { return false; }
		}

		public override IFile[] ListFilesFiltered(IFileFilter filter) {
			HttpCookie cookie = HttpContext.Current.Request.Cookies["upl"];
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

	class UploadedFileFactory : IFileFactory {
		public IFile GetFile(ManagerEngine man, string path, string child, FileType type) {
			return new UploadedFile(man, path, child, type);
		}
	}
}
