/*
 * $Id: BasicFileFilter.cs 293 2008-05-07 14:52:56Z spocke $
 *
 * Copyright © 2007, Moxiecode Systems AB, All rights reserved. 
 */

using System.Text.RegularExpressions;
using Moxiecode.Manager.Utils;

namespace Moxiecode.Manager.FileSystems {
	/// <summary>
	///  
	/// </summary>
	public enum FilterReason {
		/// <summary> </summary>
		InvalidExtension = 1,

		/// <summary> </summary>
		InvalidFileName = 2
	}

	 /// <summary>
	 ///  Basic file filter, this class handles some common filter problems
	 ///  and is possible to extend if needed.
	 /// </summary>
	 public class BasicFileFilter : IFileFilter {
	 	// Private fields
	 	private string[] excludeFolders;
		private string[] includeFolders;
		private string[] excludeFiles;
		private string[] includeFiles;
		private string[] extensions;
		private string includeFilePattern;
		private string excludeFilePattern;
		private string includeDirectoryPattern;
		private string excludeDirectoryPattern;
		private string includeWildcardPattern;
		private bool filesOnly;
		private bool dirsOnly;
		private FilterReason reason;

		/// <summary>Main constructor.</summary>
		public BasicFileFilter() {
			this.extensions = null;
			this.reason = 0;
			this.filesOnly = false;
			this.dirsOnly = false;
		}

		/// <summary>Only files are to be accepted in result.</summary>
		public bool OnlyFiles {
		 	set {
		 		this.filesOnly = value;
		 	}
		}

		/// <summary>Only dirs are to be accepted in result.</summary>
		public bool OnlyDirs {
		 	set {
		 		this.dirsOnly = value;
		 	}
		}

		/// <summary>Comma separated list of valid file extensions.</summary>
		public string IncludeExtensions {
		 	set {
			 	if (value == "*" || value == "") {
			 		this.extensions = null;
					return;
			 	}

				this.extensions = value.ToLower().Split(',');
		 	}
		}

		/// <summary>Comma separated string list of filenames to exclude.</summary>
		public string ExcludeFiles {
		 	set {
				if (value != "")
					this.excludeFiles = value.Split(',');
				else
					this.excludeFiles = null;
		 	}
		}

		/// <summary>Comma separated string list of filenames to include.</summary>
		public string IncludeFiles {
		 	set {
				if (value != "")
					this.includeFiles = value.Split(',');
				else
					this.includeFiles = null;
		 	}
		}

		/// <summary>Comma separated string list of foldernames to exclude.</summary>
		public string ExcludeFolders {
		 	set {
				if (value != "")
					this.excludeFolders = value.Split(',');
				else
					this.excludeFolders = null;
		 	}
		}

		/// <summary>Comma separated string list of foldernames to include.</summary>
		public string IncludeFolders {
		 	set {
		 		if (value != "")
					this.includeFolders = value.Split(',');
				else
					this.includeFolders = null;
		 	}
		}

		/// <summary>Regexp pattern that is used to accept files path parts.</summary>
		public string IncludeFilePattern {
		 	set {
		 		this.includeFilePattern = value != "" ? value : null;
		 	}
		}

		/// <summary>Regexp pattern that is used to deny files path parts.</summary>
		public string ExcludeFilePattern {
		 	set {
		 		this.excludeFilePattern = value != "" ? value : null;
		 	}
		}

		/// <summary>Regexp pattern that is used to accept directory path parts.</summary>
		public string IncludeDirectoryPattern {
		 	set {
		 		this.includeDirectoryPattern = value != "" ? value : null;
		 	}
		}

		/// <summary>Maximum number of directory levels to accept.</summary>
		public string ExcludeDirectoryPattern {
		 	set {
		 		this.excludeDirectoryPattern = value != "" ? value : null;
		 	}
		}

		/// <summary></summary>
		public string IncludeWildcardPattern {
			get { return includeWildcardPattern; }
			set { includeWildcardPattern = value; }
		}

		/// <summary>Status code why the accept failed. These status codes are defined as constants.</summary>
		public FilterReason Reason {
		 	get {
		 		return this.reason;
		 	}
		}

		/// <summary>Returns true or false if the file is accepted or not.</summary>
		/// <param name="file">File to verify.</param>
		/// <returns>true or false if the file is accepted or not.</returns>
		public bool Accept(IFile file) {
		 	bool valid = false;

			this.reason = 0;

			// Handle files only
			if (file.IsDirectory && this.filesOnly)
				return false;

			// Handle dirs only
			if (file.IsFile && this.dirsOnly)
				return false;

			// Handle include files
			if (file.IsFile && this.includeFiles != null) {
				valid = false;
				foreach (string fileName in this.includeFiles) {
					if (fileName == file.Name) {
						this.reason = FilterReason.InvalidFileName;
						valid = true;
						break;
					}
				}

				if (!valid) {
					this.reason = FilterReason.InvalidFileName;
					return false;
				}
			}

			// Handle exclude files
			if (file.IsFile && this.excludeFiles != null) {
				foreach (string fileName in this.excludeFiles) {
					if (fileName == file.Name)
						return false;
				}
			}

			// Handle include directories
			if (file.IsDirectory && this.includeFolders != null) {
				valid = false;
				foreach (string dirName in this.includeFolders) {
					if (dirName == file.Name) {
						valid = true;
						break;
					}
				}

				if (!valid) {
					this.reason = FilterReason.InvalidFileName;
					return false;
				}
			}

			// Handle exclude directories
			if (file.IsDirectory && this.excludeFolders != null) {
				foreach (string dirName in this.excludeFolders) {
					if (dirName == file.Name) {
						this.reason = FilterReason.InvalidFileName;
						return false;
					}
				}
			}

			// Handle include file pattern
			if (file.IsFile && this.includeFilePattern != null) {
				Regex pattern = StringUtils.CreateRegex(this.includeFilePattern);

				if (!pattern.IsMatch(file.Name)) {
					this.reason = FilterReason.InvalidFileName;
					return false;
				}
			}

			// Handle exclude file pattern
			if (file.IsFile && this.excludeFilePattern != null) {
				Regex pattern = StringUtils.CreateRegex(this.excludeFilePattern);

				if (pattern.IsMatch(file.Name)) {
					this.reason = FilterReason.InvalidFileName;
					return false;
				}
			}

			// Handle include directory pattern
			if (file.IsDirectory && this.includeDirectoryPattern != null) {
				Regex pattern = StringUtils.CreateRegex(this.includeDirectoryPattern);

				if (!pattern.IsMatch(file.Name)) {
					this.reason = FilterReason.InvalidFileName;
					return false;
				}
			}

			// Handle exclude directory pattern
			if (file.IsDirectory && this.excludeDirectoryPattern != null) {
				Regex pattern = StringUtils.CreateRegex(this.excludeDirectoryPattern);

				if (file.Name != null && pattern.IsMatch(file.Name)) {
					this.reason = FilterReason.InvalidFileName;
					return false;
				}
			}

			// Handle extensions
			if (file.IsFile && this.extensions != null) {
				valid = false;
				foreach (string ext in this.extensions) {
					if (ext == PathUtils.GetExtension(file.AbsolutePath).ToLower()) {
						valid = true;
						break;
					}
				}

				if (!valid) {
					this.reason = FilterReason.InvalidExtension;
					return false;
				}
			}

			// Handle include wildcard pattern
			if (this.includeWildcardPattern != null) {
				string patternStr;

				patternStr = this.includeWildcardPattern;
				patternStr = Regex.Replace(patternStr, @"[*.(){}\?:\-!^$|/\\]", @"\$0");
				patternStr = patternStr.Replace(@"\*", @".*");
				patternStr = "^" + patternStr + "$";

				Regex pattern = new Regex(patternStr, RegexOptions.IgnoreCase);

				if (!pattern.IsMatch(file.Name)){
					this.reason = FilterReason.InvalidFileName;
					return false;
				}
			}

			return true;
		}
	}
}
