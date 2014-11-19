/*
 * $Id: Logger.cs 36 2007-06-16 18:48:37Z spocke $
 *
 * Copyright © 2007, Moxiecode Systems AB, All rights reserved. 
 */

using System;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Moxiecode.Manager.Utils;

namespace Moxiecode.Manager.Utils {
	/// <summary>Log level enumeration.</summary>
	public enum LoggerLevel {
		/// <summary>Debug level.</summary>
		Debug = 5,

		/// <summary>Information level.</summary>
		Info = 10,

		/// <summary>Warning level.</summary>
		Warn = 20,

		/// <summary>Error level.</summary>
		Error = 30,

		/// <summary>Fatal error level.</summary>
		Fatal = 40
	}

	/// <summary>
	///  Basic logger class, handles log levels, log rotation, message patterns and much more.
	///  Designed to be a smaller alterinative to Log4NET.
	/// </summary>
	public class Logger {
		private LoggerLevel level;
		private string path, timeFormat;
		private long maxSize;
		private int maxFiles;
		private string fileFormat;
		private string format;
		private static Object logFileLock = new Object();

		/// <summary>
		///  Creates a logger instance with the default log level of debug.
		/// </summary>
		public Logger() : this(LoggerLevel.Debug) {
		}

		/// <summary>
		///  Creates a logger instance with a specific log level.
		/// </summary>
		/// <param name="level"></param>
		public Logger(LoggerLevel level) {
			this.level = level;
			this.MaxSize = "10mb";
			this.MaxFiles = 0;
			this.format = "[{time}] [{level}] {message}";
			this.fileFormat = "{level}.log";
			this.timeFormat = "yyyy-MM-dd HH:mm:ss";
		}

		/// <summary>Log level to use when writing to file.</summary>
		public LoggerLevel Level {
			get { return this.level; }
			set { this.level = value; }
		}

		/// <summary>Log directory path. This is where the log files will be stored.</summary>
		public string Path {
			get { return path; }
			set { path = value; }
		}

		/// <summary>Log message format for example: [{time}] [{level}] {message}</summary>
		public string Format {
			get { return format; }
			set { format = value; }
		}

		/// <summary>Log file format for example: {level}.log</summary>
		public string FileFormat {
			get { return fileFormat; }
			set { fileFormat = value; }
		}

		/// <summary>Time format for example: yyyy-MM-dd HH:mm:ss</summary>
		public string TimeFormat {
			get { return timeFormat; }
			set { timeFormat = value; }
		}

		/// <summary>Max file size with prefix for example: 10m or 100k.</summary>
		public string MaxSize {
			get {
				return StringUtils.GetSizeStr(this.maxSize);
			}

			set {
				this.maxSize = Convert.ToInt32(Regex.Replace(value, "[^0-9]", ""));

				// Kb multipel
				if (value.ToLower().IndexOf('k') != -1)
					this.maxSize *= 1024;

				// Mb multipel
				if (value.ToLower().IndexOf('m') != -1)
					this.maxSize *= (1024 * 1024);
			}
		}

		/// <summary>Max log files in rotation.</summary>
		public int MaxFiles {
			get { return maxFiles; }
			set { maxFiles = value; }
		}

		/// <summary>
		///  Is debug enabled. This bool property can be useful to do conditonal debug
		///  chunks to reduce the extra overhead to build the debug message.
		/// </summary>
		public bool IsDebug {
			get { return this.level >= LoggerLevel.Debug; }
		}

		/// <summary>
		///  Is info enabled. This bool property can be useful to do conditonal info
		///  chunks to reduce the extra overhead to build the info message.
		/// </summary>
		public bool IsInfo {
			get { return this.level >= LoggerLevel.Info; }
		}

		/// <summary>
		///  Is warning enabled. This bool property can be useful to do conditonal warning
		///  chunks to reduce the extra overhead to build the warning message.
		/// </summary>
		public bool IsWarn {
			get { return this.level >= LoggerLevel.Error; }
		}

		/// <summary>
		///  Is error enabled. This bool property can be useful to do conditonal error
		///  chunks to reduce the extra overhead to build the error message.
		/// </summary>
		public bool IsError {
			get { return this.level >= LoggerLevel.Error; }
		}

		/// <summary>
		///  Is fatal enabled. This bool property can be useful to do conditonal fatal
		///  chunks to reduce the extra overhead to build the fatal message.
		/// </summary>
		public bool IsFatal {
			get { return this.level >= LoggerLevel.Error; }
		}

		/// <summary>
		///  Level string name, useful when the logger
		///  level comes from a config or page parameter.
		/// </summary>
		public string LevelName {
			set {
				switch (value.ToLower()) {
					case "debug":
						this.level = LoggerLevel.Debug;
						break;

					case "info":
						this.level = LoggerLevel.Info;
						break;

					case "warn":
					case "warning":
						this.level = LoggerLevel.Warn;
						break;

					case "error":
						this.level = LoggerLevel.Error;
						break;

					case "fatal":
						this.level = LoggerLevel.Fatal;
						break;

					default:
						this.level = LoggerLevel.Fatal;
						break;
				}
			}

			get {
				switch (this.level) {
					case LoggerLevel.Debug:
						return "debug";

					case LoggerLevel.Info:
						return "info";

					case LoggerLevel.Warn:
						return "warn";

					case LoggerLevel.Error:
						return "error";

					case LoggerLevel.Fatal:
						return "fatal";
				}

				return null;
			}
		}
		
		/// <summary>Logs a debug message if the debug level is enabled.</summary>
		/// <param name="args">Arguments with items to write to log.</param>
		public void Debug(params object[] args) {
			this.Trace(LoggerLevel.Debug, args);
		}

		/// <summary>Logs a info message if the debug level is enabled.</summary>
		/// <param name="args">Arguments with items to write to log.</param>
		public void Info(params object[] args) {
			this.Trace(LoggerLevel.Info, args);
		}

		/// <summary>Logs a warn message if the debug level is enabled.</summary>
		/// <param name="args">Arguments with items to write to log.</param>
		public void Warn(params object[] args) {
			this.Trace(LoggerLevel.Warn, args);
		}

		/// <summary>Logs a error message if the debug level is enabled.</summary>
		/// <param name="args">Arguments with items to write to log.</param>
		public void Error(params object[] args) {
			this.Trace(LoggerLevel.Error, args);
		}

		/// <summary>Logs a fatal message if the debug level is enabled.</summary>
		/// <param name="args">Arguments with items to write to log.</param>
		public void Fatal(params object[] args) {
			this.Trace(LoggerLevel.Fatal, args);
		}

		/// <summary>Logs a message if the specified level is enabled.</summary>
		/// <param name="level">Log level to write the message as.</param>
		/// <param name="args">Arguments with items to write to log.</param>
		public void Trace(LoggerLevel level, object[] args) {
			string msg, levelName = "UNKNOWN", logFile;
			StreamWriter writer = null;
			StringBuilder strBuilder;
			FileInfo logFileInfo;

			if (level < this.level)
				return;

			strBuilder = new StringBuilder();
			foreach (object obj in args) {
				if (obj == null)
					strBuilder.Append("null");
				else
					strBuilder.Append(obj.ToString());

				if (obj != args[args.Length - 1])
					strBuilder.Append(", ");
			}

			switch (level) {
				case LoggerLevel.Debug:
					levelName = "DEBUG";
					break;

				case LoggerLevel.Info:
					levelName = "INFO";
					break;

				case LoggerLevel.Warn:
					levelName = "WARN";
					break;

				case LoggerLevel.Error:
					levelName = "ERROR";
					break;

				case LoggerLevel.Fatal:
					levelName = "FATAL";
					break;
			}

			// Replace vars
			msg = this.format;
			msg = msg.Replace("{message}", strBuilder.ToString());
			msg = msg.Replace("{time}", StringUtils.GetDate(DateTime.Now, this.timeFormat));
			msg = msg.Replace("{level}", levelName);
			logFile = this.fileFormat;
			logFile = logFile.Replace("{level}", levelName.ToLower());
			logFile = PathUtils.AddTrailingSlash(this.Path) + logFile;

			// Log rotate
			if (this.maxFiles > 0) {
				logFileInfo = new FileInfo(logFile);
				if (logFileInfo.Exists && logFileInfo.Length + msg.Length > this.maxSize) {
					// Roll them
					for (int i=this.maxFiles - 1; i>=1; i--) {
						if (File.Exists(logFile + "." + i))
							File.Move(logFile + "." + i, logFile + "." + (i + 1));
					}

					// Move current to new empty slot
					File.Move(logFile, logFile + ".1");

					// Delete last one
					if (File.Exists(logFile + "." + this.maxFiles))
						File.Delete(logFile + "." + this.maxFiles);
				}
			}

			// Thread lock
			lock (logFileLock) {
				// Write message to log file
				try {
					writer = new StreamWriter(File.Open(logFile, FileMode.Append));
					writer.WriteLine(msg);
				} finally {
					if (writer != null)
						writer.Close();
				}
			}
		}
	}
}
