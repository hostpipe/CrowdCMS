/*
 * $Id: LanguagePack.cs 9 2007-05-27 10:47:07Z spocke $
 *
 * Copyright © 2007, Moxiecode Systems AB, All rights reserved. 
 */

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;

namespace Moxiecode.Manager.Utils {
	/// <summary>
	///  ..
	/// </summary>
	public class LanguagePack {
		private LanguageCollection languages;
		private string author, description;
		private int versionMinor, versionMajor;
		private DateTime releaseDate;

		/// <summary>
		///  Language pack constructor.
		/// </summary>
		public LanguagePack() {
			this.languages = new LanguageCollection();
		}

		/// <summary>
		///  Loads a XML language pack file.
		/// </summary>
		/// <param name="path">Path to language pack file.</param>
		public void Load(string path) {
			XmlDocument doc = new XmlDocument();

			doc.Load(path);

			// Get meta data
			this.author = doc.SelectNodes("//author/text()").Item(0).Value;
			this.description = doc.SelectNodes("//description/text()").Item(0).Value;
			this.versionMinor = Convert.ToInt32(doc.SelectNodes("//version/@minor").Item(0).Value);
			this.versionMajor = Convert.ToInt32(doc.SelectNodes("//version/@major").Item(0).Value);
			this.releaseDate = DateTime.ParseExact(doc.SelectNodes("//version/@releasedate").Item(0).Value, "yyyy-MM-dd", new System.Globalization.CultureInfo("en-US"));

			// Get all languages
			XmlNodeList languageNodes = doc.SelectNodes("//language");
			foreach (XmlNode languageNode in languageNodes) {
				XmlElement languageElm = (XmlElement) languageNode;
				XmlNodeList groupNodes = languageElm.SelectNodes("group");
				Language language = new Language(languageElm.GetAttribute("code"), languageElm.GetAttribute("title"), languageElm.GetAttribute("dir"));
				this.languages.Add(language.Code, language);

				foreach (XmlNode groupNode in groupNodes) {
					XmlElement groupElm = (XmlElement) groupNode;
					XmlNodeList itemNodes = groupElm.SelectNodes("item");
					Group group = new Group(groupElm.GetAttribute("target"));

					if (language.Groups[group.Target] != null)
						language.Groups.Remove(group.Target);

					language.Groups.Add(group.Target, group);

					foreach (XmlNode itemNode in itemNodes) {
						XmlElement itemElm = (XmlElement) itemNode;
						string key, val;

						key = itemElm.GetAttribute("name");
						if (itemElm.HasChildNodes)
							val = itemElm.FirstChild.Value;
						else
							val = itemElm.GetAttribute("value");

						if (group.Items[key] != null)
							group.Items.Remove(key);

						group.Items.Add(key, val);
					}
				}
			}
		}

		/// <summary>
		///  Language collection.
		/// </summary>
		public LanguageCollection Languages {
			get  {
				return this.languages;
			}
		}
	}

	/// <summary>
	///  LanguageCollection.
	/// </summary>
	public class LanguageCollection : Hashtable {
		/// <summary>
		///  ..
		/// </summary>
		public Language this[string key]  {
			get  {
				return (Language) base[key];
			}
		}
	}

	/// <summary>
	///  Language class.
	/// </summary>
	public class Language {
		private GroupCollection groups;
		private string code, title, dir;

		/// <summary>
		///  Language constructor.
		/// </summary>
		/// <param name="code">Lang code</param>
		/// <param name="title">Lang title</param>
		/// <param name="dir">Lang directionality</param>
		public Language(string code, string title, string dir) {
			this.code = code;
			this.title = title;
			this.dir = dir;
			this.groups = new GroupCollection();
		}

		/// <summary>
		///  Lang code
		/// </summary>
		public string Code {
			get {
				return this.code;
			}
		}

		/// <summary>
		///  Lang title
		/// </summary>
		public string Title {
			get {
				return this.title;
			}
		}

		/// <summary>
		///  Lang directionality
		/// </summary>
		public string Dir {
			get {
				return this.dir;
			}
		}

		/// <summary>
		///  Groups collection.
		/// </summary>
		public GroupCollection Groups {
			get  {
				return this.groups;
			}
		}
	}

	/// <summary>
	///  Groups collection.
	/// </summary>
	public class GroupCollection : Hashtable {
		/// <summary>
		///  Value property.
		/// </summary>
		public Group this[string key]  {
			get  {
				return (Group) base[key];
			}
		}
	}

	/// <summary>
	///  Group class.
	/// </summary>
	public class Group {
		private NameValueCollection items;
		private string target;

		/// <summary>
		///  Group constructor.
		/// </summary>
		/// <param name="target">Target name</param>
		public Group(string target) {
			this.items = new NameValueCollection();
			this.target = target;
		}

		/// <summary>
		///  Target name.
		/// </summary>
		public string Target {
			get {
				return this.target;
			}
		}

		/// <summary>
		///  Items collection.
		/// </summary>
		public NameValueCollection Items {
			get  {
				return this.items;
			}
		}
	}
}
