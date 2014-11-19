/*
 * $Id: BasicFileFilter.cs 293 2008-05-07 14:52:56Z spocke $
 *
 * Copyright © 2007, Moxiecode Systems AB, All rights reserved. 
 */

using System.Text.RegularExpressions;
using Moxiecode.Manager.Utils;
using System.Collections;

namespace Moxiecode.Manager.FileSystems {
	 /// <summary>
	 ///  This combines different filters into one.
	 /// </summary>
	 public class CombinedFileFilter : IFileFilter {
		private ArrayList filters;

		/// <summary>Main constructor.</summary>
		public CombinedFileFilter() {
			this.filters = new ArrayList();
		}

		/// <summary>
		///  Adds a file filter.
		/// </summary>
		/// <param name="filter">File filter to add.</param>
		public void AddFilter(IFileFilter filter) {
			this.filters.Add(filter);
		}
		
		/// <summary>Returns true or false if the file is accepted or not.</summary>
		/// <param name="file">File to verify.</param>
		/// <returns>true or false if the file is accepted or not.</returns>
		public bool Accept(IFile file) {
			foreach (IFileFilter filter in this.filters) {
				if (!filter.Accept(file))
					return false;
			}

			return true;
		}
	}
}
