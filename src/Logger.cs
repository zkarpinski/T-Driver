using System;
using System.Globalization;
using System.IO;

namespace TDriver {
    internal static class Logger {
        public static int ErrorCount = 0;

        /// <summary>
        ///     Add data to Comma Seperated Values file for data collecting.
        /// </summary>
        /// <param name="csvFile"></param>
        /// <param name="subsection"></param>
        /// <param name="deliveryMethod"></param>
        /// <param name="accountNumber"></param>
        public static void AddToCSV(String csvFile, String subsection, String deliveryMethod, String accountNumber) {
            DateTime logTime = DateTime.Now;

            if (!File.Exists(csvFile)) {
                FileStream fs = File.Create(csvFile);
                fs.Close();
            }
            string logAction = String.Format("{0},{1},{2},{3}{4}",
                logTime.ToString(CultureInfo.InvariantCulture), subsection, deliveryMethod, accountNumber,
                Environment.NewLine);
            File.AppendAllText(csvFile, logAction);
        }

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