/*
 * $Id: PathUtils.cs 571 2008-11-07 13:53:58Z spocke $
 *
 * Copyright © 2007, Moxiecode Systems AB, All rights reserved. 
 */

using System.IO;
using System.Text.RegularExpressions;

namespace Moxiecode.Manager.Utils {
	/// <summary>
	///  This class conains common methods for handling path strings.
	/// </summary>
	public class PathUtils {
		/// <summary>
		///  Converts a system path to a Unix style path with "/".
		/// </summary>
		/// <param name="path">Path to convert.</param>
		public static string ToUnixPath(string path) {
			return path.Replace('\\', '/');
		}

		/// <summary>
		///  Converts a Unix style path to a local file system path.
		/// </summary>
		/// <param name="path">Unix style path to convert.</param>
		/// <returns>Local file system path.</returns>
		public static string ToOSPath(string path) {
			return path.Replace('/', Path.DirectorySeparatorChar);
		}
 
		/// <summary>
		///  Removes any trailing slash from a path. For example /my/path/ will be converted to /my/path.
		/// </summary>
		/// <param name="path">Path to remove trailing slashes from.</param>
		/// <returns>Path without trailing slash.</returns>
 		public static string RemoveTrailingSlash(string path) {
			if (path.Length > 0 && (path[path.Length-1] == '/' || path[path.Length-1] == '\\'))
				path = path.Substring(0, path.Length-1);

			return path;
		}

		/// <summary>
		///  Adds a trailing slash to the specified path. For example /my/path will be converted to /my/path/.
		/// </summary>
		/// <param name="path">Path to add trailing slash to.</param>
		/// <returns>Path with added trailing slash.</returns>
		public static string AddTrailingSlash(string path) {
			if (path.Length > 0 && (path[path.Length-1] != '/' && path[path.Length-1] != '\\'))
				return path + "/";

			return path;
		}

		/// <summary>
		///  Verifies that the specified path is a child/subpath of the parent path.
		/// </summary>
		/// <param name="parent_path">Parent directory path.</param>
		/// <param name="path">Child path that must be a subpath or the same as parent path.</param>
		/// <returns>True if the specified path is a child/subpath of the parent path</returns>
		public static bool IsChildPath(string parent_path, string path) {
			return AddTrailingSlash(PathUtils.ToUnixPath(path)).ToLower().IndexOf(AddTrailingSlash(PathUtils.ToUnixPath(parent_path)).ToLower()) == 0;
		}

		/// <summary>
		///  Returns true/false if a path is absolute or not.
		/// </summary>
		/// <param name="path">Path to check if it's absolute or no.</param>
		/// <returns>true/false if the specified path is absolute.</returns>
		public static bool IsAbsolutePath(string path) {
			return Regex.IsMatch(path, @"(^/.*)|(^\\.*)|(^[a-zA-Z]:.*)");
		}

		/// <summary>
		///  Returns the extension of a file.
		/// </summary>
		/// <param name="path">Path to get extension from.</param>
		/// <returns>Extension like xml, pdf etc.</returns>
		public static string GetExtension(string path) {
			int pos;

			if ((pos = path.LastIndexOf('.')) != -1)
				return path.Substring(pos + 1);

		 	return "";
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string FileName(string path) {
			Match match = Regex.Match(path, @"([^/]+)$");

			if (match.Success)
				return match.Groups[1].Value;

			return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string BaseName(string path) {
			Match match = Regex.Match(path, @"([^/]+)(\.[^.]+)$"); // file.ext

			if (match.Success)
				return match.Groups[1].Value;

			match = Regex.Match(path, @"([^/]+)$"); // file

			if (match.Success)
				return match.Groups[1].Value;

			match = Regex.Match(path, @"([^/]+)/$"); // dir/

			if (match.Success)
				return match.Groups[1].Value;

			return null;
		}
	}
}
