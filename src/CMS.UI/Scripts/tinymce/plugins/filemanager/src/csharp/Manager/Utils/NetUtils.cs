/*
 * $Id: NetUtils.cs 9 2007-05-27 10:47:07Z spocke $
 *
 * Copyright © 2007, Moxiecode Systems AB, All rights reserved. 
 */

using System;

namespace Moxiecode.Manager.Utils {
	/// <summary>
	///  Network utility class.
	/// </summary>
	public class NetUtils {
		/// <summary>
		///   Returns a IPv4 address as a int value, useful for range checking. 
		/// </summary>
		/// <param name="ip_address">IP address string to convert.</param>
		/// <returns>IPv4 address</returns>
		public static int GetIPAddress(string ip_address) {
			byte[] parts = System.Net.IPAddress.Parse(ip_address).GetAddressBytes();
			int address = 0;

			// Mask addess
			address = address | (parts[0] << 24);
			address = address | (parts[1] << 16);
			address = address | (parts[2] << 8);
			address = address | parts[3];

			return address;
		}

		/// <summary>
		///   Returns a IPv4 address string from a int ip address. 
		/// </summary>
		/// <param name="ip_address">IP address to convert.</param>
		/// <returns>IPv4 address string like 213.199.80.26</returns>
		public static string GetIPAddressString(int ip_address) {
			string ipAddress = "";

			ipAddress += (byte) (ip_address >> 24) + ".";
			ipAddress += (byte) (ip_address >> 16) + ".";
			ipAddress += (byte) (ip_address >> 8) + ".";
			ipAddress += (byte) ip_address;

			return ipAddress;
		}
	}
}
