using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace CMS.Utils.Diagnostics
{
    /// <summary>
    /// Application logger which bases on log4net and configuration stored in app.config / web.config
    /// </summary>
    public static class Log
    {

        #region Constructor(s)

        static Log()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        #endregion


        #region Multiple Exceptions

        public static void UnhandledExceptions(Exception[] exceptions, string source)
        {
            StringBuilder stackTrace = new StringBuilder();
            stackTrace.AppendLine(String.Format("Unhandled exception was thrown ({0})", source));

            foreach (Exception ex in exceptions)
            {
                Exception exception = ex;
                while (exception != null)
                {
                    stackTrace.AppendLine(exception.Message);
                    stackTrace.AppendLine(exception.StackTrace);

                    exception = exception.InnerException;
                }
            }

            Log.Fatal(stackTrace.ToString());
        }

        #endregion


        #region log INFO level message

        /// <summary>
        /// Add log entry for INFO logging level
        /// </summary>
        /// <param name="message">Log entry message</param>
        public static void Info<T>(string message)
        {
            LogManager.GetLogger(typeof(T)).Info(message);
        }

        /// <summary>
        /// Add log entry for INFO logging level
        /// </summary>
        /// <param name="message">Log entry message</param>
        public static void Info(string message)
        {
            LogManager.GetLogger("Default").Info(message);
        }

        /// <summary>
        /// Add log entry for INFO logging level
        /// </summary>
        /// <param name="message">Log entry message</param>
        /// <param name="exception">Exception which trace is logged</param>
        public static void Info<T>(string message, Exception exception)
        {
            LogManager.GetLogger(typeof(T)).Info(message, exception);
        }

        /// <summary>
        /// Add log entry for INFO logging level
        /// </summary>
        /// <param name="message">Log entry message</param>
        /// <param name="exception">Exception which trace is logged</param>
        public static void Info(string message, Exception exception)
        {
            LogManager.GetLogger("Default").Info(message, exception);
        }

        #endregion


        #region log DEBUG level message

        /// <summary>
        /// Add log entry for DEBUG logging level
        /// </summary>
        /// <param name="message">Log entry message</param>
        public static void Debug<T>(string message)
        {
            LogManager.GetLogger(typeof(T)).Debug(message);
        }

        /// <summary>
        /// Add log entry for DEBUG logging level
        /// </summary>
        /// <param name="message">Log entry message</param>
        public static void Debug(string message)
        {
            LogManager.GetLogger("Default").Debug(message);
        }

        /// <summary>
        /// Add log entry for DEBUG logging level
        /// </summary>
        /// <param name="message">Log entry message</param>
        /// <param name="exception">Exception which trace is logged</param>
        public static void Debug<T>(string message, Exception exception)
        {
            LogManager.GetLogger(typeof(T)).Debug(message, exception);
        }

        /// <summary>
        /// Add log entry for DEBUG logging level
        /// </summary>
        /// <param name="message">Log entry message</param>
        /// <param name="exception">Exception which trace is logged</param>
        public static void Debug(string message, Exception exception)
        {
            LogManager.GetLogger("Default").Debug(message, exception);
        }

        #endregion


        #region log WARNING level message

        /// <summary>
        /// Add log entry for WARNING logging level
        /// </summary>
        /// <param name="message">Log entry message</param>
        public static void Warn<T>(string message)
        {
            LogManager.GetLogger(typeof(T)).Warn(message);
        }

        /// <summary>
        /// Add log entry for WARNING logging level
        /// </summary>
        /// <param name="message">Log entry message</param>
        public static void Warn(string message)
        {
            LogManager.GetLogger("Default").Warn(message);
        }

        /// <summary>
        /// Add log entry for WARNING logging level
        /// </summary>
        /// <param name="message">Log entry message</param>
        /// <param name="exception">Exception which trace is logged</param>
        public static void Warn<T>(string message, Exception exception)
        {
            LogManager.GetLogger(typeof(T)).Warn(message, exception);
        }

        /// <summary>
        /// Add log entry for WARNING logging level
        /// </summary>
        /// <param name="message">Log entry message</param>
        /// <param name="exception">Exception which trace is logged</param>
        public static void Warn(string message, Exception exception)
        {
            LogManager.GetLogger("Default").Warn(message, exception);
        }

        #endregion


        #region log ERROR level message

        /// <summary>
        /// Add log entry for ERROR logging level
        /// </summary>
        /// <param name="message">Log entry message</param>
        public static void Error<T>(string message)
        {
            LogManager.GetLogger(typeof(T)).Error(message);
        }

        /// <summary>
        /// Add log entry for ERROR logging level
        /// </summary>
        /// <param name="message">Log entry message</param>
        public static void Error(string message)
        {
            LogManager.GetLogger("Default").Error(message);
        }

        /// <summary>
        /// Add log entry for ERROR logging level
        /// </summary>
        /// <param name="message">Log entry message</param>
        /// <param name="exception">Exception which trace is logged</param>
        public static void Error<T>(string message, Exception exception)
        {
            LogManager.GetLogger(typeof(T)).Error(message, exception);
        }

        /// <summary>
        /// Add log entry for ERROR logging level
        /// </summary>
        /// <param name="message">Log entry message</param>
        /// <param name="exception">Exception which trace is logged</param>
        public static void Error(string message, Exception exception)
        {
            LogManager.GetLogger("Default").Error(message, exception);
        }

        #endregion


        #region log FATAL level message

        /// <summary>
        /// Add log entry for FATAL logging level
        /// </summary>
        /// <param name="message">Log entry message</param>
        public static void Fatal<T>(string message)
        {
            LogManager.GetLogger(typeof(T)).Fatal(message);
        }

        /// <summary>
        /// Add log entry for FATAL logging level
        /// </summary>
        /// <param name="message">Log entry message</param>
        public static void Fatal(string message)
        {
            LogManager.GetLogger("Default").Fatal(message);
        }

        /// <summary>
        /// Add log entry for ERROR logging level
        /// </summary>
        /// <param name="message">Log entry message</param>
        /// <param name="exception">Exception which trace is logged</param>
        public static void Fatal<T>(string message, Exception exception)
        {
            LogManager.GetLogger(typeof(T)).Fatal(message, exception);
        }

        /// <summary>
        /// Add log entry for ERROR logging level
        /// </summary>
        /// <param name="message">Log entry message</param>
        /// <param name="exception">Exception which trace is logged</param>
        public static void Fatal(string message, Exception exception)
        {
            LogManager.GetLogger("Default").Fatal(message, exception);
        }

        #endregion

    }
}
