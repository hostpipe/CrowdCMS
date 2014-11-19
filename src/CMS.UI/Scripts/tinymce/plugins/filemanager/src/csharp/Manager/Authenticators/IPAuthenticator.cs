/*
 * $Id: IPAuthenticator.cs 9 2007-05-27 10:47:07Z spocke $
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
 	public class IPAuthenticator : Plugin {
		/// <summary>
		///  Main constructor.
		/// </summary>
		public IPAuthenticator() {
		}

		/// <summary>
		/// 
		/// </summary>
		public override string ShortName {
			get { return "IPAuthenticator"; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="man"></param>
		/// <returns></returns>
		public override bool OnAuthenticate(ManagerEngine man) {
			HttpContext context = HttpContext.Current;
			string[] ipAddresses = man.Config.Get("IPAuthenticator.ip_numbers").Split(new char[]{','});

			foreach (string ipadds in ipAddresses) {
				string[] ipRange = ipadds.Split(new char[]{'-'});

				// Single IP
				if (ipRange.Length == 1 && NetUtils.GetIPAddress(ipRange[0]) == NetUtils.GetIPAddress(context.Request.ServerVariables["REMOTE_ADDR"]))
				    return true;

				// IP range
				if (ipRange.Length == 2 && NetUtils.GetIPAddress(ipRange[0]) >= NetUtils.GetIPAddress(context.Request.ServerVariables["REMOTE_ADDR"]) && NetUtils.GetIPAddress(ipRange[1]) <= NetUtils.GetIPAddress(context.Request.ServerVariables["REMOTE_ADDR"]))
				    return true;
			}

			return false;
		}
	}
}
