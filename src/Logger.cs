using System;
using System.Globalization;
using System.IO;

namespace TDriver {
    internal static class Logger {
        public static int ErrorCount;

        /// <summary>
        ///     Logs an error message.
        /// </summary>
        /// <param name="logFile"></param>
        /// <param name="message">Error to be logged.</param>
        public static void AddError(String logFile, string message) {
            DateTime logTime = DateTime.Now;

            //Increase the error count
            ErrorCount++;

            if (!File.Exists(logFile)) {
                FileStream fs = File.Create(logFile);
                fs.Close();
            }
            string logAction = String.Format("{0} \t ERROR: {1} {2}", logTime.ToString(CultureInfo.InvariantCulture),
                message, Environment.NewLine);
            File.AppendAllText(logFile, logAction);
        }
    }
}