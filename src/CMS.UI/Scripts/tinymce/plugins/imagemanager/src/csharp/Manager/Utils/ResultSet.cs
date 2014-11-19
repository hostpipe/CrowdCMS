/*
 * $Id: ResultSet.cs 9 2007-05-27 10:47:07Z spocke $
 *
 * Copyright © 2007, Moxiecode Systems AB, All rights reserved. 
 */

using System;
using System.Collections;
using System.Collections.Specialized;

namespace Moxiecode.Manager.Utils {
	/// <summary>
	/// 
	/// </summary>
	public class ArrayListList : ArrayList {
		/// <summary>
		/// 
		/// </summary>
		public new ArrayList this[int i] {
			get {
				if (i < 0 || i >= base.Count || base[i] == null)
					throw new ArgumentException("Invalid index value.");

				return (ArrayList) base[i];
			}

			set {
				if (i != -1 && i < base.Count)
					base[i] = value;
			}
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class StringList : ArrayList {
		/// <summary>
		/// 
		/// </summary>
		public new string this[int i] {
			get {
				if (i < 0 || i >= base.Count || base[i] == null)
					throw new ArgumentException("Invalid index value.");

				return (string) base[i];
			}

			set {
				if (i != -1 && i < base.Count)
					base[i] = value;
			}
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class NameObjectCollection : DictionaryBase {
		/// <summary>
		/// 
		/// </summary>
		public object this[string key] {
			get { return (object) this.Dictionary[key]; }
			set { this.Dictionary[key] = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="obj"></param>
		public void Add(string key, object obj) {
			this.Dictionary.Add(key, obj);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool Contains(string key) {
			return this.Dictionary.Contains(key);
		}

		/// <summary>
		/// 
		/// </summary>
		public ICollection Keys {
			get {return this.Dictionary.Keys;}
		}
	}

	/// <summary>
	/// Description of ResultSet.
	/// </summary>
	public class ResultSet {
		private NameObjectCollection header;
		private ManagerConfig config;
		private StringList columns;
		private ArrayListList data;

		/// <summary>
		/// 
		/// </summary>
		public ResultSet() : this(null, new NameObjectCollection()) {
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="columns"></param>
		public ResultSet(string[] columns) : this(columns, new NameObjectCollection()) {
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="columns"></param>
		/// <param name="header"></param>
		public ResultSet(string[] columns, NameObjectCollection header) {
			this.header = header;
			this.config = null;
			this.columns = new StringList();
			this.data = new ArrayListList();

			if (columns != null) {
				foreach (string col in columns)
					this.columns.Add(col);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public NameObjectCollection Header {
			get { return header; }
		}

		/// <summary>
		/// 
		/// </summary>
		public StringList Columns {
			get { return columns; }
		}

		/// <summary>
		/// 
		/// </summary>
		public ArrayListList Data {
			get { return data; }
		}

		/// <summary>
		/// 
		/// </summary>
		public ManagerConfig Config {
			get { return config; }
			set { config = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public Hashtable this[int index] {
			get {
				Hashtable hash = new Hashtable();

				for (int i=0; i<this.data[index].Count; i++)
					hash[this.columns[i]] = this.data[index][i];

				return hash;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		public void Add(params object[] args) {
			ArrayList row = new ArrayList();

			foreach (object obj in args)
				row.Add(obj);

			this.Data.Add(row);
		}
	}
}
