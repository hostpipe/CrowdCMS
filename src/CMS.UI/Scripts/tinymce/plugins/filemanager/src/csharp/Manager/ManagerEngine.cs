/*
 * $Id: ManagerEngine.cs 819 2010-11-01 11:23:50Z spocke $
 *
 * Copyright © 2007, Moxiecode Systems AB, All rights reserved. 
 */

using System;
using System.Web;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using Moxiecode.Manager.FileSystems;
using Moxiecode.Manager.Utils;
using System.Text.RegularExpressions;

namespace Moxiecode.Manager {
	/// <summary>
	/// 
	/// </summary>
	public enum EventType {
		/// <summary></summary>
		Authenticate,

		/// <summary></summary>
		PreInit,

		/// <summary></summary>
		Init,

		/// <summary></summary>
		Login,

		/// <summary></summary>
		Logout,

		/// <summary></summary>
		BeforeFileAction,

		/// <summary></summary>
		FileAction,

		/// <summary></summary>
		BeforeRPC,

		/// <summary></summary>
		BeforeStream,

		/// <summary></summary>
		Stream,

		/// <summary></summary>
		AfterStream,

		/// <summary></summary>
		BeforeUpload,

		/// <summary></summary>
		AfterUpload,

		/// <summary></summary>
		CustomInfo,

		/// <summary></summary>
		InsertFile,

		/// <summary></summary>
		RPC,

		/// <summary></summary>
		Upload,

		/// <summary></summary>
		KeepAlive
	}

	/// <summary>
	///  
	/// </summary>
	public class PluginList : ArrayList {
		/// <summary>
		///  
		/// </summary>
		public new IPlugin this[int i] {
			get {
				if (i < 0 || i >= base.Count || base[i] == null)
					throw new ArgumentException("Invalid index value.");

				return (IPlugin) base[i];
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
	public class FileFactoryDictionary : DictionaryBase {
		/// <summary>
		/// 
		/// </summary>
		public IFileFactory this[string key] {
			get { return (IFileFactory) this.Dictionary[key]; }
			set { this.Dictionary[key] = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="file_factory"></param>
		public void Add(string key, IFileFactory file_factory) {
			this.Dictionary.Add(key, file_factory);
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
	/// Description of MyClass.
	/// </summary>
	public class ManagerEngine {
		private StringList rootPaths;
		private LanguagePack langPack;
		private PluginList plugins;
		private Hashtable pluginLookup;
		private FileFactoryDictionary fileSystems;
		private Logger logger;
		private string wwwroot, prefix, managerPath, invalidFileMsg;
		private MimeTypes mimeTypes;
		private ManagerConfig config;
		private NameValueCollection rootNames;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="prefix"></param>
		public ManagerEngine(string prefix) : this(prefix, null) {
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="prefix"></param>
		/// <param name="manager_path"></param>
		public ManagerEngine(string prefix, string manager_path) {
			this.rootPaths = new StringList();
			this.rootNames = new NameValueCollection();
			this.plugins = new PluginList();
			this.fileSystems = new FileFactoryDictionary();
			this.langPack = null;
			this.logger = new Logger();
			this.pluginLookup = new Hashtable();

			if (manager_path != null)
				this.ManagerPath = manager_path;
			else
				this.ManagerPath = System.Web.HttpContext.Current.Server.MapPath("..");

			this.Plugins.Add(new CorePlugin());

			ManagerConfig coreConfig = (ManagerConfig) System.Web.HttpContext.Current.GetConfig("CorePlugin");

			this.prefix = prefix;

			// Add instance of each plugin
			foreach (string className in coreConfig.Plugins) {
				IPlugin plugin = (IPlugin) InstanceFactory.CreateInstance(className);

				if (plugin == null)
					throw new ManagerException("Could not create instance of plugin class: " + className);

				this.Plugins.Add(plugin);
			}

			this.DispatchEvent(EventType.PreInit, prefix);
			this.SetupConfigItems();

			// Setup logger
			if (!this.Config.GetBool("log.enabled", false))
				this.logger.Level = LoggerLevel.Fatal;
			else
				this.logger.LevelName = this.Config.Get("log.level", "fatal");

			this.logger.Path = this.ToAbsPath(this.Config.Get("log.path", "logs"));
			this.logger.FileFormat = this.Config.Get("log.filename", "{level}.log");
			this.logger.Format = this.Config.Get("log.format", "[{time}] [{level}] {message}");
			this.logger.MaxSize = this.Config.Get("log.max_size", "100k");
			this.logger.MaxFiles = this.Config.GetInt("log.max_files", 10);

			// Add instance of each plugin
			AssemblyLoader loader = new AssemblyLoader();
			foreach (string className in this.Config.Plugins) {
				IPlugin plugin = (IPlugin) loader.CreateInstance(className);

				if (plugin == null)
					throw new Exception("Could not create instance of plugin class: " + className);

				this.Plugins.Add(plugin);

				// Add by class name and by short name
				this.pluginLookup[className] = plugin;

				if (plugin.ShortName != null)
					this.pluginLookup[plugin.ShortName] = plugin;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string Prefix {
			get { return prefix; }
		}

		/// <summary>
		///  
		/// </summary>
		public string ManagerPath {
			get { return managerPath; }
			set { managerPath = PathUtils.RemoveTrailingSlash(PathUtils.ToUnixPath(value)); }
		}

		/// <summary>
		/// 
		/// </summary>
		public string SiteRoot {
			get {
				if (this.wwwroot == null) {
					this.wwwroot = Config["preview.wwwroot"];

					// Is absolute
					if (PathUtils.IsAbsolutePath(this.wwwroot))
						return PathUtils.RemoveTrailingSlash(PathUtils.ToUnixPath(this.wwwroot));

					// Is empty/default
					if (this.wwwroot == null || this.wwwroot == "") {
						string siteRoot;

						try {
							// This will fail on .NET Development Servers
							siteRoot = System.Web.HttpContext.Current.Server.MapPath("/");
						} catch {
							siteRoot = System.Web.HttpContext.Current.Request.PhysicalApplicationPath;
						}

						this.wwwroot = PathUtils.RemoveTrailingSlash(PathUtils.ToUnixPath(siteRoot));
					} else
						this.wwwroot = PathUtils.RemoveTrailingSlash(PathUtils.ToUnixPath(this.MapPath(this.wwwroot)));
				}

				return this.wwwroot;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Logger Logger {
			get { return this.logger; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string InvalidFileMsg {
			get { return invalidFileMsg; }
		}

		/// <summary>
		/// 
		/// </summary>
		public PluginList Plugins {
			get { return this.plugins; }
		}

		/// <summary>
		/// 
		/// </summary>
		public MimeTypes MimeTypes {
			get {
				if (mimeTypes == null)
					mimeTypes = new MimeTypes(this.MapPath("mime.types"));

				return mimeTypes;
			}

			set { mimeTypes = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public FileFactoryDictionary FileSystems {
			get { return fileSystems; }
		}

		/// <summary>
		/// 
		/// </summary>
		public StringList RootPaths {
			get { return rootPaths; }
			set { rootPaths = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public NameValueCollection RootNames {
			get { return rootNames; }
		}

		/// <summary>
		/// 
		/// </summary>
		public LanguagePack LangPack {
			get {
				if (this.langPack == null) {
					this.langPack = new LanguagePack();
					this.langPack.Load(this.MapPath(@"language/" + this.prefix + "/" + this.Config.Get("general.language", "en") + ".xml"));
				}

				return this.langPack;
			}

			set {
				this.langPack = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public ManagerConfig Config {
			get {
				return this.config;
			}

			set {
				this.wwwroot = null; // Clear the cached wwwroot
				this.config = value;
				this.SetupConfigItems();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool IsAuthenticated {
			get {
				string authenticator = this.Config.Get("authenticator", "");
				string[] authClasses = authenticator.Split(new char[]{'+', '|'});

				// No auth
				if (authenticator == null || authenticator.Trim() == "")
					return true;

				if (authenticator.IndexOf('+') != -1) {
					// AND mode
					foreach (string auth in authClasses) {
						IPlugin plugin = (IPlugin) this.pluginLookup[auth.Trim()];

						//this.logger.Debug(auth);

						if (plugin != null) {
							if (!plugin.OnAuthenticate(this))
								return false;
						} else
							throw new ManagerException("Fatal: Could not find authenticator: " + auth.Trim());
					}

					this.SetupConfigItems();
					// this.CreateRoots();

					return true;
				} else {
					// OR mode
					foreach (string auth in authClasses) {
						IPlugin plugin = (IPlugin) this.pluginLookup[auth.Trim()];

						if (plugin != null) {
							if (plugin.OnAuthenticate(this)) {
								this.SetupConfigItems();
								return true;
							}
						} else
							throw new ManagerException("Fatal: Could not find authenticator: " + auth.Trim());
					}				
				}

				this.SetupConfigItems();
				// this.CreateRoots();

				return false;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="long_name"></param>
		/// <param name="short_name"></param>
		/// <returns></returns>
		public ManagerConfig ResolveConfig(string long_name, string short_name) {
			ManagerConfig config;

			if ((config = (ManagerConfig) HttpContext.Current.GetConfig(long_name)) != null)
				return config;

			if ((config = (ManagerConfig) HttpContext.Current.GetConfig(short_name)) != null)
				return config;

			return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="config"></param>
		/// <param name="config_prefixes"></param>
		/// <returns></returns>
		public ManagerConfig GetJSConfig(ManagerConfig config, string config_prefixes) {
			string[] encrypted = new string[]{"filesystem.path", "filesystem.rootpath", "filesystem.directory_templates"};
			ManagerConfig outConfig = new ManagerConfig();
			string[] names, prefixes;
			string prefix;
			bool validPrefix;
			int pos;

			prefixes = config_prefixes.Split(new char[]{','});

			foreach (string key in config.AllKeys) {
				if ((pos = key.IndexOf(".allow_export")) != -1) {
					if (config[key] == null)
						continue;

					names = config[key].Split(new char[]{','});
					prefix = key.Substring(0, pos);
					validPrefix = false;

					if (config_prefixes != "*") {
						foreach (string targetPrefix in prefixes) {
							if (targetPrefix == prefix) {
								validPrefix = true;
								break;
							}
						}
					} else
						validPrefix = true;

					// All exported or the requested visible ones
					if (validPrefix) {
						foreach (string name in names) {
							string outKey = prefix + "." + name;

							// Encrypt if needed
							foreach (string encryptKey in encrypted) {
								if (encryptKey == outKey) {
									outConfig[outKey] = this.EncryptPath(config[outKey]);
									break;
								}
							}

							// Output normal
							if (outConfig[outKey] == null)
								outConfig[outKey] = config[outKey];
						}
					}
						
				}
			}

			return outConfig;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public string ToAbsPath(string path) {
			// Is relative force it absolute
			if (!PathUtils.IsAbsolutePath(path))
				path = new DirectoryInfo(this.MapPath(path)).FullName;

			return PathUtils.ToUnixPath(path);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public string ToVisualPath(string path) {
			return this.ToVisualPath(path, null);
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <param name="root"></param>
		/// <returns></returns>
		public string ToVisualPath(string path, string root) {
			string fs = "file";
			Match match;

			if (!this.Config.GetBool("general.user_friendly_paths", false))
				return path;

			// Parse file system
			match = Regex.Match(path, @"([a-z]+):\/\/(.+)", RegexOptions.IgnoreCase);
			if (match.Groups.Count == 3) {
				fs = match.Groups[1].Value;
				path = match.Groups[2].Value;				
			}

			path = this.DecryptPath(path);

			// Use specified root
			if (root != null) {
				if (path.IndexOf(root) == 0)
					path = path.Substring(root.Length);

				if (path == "")
					path = "/";

				// Re-attach fs
				if (fs != "file")
					path = fs + "://" + path;

				return this.EncryptPath(path);
			}

			// Use config roots

			// Replace root names
			foreach (string rootPath in this.rootNames.Keys)
				path = path.Replace(rootPath, "/" + this.rootNames[rootPath]);

			if (this.RootPaths.Count > 1) {
				foreach (string rootPath in this.RootPaths)
					path = path.Replace(rootPath, "/" + PathUtils.BaseName(rootPath));
			} else
				path = path.Replace(this.RootPaths[0], "/");

			// Remove / in beginning to avoid //files
			if (path.StartsWith("//"))
				path = path.Substring(1);

			// Re-attach fs
			if (fs != "file")
				path = fs + "://" + path;

			return this.EncryptPath(path);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public string MapPath(string path) {
			// Handle application absolute paths
			if (path.StartsWith("~/"))
				return PathUtils.ToUnixPath(HttpContext.Current.Server.MapPath(path));

			return PathUtils.ToUnixPath(Path.GetFullPath(this.ManagerPath + "/" + path));
		}

		/**
		 * Dispatches a event to all registered plugins. This method will loop through all plugins and call the specific event if this
		 * event method returns false the chain will be terminated.
		 *
		 * @param String $event Event name to be dispatched for example onAjaxCommand.
		 * @param Array $args Optional array with arguments.
		 * @return Bool Returns true of a plugin returned true, false if not.
		 */
		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public bool DispatchEvent(EventType type, params object[] args) {
			foreach (IPlugin plugin in this.plugins) {
				if (plugin.Prefix != null && this.prefix != plugin.Prefix)
					continue;

				switch (type) {
					case EventType.PreInit:
						if (!plugin.OnPreInit(this, (string) args[0]))
							return false;

						break;
						
					case EventType.Init:
						if (!plugin.OnInit(this))
							return false;

						break;

					case EventType.Login:
						if (!plugin.OnLogin(this))
							return false;

						break;

					case EventType.Logout:
						if (!plugin.OnLogout(this))
							return false;

						break;

					case EventType.BeforeFileAction:
						if (args.Length == 3) {
							if (!plugin.OnBeforeFileAction(this, (FileAction) args[0], (IFile) args[1], (IFile) args[2]))
								return false;
						} else if (args.Length == 2) {
							if (!plugin.OnBeforeFileAction(this, (FileAction) args[0], (IFile) args[1], null))
								return false;
						}

						break;

					case EventType.FileAction:
						if (args.Length == 3) {
							if (!plugin.OnFileAction(this, (FileAction) args[0], (IFile) args[1], (IFile) args[2]))
								return false;
						} else if (args.Length == 2) {
							if (!plugin.OnFileAction(this, (FileAction) args[0], (IFile) args[1], null))
								return false;
						}

						break;

					case EventType.BeforeRPC:
						if (!plugin.OnBeforeRPC(this, (string) args[0], (Hashtable) args[1]))
							return false;

						break;

					case EventType.BeforeStream:
						if (!plugin.OnBeforeStream(this, (string) args[0], (NameValueCollection) args[1]))
							return false;

						break;

					case EventType.Stream:
						if (!plugin.OnStream(this, (string) args[0], (NameValueCollection) args[1]))
							return false;

						break;

					case EventType.AfterStream:
						if (!plugin.OnAfterStream(this, (string) args[0], (NameValueCollection) args[1]))
							return false;

						break;

					case EventType.BeforeUpload:
						if (!plugin.OnBeforeUpload(this, (string) args[0], (NameValueCollection) args[1]))
							return false;

						break;

					case EventType.AfterUpload:
						if (!plugin.OnAfterUpload(this, (string) args[0], (NameValueCollection) args[1]))
							return false;

						break;

					case EventType.CustomInfo:
						if (!plugin.OnCustomInfo(this, (IFile) args[0], (string) args[1], (Hashtable) args[2]))
							return false;

						break;

					case EventType.InsertFile:
						if (!plugin.OnInsertFile(this, (IFile) args[0]))
							return false;

						break;
				}
			}

			return true;
		}

		/**
		 * Executes a event in all registered plugins if a plugin returns a object or array the execution chain will be
		 * terminated.
		 *
		 * @param String $event Event name to be dispatched for example onAjaxCommand.
		 * @param Array $args Optional array with arguments.
		 * @return Bool Returns true of a plugin returned true, false if not.
		 */
		public object ExecuteEvent(EventType type, params object[] args) {
			object returnVal;

			foreach (IPlugin plugin in this.plugins) {
				if (plugin.Prefix != null && this.prefix != plugin.Prefix)
					continue;

				switch (type) {
					case EventType.RPC:
						returnVal = plugin.OnRPC(this, (string) args[0], (Hashtable) args[1]);

						if (returnVal != null)
							return returnVal;

						break;

					case EventType.Upload:
						returnVal = plugin.OnUpload(this, (string) args[0], (NameValueCollection) args[1]);

						if (returnVal != null)
							return returnVal;

						break;
				}
			}

			return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public string DecryptPath(string path) {
			if (path == null)
				return null;

			if (path == "{default}")
				return this.Config["filesystem.path"];

			// Is relative path force it absolute
		 	// breaks imagemanager.saveImage function
			if (path.IndexOf(':') == -1 && path.IndexOf('/') != 0 && path.IndexOf('\\') != 0 && path.IndexOf('{') == -1)
		 		path = this.MapPath(path);

			if (config.GetBool("general.encrypt_paths", false)) {
				int count = 0;

				foreach (string rootPath in this.rootPaths)
					path = path.Replace("{" + count++ + "}", rootPath);
			}

			return PathUtils.ToUnixPath(path);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public string EncryptPath(string path) {
			ManagerConfig config = this.config;

			if (config.GetBool("general.encrypt_paths", false)) {
				int count = 0;

				foreach (string rootPath in this.rootPaths)
					path = path.Replace(rootPath, "{" + count++ + "}");
			}

			return path;
		}

		/// <summary>
		///  Returns a site absolute path from a absolute file system path for
		///  example /www/mywwwroot/mydir/myfile.htm will be converted to /mydir/myfile.htm.
		/// </summary>
		/// <param name="abs_path">Absolute path for example /mydir/myfile.htm</param>
		/// <returns>Site absolute path (URI) or empty string on failure.</returns>
		public string ConvertPathToURI(string abs_path) {
			string uri;

			//if (!PathUtils.IsChildPath(this.SiteRoot, abs_path))
			//	throw new ManagerException("Path is not allowed.");

			uri = abs_path.Substring(this.SiteRoot.Length);

			if (this.logger.IsDebug)
				this.logger.Info("ConvertPathToURI: SiteRoot=" + this.SiteRoot + ", Path: " + abs_path + " -> URI: " + uri);

			return uri;
		}

		/// <summary>
		///  Converts a URI like /mydir/myfile.htm to /site/root/mydir/myfile.htm
		/// </summary>
		/// <param name="uri">URI to convert into path.</param>
		/// <returns>Absolute filesytem path to URI.</returns>
		public string ConvertURIToPath(string uri) {
			if (this.logger.IsDebug)
				this.logger.Info("ConvertURIToPath: SiteRoot=" + this.SiteRoot + ", URI: " + uri + " -> Path: " + PathUtils.RemoveTrailingSlash(this.SiteRoot) + uri);

			return PathUtils.RemoveTrailingSlash(this.SiteRoot) + uri;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public IFile GetFile(string path) {
			return this.GetFile(path, "", FileType.Unknown);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <param name="child"></param>
		/// <returns></returns>
		public IFile GetFile(string path, string child) {
			return this.GetFile(path, child, FileType.Unknown);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <param name="child"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public IFile GetFile(string path, string child, FileType type) {
			string fs = "file";
			Match match;
			IFile file;

			if (path == null)
				throw new ManagerException("{#error.file_not_exists}");

			// Parse file system
			match = Regex.Match(path, @"([a-z]+):\/\/(.+)", RegexOptions.IgnoreCase);
			if (match.Groups.Count == 3) {
				fs = match.Groups[1].Value;
				path = match.Groups[2].Value;				
			}

			// Get file from factory
			IFileFactory factory = this.fileSystems[fs];

			if (factory == null)
				throw new ManagerException(ManagerErrorLevel.Fatal, "{#error.no_filesystem}");

			if (fs == "file" && !VerifyPath(path))
				throw new ManagerException(ManagerErrorLevel.Fatal, "{#error.no_access}");

			file = factory.GetFile(this, path, child, type);

			if (fs == "file" && !VerifyPath(file.AbsolutePath))
				throw new ManagerException(ManagerErrorLevel.Fatal, "{#error.no_access}");

			if (fs == "file" && (child.IndexOf('/') != -1 || child.IndexOf('\\') != -1))
				throw new ManagerException(ManagerErrorLevel.Fatal, "{#error.no_access}");

			return file;
		}

		/// <summary>
		///  Returns FS from path.
		/// </summary>
		/// <param name="path">Path</param>
		/// <returns>FS</returns>
		public string GetFSFromPath(string path) {
			string fs = "file";
			Match match;

			// Parse file system
			match = Regex.Match(path, @"([a-z]+):\/\/(.+)", RegexOptions.IgnoreCase);
			if (match.Groups.Count == 3)
				fs = match.Groups[1].Value;

			return fs;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="group"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public string GetLang(string group, string key) {
			return this.GetLang(group, key, null);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="group"></param>
		/// <param name="key"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public string GetLang(string group, string key, NameValueCollection args) {
			Language lang;
			Moxiecode.Manager.Utils.Group groupObj;
			string val;

			lang = this.LangPack.Languages[this.Config.Get("general.language", "en")];
			if (lang == null)
				return "$" + group + "." + key + "$";

			groupObj = lang.Groups[group];
			if (groupObj == null)
				return "$" + group + "." + key + "$";

			val = groupObj.Items[key];
			if (val == null)
				return "$" + group + "." + key + "$";

			if (args != null) {
				foreach (string argKey in args.AllKeys)
					val = val.Replace("{" + argKey + "}", args[argKey]);
			}

			return val;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="tool"></param>
		/// <returns></returns>
		public bool IsToolEnabled(string tool) {
			return this.IsToolEnabled(tool, this.Config);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="tool"></param>
		/// <param name="config"></param>
		/// <returns></returns>
		public bool IsToolEnabled(string tool, ManagerConfig config) {
			string[] ar;

			// Is disabled
			ar = config["general.disabled_tools"].Split(new char[] {','});
			foreach (string chkTool in ar) {
				if (chkTool == tool)
					return false;
			}

			// Is it there at all?
			ar = config["general.tools"].Split(new char[] {','});
			foreach (string chkTool in ar) {
				if (chkTool == tool)
					return true;
			}

			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public bool VerifyPath(string path) {
			string fs = "file";
			Match match;

			if (path == null)
				return false;

			// Parse file system
			match = Regex.Match(path, @"([a-z]+):\/\/(.+)", RegexOptions.IgnoreCase);
			if (match.Groups.Count == 3) {
				fs = match.Groups[1].Value;
				path = match.Groups[2].Value;				
			}

			// Accept all non local fs
			if (fs != "file")
				return true;

			foreach (string rootPath in this.RootPaths) {
				if (PathUtils.IsChildPath(rootPath, path))
					return true;
			}

			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>
		public bool VerifyFile(IFile file) {
			return this.VerifyFile(file, null, Config);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="file"></param>
		/// <param name="action"></param>
		/// <returns></returns>
		public bool VerifyFile(IFile file, string action) {
			return this.VerifyFile(file, action, Config);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="file"></param>
		/// <param name="action"></param>
		/// <param name="config"></param>
		/// <returns></returns>
		public bool VerifyFile(IFile file, string action, ManagerConfig config) {
			BasicFileFilter filter;

			// Verify filesystem config
			filter = new BasicFileFilter();
			filter.IncludeDirectoryPattern = config["filesystem.include_directory_pattern"];
			filter.ExcludeDirectoryPattern = config["filesystem.exclude_directory_pattern"];
			filter.IncludeFilePattern = config["filesystem.include_file_pattern"];
			filter.ExcludeFilePattern = config["filesystem.exclude_file_pattern"];
			filter.IncludeExtensions = config["filesystem.extensions"];

			this.invalidFileMsg = "{#error.invalid_filename}";

			if (!filter.Accept(file)) {
				if (filter.Reason == FilterReason.InvalidFileName) {
					if (file.IsDirectory)
						this.invalidFileMsg = config["filesystem.invalid_directory_name_msg"];
					else
						this.invalidFileMsg = config["filesystem.invalid_file_name_msg"];

					if (this.invalidFileMsg.Length == 0)
						this.invalidFileMsg = "{#error.invalid_filename}";
				}

				return false;
			}

			// Verify action specific config
			filter = new BasicFileFilter();

			if (action != null) {
				if (config[action + ".include_directory_pattern"] != null)
					filter.IncludeDirectoryPattern = config[action + ".include_directory_pattern"];

				if (config[action + ".exclude_directory_pattern"] != null)
					filter.ExcludeDirectoryPattern = config[action + ".exclude_directory_pattern"];

				if (config[action + ".include_file_pattern"] != null)
					filter.IncludeFilePattern = config[action + ".include_file_pattern"];

				if (config[action + ".exclude_file_pattern"] != null)
					filter.ExcludeFilePattern = config[action + ".exclude_file_pattern"];

				if (config[action + ".extensions"] != null)
					filter.IncludeExtensions = config[action + ".extensions"];
			}

			if (!filter.Accept(file)) {
				if (filter.Reason == FilterReason.InvalidFileName) {
					if (file.IsDirectory)
						this.invalidFileMsg = config[action + ".invalid_directory_name_msg"];
					else
						this.invalidFileMsg = config[action + ".invalid_file_name_msg"];

					if (this.invalidFileMsg == null || this.invalidFileMsg.Length == 0)
						this.invalidFileMsg = "{#error.invalid_filename}";
				}

				return false;
			}

			return true;
		}

		#region private methods

		private void SetupConfigItems() {
			ArrayList newRoots = new ArrayList();
			string rootPath;
			bool found;

			this.rootNames = new NameValueCollection();
			this.RootPaths = new StringList();

			// Force absolute paths for configured rootpaths
			foreach (string rootChunk in this.Config["filesystem.rootpath"].Trim().Split(new char[]{';'})) {
				string[] nameValue = rootChunk.Trim().Split(new char[]{'='});

				if (nameValue.Length > 1) {
					rootPath = PathUtils.ToUnixPath(this.ToAbsPath(nameValue[1]));

					this.rootNames[rootPath] = nameValue[0];
					this.RootPaths.Add(rootPath);

					newRoots.Add(nameValue[0] + "=" + rootPath);
				} else {
					this.RootPaths.Add(PathUtils.ToUnixPath(this.ToAbsPath(nameValue[0])));
					newRoots.Add(PathUtils.ToUnixPath(nameValue[0]));
				}
			}

			// Setup new root path list with absolute paths
			this.Config["filesystem.rootpath"] = String.Join(";", (string[]) newRoots.ToArray(typeof(String)));

			// Force absolute path
			if (this.Config.Get("filesystem.path", "") == "")
				this.Config["filesystem.path"] = PathUtils.ToUnixPath(this.RootPaths[0]);
			else
				this.Config["filesystem.path"] = this.ToAbsPath(this.Config["filesystem.path"]);

		 	// Path is not a child path of root then override path with root
		 	found = false;
		 	foreach (string root in this.RootPaths) {
		 		if (PathUtils.IsChildPath(root, this.Config["filesystem.path"])) {
		 			found = true;
		 			break;
		 		}
		 	}

		 	// Path was not within any of the rootpaths use the first one
		 	if (!found)
			 	config["filesystem.path"] = this.RootPaths[0];

			// Setup absolute wwwroot
			if (this.Config.Get("preview.wwwroot", "") == "")
				this.Config["preview.wwwroot"] = this.SiteRoot;
			else
				this.Config["preview.wwwroot"] = this.ToAbsPath(PathUtils.ToUnixPath(this.Config["preview.wwwroot"]));

			// Setup preview.urlprefix
			if (config["preview.urlprefix"] != null) {
				config["preview.urlprefix"] = config["preview.urlprefix"].Replace("{proto}", System.Web.HttpContext.Current.Request.Url.Scheme);
				config["preview.urlprefix"] = config["preview.urlprefix"].Replace("{host}", System.Web.HttpContext.Current.Request.Url.Host);
				config["preview.urlprefix"] = config["preview.urlprefix"].Replace("{port}", "" + System.Web.HttpContext.Current.Request.Url.Port);
			}
		}
/*
		private void CreateRoots() {
			// No file systems yet
			if (this.FileSystems.Keys.Count == 0)
				return;

			// Try creating each root
			foreach (string root in this.RootPaths) {
				IFile rootFile = this.GetFile(root);

				if (!rootFile.Exists)
					rootFile.MkDir();
			}
		}
*/		
		#endregion
	}
}
