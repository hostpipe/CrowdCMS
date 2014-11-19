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
using System.Collections.Specialized;
using System.Web;
using System.Web.SessionState;
using System.Text;
using System.Security.Cryptography;
using Moxiecode.Manager;
using Moxiecode.Manager.FileSystems;
using Moxiecode.Manager.Utils;

namespace Moxiecode.Manager.Plugins {
	/// <summary>
	///  This is a template plugin to be used to create new plugins. Rename all Template references below to your plugins name
	///  and implement the methods you need.
	/// </summary>
	public class ExternalAuthenticator : Plugin {
		public ExternalAuthenticator() {
		}

		/// <summary>
		///  Short name for the plugin, used in the authenticator config option for example
		///  so that you don't need to write the long name for it namespace.classname.
		/// </summary>
		public override string ShortName {
			get {
				return "ExternalAuthenticator";
			}
		}

		/// <summary>
		///  Gets called on a authenication request. This method should check sessions or simmilar to verify that the user has access to the backend.
		///  This method should return true if the current request is authenicated or false if it's not.
		/// </summary>
		/// <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		/// <returns>true/false if the user is authenticated</returns>
		public override bool OnAuthenticate(ManagerEngine man) {
			HttpContext context = HttpContext.Current;
			HttpSessionState session = context.Session;
			HttpRequest request = context.Request;
			HttpResponse response = context.Response;
			ManagerConfig config = man.Config;
			string dir, authURL, secretKey, data, returnURL, prefix;

			// Get some config values
			authURL = config["ExternalAuthenticator.external_auth_url"];
			secretKey = config["ExternalAuthenticator.secret_key"];
			prefix = config["ExternalAuthenticator.session_prefix"];
			dir = Path.GetFileName(Path.GetDirectoryName(request.FilePath));

			if (prefix == null)
				prefix = "mcmanager_";

			// Always load the language packs
			if (dir == "language") {
				// Override language
				if (session[prefix + "_ExternalAuthenticator.general.language"] != null)
					config["general.language"] = (string) session[prefix + "_ExternalAuthenticator.general.language"];

				return true;
			}

			// Check already authenticated on rpc and stream
			if (dir == "rpc" || dir  == "stream") {
				if (session[prefix + "_ExternalAuthenticator"] != null && ((string) session[prefix + "_ExternalAuthenticator"]) == "true") {
					// Override config values
					foreach (string key in session.Keys) {
						if (key.StartsWith(prefix + "_ExternalAuthenticator."))
							config[key.Substring((prefix + "_ExternalAuthenticator.").Length)] = (string) session[key];
					}

					// Force update of internal items
					man.Config = config;

					return true;
				}
			}

			// Handle post
			if (request.Form["key"] != null) {
				data = "";
				foreach (string key in request.Form.Keys) {
					if (key != "key")
						data += request.Form[key];
				}

				// Check if keys match
				if (MD5(data + secretKey) == request.Form["key"]) {
					session[prefix + "_ExternalAuthenticator"] = "true";

					// Store input data in session scope
					foreach (string key in request.Form.Keys) {
						string ckey = key.Replace("__", "."); // Handle PHP escaped strings
						session[prefix + "_ExternalAuthenticator." + ckey] = request.Form[key];
					}

					return true;
				} else {
					response.Write("Input data doesn't match verify that the secret keys are the same.");
					response.End();
					return false;
				}
			}

			// Build return URL
			returnURL = request.Url.ToString();
			if (returnURL.IndexOf('?') != -1)
				returnURL = returnURL.Substring(0, returnURL.IndexOf('?'));
			returnURL += "?type=" + man.Prefix;

			// Force auth absolute
			authURL = new Uri(request.Url, authURL).ToString();

			// Handle rpc and stream requests
			if (dir == "rpc" || dir  == "stream") {
				returnURL = new Uri(request.Url, PathUtils.ToUnixPath(Path.GetDirectoryName(Path.GetDirectoryName(request.FilePath)))).ToString() + "/default.aspx?type=" + man.Prefix;
				config["authenticator.login_page"] = authURL + "?return_url=" + context.Server.UrlEncode(returnURL);
				return false;
			}

			response.Redirect(authURL + "?return_url=" + context.Server.UrlEncode(returnURL), true);
			return false;
		}

		private string MD5(string str) {
			MD5 md5 = new MD5CryptoServiceProvider();
			byte[] result = md5.ComputeHash(Encoding.ASCII.GetBytes(str));
			str = BitConverter.ToString(result).ToLower();

			return str.Replace("-", "");
		}
	}
}
