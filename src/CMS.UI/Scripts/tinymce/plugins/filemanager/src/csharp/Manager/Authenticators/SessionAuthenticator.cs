/*
 * $Id: SessionAuthenticator.cs 641 2009-01-19 13:48:24Z spocke $
 *
 * Copyright © 2007, Moxiecode Systems AB, All rights reserved. 
 */

using System.Collections.Specialized;
using System.Web.SessionState;
using Moxiecode.Manager.Utils;
using System.Web;
using System.Web.UI;

namespace Moxiecode.Manager.Authenticators {
	/// <summary>
	///   This class is a session authenticator implementation, this implementation will check for
	///   session keys defined by the config options "authenticator.session.logged_in_key, 
	///   authenticator.session.groups_key".
	/// </summary>
 	public class SessionAuthenticator : Plugin {
		/// <summary>
		///  Main constructor.
		/// </summary>
		public SessionAuthenticator() {
		}

		/// <summary>
		/// 
		/// </summary>
		public override string ShortName {
			get { return "SessionAuthenticator"; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="man"></param>
		/// <returns></returns>
		public override bool OnAuthenticate(ManagerEngine man) {
			HttpContext context = HttpContext.Current;
			ManagerConfig config = man.Config;
			string loggedInKey, groupsKey, userKey, pathKey, rootPathKey, configPrefix;

			// Support both old and new format
			loggedInKey = (string) config.Get("SessionAuthenticator.logged_in_key", config["authenticator.session.logged_in_key"]);
		 	groupsKey = (string) config.Get("SessionAuthenticator.groups_key", config["authenticator.session.groups_key"]);
		 	userKey = (string) config.Get("SessionAuthenticator.user_key", config["authenticator.session.user_key"]);
		 	pathKey = (string) config.Get("SessionAuthenticator.path_key", config["authenticator.session.path_key"]);
		 	rootPathKey = (string) config.Get("SessionAuthenticator.rootpath_key", config["authenticator.session.rootpath_key"]);
		 	configPrefix = (string) config.Get("SessionAuthenticator.config_prefix", "mcmanager") + ".";

		 	// Grab current user/login
		 	string user = (string) (context.Session[userKey] != null ? context.Session[userKey] : "");

		 	// Cleanup
		 	user = user.Replace("\\", "");
		 	user = user.Replace("/", "");
		 	user = user.Replace(":", "");

		 	// Replace all ${user} with current user
		 	for (int i=0; i<config.Keys.Count; i++) {
		 		config[config.Keys[i]] = config[config.Keys[i]].Replace("${user}", user);
		 		config[config.Keys[i]] = config[config.Keys[i]].Replace("{$user}", user);
		 	}

		 	// Loop through all sessions
		 	foreach (string key in context.Session.Keys) {
		 		if (key.StartsWith(configPrefix))
		 			config[key.Substring(configPrefix.Length)] = "" + context.Session[key];
		 	}

		 	// path specified in session
		 	if (context.Session[pathKey] != null)
		 		config["filesystem.path"] = (string) context.Session[pathKey];

		 	// Root path specified in session
		 	if (context.Session[rootPathKey] != null)
		 		config["filesystem.rootpath"] = (string) context.Session[rootPathKey];			

		 	// Force update of internal items
		 	man.Config = man.Config;

			return context.Session[loggedInKey] != null && StringUtils.CheckBool((string) context.Session[loggedInKey]);
		}
	}
}
