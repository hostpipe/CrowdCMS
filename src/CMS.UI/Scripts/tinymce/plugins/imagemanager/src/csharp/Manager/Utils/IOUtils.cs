/*
 * $Id: IOUtils.cs 125 2007-10-15 11:38:30Z spocke $
 *
 * Copyright © 2007, Moxiecode Systems AB, All rights reserved. 
 */

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Moxiecode.Manager.Utils {
	/// <summary>
	///  Win32 API function calls.
	/// </summary>
	public class IOUtils {
#if(UNSAFE)
		/// <summary>
		///  Kernel32 function that creates directory. Due to a bug/issue with the .NET runtime
		///  a call to this method is required.
		/// </summary>
		/// <param name="lpPathName">Path to create.</param>
		/// <param name="lpSecurityAttributes">Security attributes.</param>
		/// <returns>true - success, false - failure</returns>
		[DllImport("kernel32.dll")]
		public static extern bool CreateDirectory(string lpPathName, IntPtr lpSecurityAttributes);
#else
		/// <summary>
		///  Kernel32 function that creates directory. Due to a bug/issue with the .NET runtime
		///  a call to this method is required.
		/// </summary>
		/// <param name="lpPathName">Path to create.</param>
		/// <param name="lpSecurityAttributes">Security attributes.</param>
		/// <returns>true - success, false - failure</returns>
		public static bool CreateDirectory(string lpPathName, IntPtr lpSecurityAttributes) {
			new System.IO.DirectoryInfo(lpPathName).Create();
			return true;
		}
#endif

		/// <summary>
		/// 
		/// </summary>
		/// <param name="in_stream"></param>
		/// <param name="out_stream"></param>
		/// <param name="buff_size"></param>
		public static void StreamFromTo(Stream in_stream, Stream out_stream, int buff_size) {
			byte[] buff = new byte[buff_size];
			int len;

			try {
				while ((len = in_stream.Read(buff, 0, buff_size)) > 0) {
					out_stream.Write(buff, 0, len);
					out_stream.Flush();
				}
			} finally {
				if (in_stream != null)
					in_stream.Close();

				if (out_stream != null)
					out_stream.Close();
			}
		}
	}
}
