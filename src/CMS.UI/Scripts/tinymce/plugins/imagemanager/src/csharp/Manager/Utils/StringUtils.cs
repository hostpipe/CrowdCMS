/*
 * $Id: StringUtils.cs 465 2008-10-15 11:53:17Z spocke $
 *
 * Copyright © 2007, Moxiecode Systems AB, All rights reserved. 
 */

using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Moxiecode.Manager.Utils {
	/// <summary>
	///  Utility class to handle common string issues.
	/// </summary>
	public class StringUtils {
		/// <summary>
		///  Checks if the string is bool true/false value.
		/// </summary>
		/// <param name="str">String to check.</param>
		/// <returns>true/false depending on string contents.</returns>
		public static bool CheckBool(string str) {
			if (str == null)
				return false;

 			str = str.Trim().ToLower();

			return str == "yes" || str == "true" || str == "1";
		}
 
		/// <summary>
		///  Returns a size string out of a size long for example 10MB.
		/// </summary>
		/// <param name="size">Size long to convert.</param>
		/// <returns>Size string representation of the specified long.</returns>
 		public static string GetSizeStr(long size) {
			// MB
			if (size > 1048576)
				return Math.Round(size / 1048576.0, 1) + " MB";

			// KB
			if (size > 1024)
				return Math.Round(size / 1024.0, 1) + " KB";

			return size + " bytes";
		}

		/// <summary>
		///  Returns a size long
		/// </summary>
		/// <param name="size">Size string to convert.</param>
		/// <param name="default_size">Default size string.</param>
		/// <returns>Long representation of the size string.</returns>
		public static long GetSizeLong(string size, string default_size) {
			long sizeInt = 0;
			int pos;

			if (size == null)
				size = default_size;

			size = size.ToLower();

			if ((pos = size.IndexOf("m")) != -1)
				sizeInt = Convert.ToInt64(size.Substring(0, pos)) * (1024 * 1024);
			else if ((pos = size.IndexOf("k")) != -1)
				sizeInt = Convert.ToInt64(size.Substring(0, pos)) * 1024;
			else
				sizeInt = Convert.ToInt64(size);

			return sizeInt;
		}

		/// <summary>
		///  Creates a regex PHP style like /regex/i makes it case insensitive.
		/// </summary>
		/// <param name="regex">Regex to convert.</param>
		/// <returns>Returns a regex instance from a PHP style regex string.</returns>
		public static Regex CreateRegex(string regex) {
			if (regex.EndsWith("/"))
				return new Regex(regex.Substring(1, regex.Length-2), RegexOptions.CultureInvariant);

			if (regex.EndsWith("/i"))
				return new Regex(regex.Substring(1, regex.Length-3), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

			return new Regex(regex, RegexOptions.CultureInvariant);
		}

		/// <summary>
		///  Returns a formatted date string by the specified format string.
		/// </summary>
		/// <param name="date">Date to convert to a string.</param>
		/// <param name="format">Format to convert date by.</param>
		/// <returns></returns>
		public static string GetDate(DateTime date, string format) {
			return date.ToString(format, new System.Globalization.CultureInfo("en-US"));
		}

		/// <summary>
		///  Escapes a string so it's JavaScript compatible. Converts ' and " to \' and \".
		/// </summary>
		/// <param name="str">String to escape.</param>
		/// <returns>Escaped string.</returns>
		public static string Escape(string str) {
			if (str == null)
				return null;

			StringBuilder strBuilder = new StringBuilder(str.Length);
 
			char[] chars = str.ToCharArray();
			for (int i=0; i<chars.Length; i++) {
				switch (chars[i]) {
					case '\n':
						strBuilder.Append("\\n");
						break;

					case '\r':
						strBuilder.Append("\\r");
						break;

					case '\'':
						strBuilder.Append("\\\'");
						break;

					case '\"':
						strBuilder.Append("\\\"");
						break;

					case '\\':
						strBuilder.Append("\\\\");
						break;

					default:
						strBuilder.Append(chars[i]);
						break;
				}
			}

			return strBuilder.ToString();
		}

		/// <summary>
		///  Senitizes the specfified string.
		/// </summary>
		/// <param name="str">String to sanitize.</param>
		/// <returns>Sanitized string.</returns>
		public static string Sanitize(string str) {
			if (str == null)
				return null;

			return Regex.Replace(str, @"[^0-9a-z\-_,]+", "");
		}
	}
}
