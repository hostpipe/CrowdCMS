using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace CMS.DAL.Logging
{
    /// <summary>
    /// Provides global logging functionality.
    /// </summary>
    /// <remarks>Method names represent the level of log message.</remarks>
    public static partial class Log
    {
        static Log()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        #region DEBUG messages

        public static void Debug(object message)
        {
            LogManager.GetLogger("Default").Debug(message);
        }

        public static void Debug(string logName, object message)
        {
            LogManager.GetLogger(logName).Debug(message);
        }

        public static void Debug(string message, Exception exception)
        {
            LogManager.GetLogger("Default").Debug(message, exception);
        }

        public static void Debug(string logName, object message, Exception exception)
        {
            LogManager.GetLogger(logName).Debug(message, exception);
        }

        #endregion

        #region INFO messages

        public static void Info(object message)
        {
            LogManager.GetLogger("Default").Info(message);
        }

        public static void Info(string logName, object message)
        {
            LogManager.GetLogger(logName).Info(message);
        }

        public static void Info(string message, Exception exception)
        {
            LogManager.GetLogger("Default").Info(message, exception);
        }

        public static void Info(string logName, object message, Exception exception)
        {
            LogManager.GetLogger(logName).Info(message, exception);
        }

        #endregion

        #region WARN messages

        public static void Warn(object message)
        {
            LogManager.GetLogger("Default").Warn(message);
        }

        public static void Warn(string logName, object message)
        {
            LogManager.GetLogger(logName).Warn(message);
        }

        public static void Warn(string message, Exception exception)
        {
            LogManager.GetLogger("Default").Warn(message, exception);
        }

        public static void Warn(string logName, object message, Exception exception)
        {
            LogManager.GetLogger(logName).Warn(message, exception);
        }

        #endregion

        #region ERROR messages

        public static void Error(object message)
        {
            LogManager.GetLogger("Default").Error(message);
        }

        public static void Error(string logName, object message)
        {
            LogManager.GetLogger(logName).Error(message);
        }

        public static void Error(string message, Exception exception)
        {
            LogManager.GetLogger("Default").Error(message, exception);
        }

        public static void Error(string logName, object message, Exception exception)
        {
            LogManager.GetLogger(logName).Error(message, exception);
        }


        #endregion

        #region FATAL messages

        public static void Fatal(object message)
        {
            LogManager.GetLogger("Default").Fatal(message);
        }

        public static void Fatal(string logName, object message)
        {
            LogManager.GetLogger(logName).Fatal(message);
        }

        public static void Fatal(string message, Exception exception)
        {
            LogManager.GetLogger("Default").Fatal(message, exception);
        }

        public static void Fatal(string logName, object message, Exception exception)
        {
            LogManager.GetLogger(logName).Fatal(message, exception);
        }

        #endregion

    }
}
