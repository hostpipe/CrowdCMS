/*
 * $Id: ASPNETAuthenticator.cs 9 2007-05-27 10:47:07Z spocke $
 *
 * Copyright © 2007, Moxiecode Systems AB, All rights reserved. 
 */

using System.Collections.Specialized;
using System.Web.SessionState;
using Moxiecode.Manager.Utils;
using Moxiecode.Manager;
using System.Web;
using System.Web.UI;

namespace Moxiecode.Manager.Authenticators {
	/// <summary>
	///   This class authenicates users agains the built in ASP.Net Page authenication. Use this authenticator
	///   when FormsAuthentication, Windows or Passport authentication providers are used in Web.config.
	/// </summary>
 	public class ASPNETAuthenticator : Plugin {
		/// <summary>
		///  Main constructor.
		/// </summary>
		public ASPNETAuthenticator() {
		}

		/// <summary>
		/// 
		/// </summary>
		public override string ShortName {
			get { return "ASPNETAuthenticator"; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="man"></param>
		/// <returns></returns>
		public override bool OnAuthenticate(ManagerEngine man) {
			HttpContext context = HttpContext.Current;
			ManagerConfig config = man.Config;
		 	string user = context.User.Identity.Name;

		 	// Cleanup
		 	user = user.Replace("\\", "");
		 	user = user.Replace("/", "");
		 	user = user.Replace(":", "");

		 	// Replace all ${user} with current user
		 	for (int i=0; i<config.Keys.Count; i++)
		 		config[config.Keys[i]] = config[config.Keys[i]].Replace("${user}", user);

			return context.User.Identity.IsAuthenticated;
		}
	}
}
