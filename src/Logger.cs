using System;
using System.Globalization;
using System.IO;

namespace TDriver {
    internal class Logger {
        /// <summary>
        ///     Add data to Comma Seperated Values file for data collecting.
        /// </summary>
        /// <param name="csvFile"></param>
        /// <param name="dpaType"></param>
        /// <param name="deliveryMethod"></param>
        /// <param name="accountNumber"></param>
        public void AddToCSV(String csvFile, String dpaType, String deliveryMethod, String accountNumber) {
            DateTime logTime = DateTime.Now;

            if (!File.Exists(csvFile)) {
                FileStream fs = File.Create(csvFile);
                fs.Close();
            }
            string logAction = String.Format("{0},{1},{2},{3}{4}",
                logTime.ToString(CultureInfo.InvariantCulture), dpaType, deliveryMethod, accountNumber,
                Environment.NewLine);
            File.AppendAllText(csvFile, logAction);
        }


        public void LogFax(String logFile, ref Fax fax, string userId) {
            DateTime logTime = DateTime.Now;

            if (!File.Exists(logFile)) {
                FileStream fs = File.Create(logFile);
                fs.Close();
            }
            string logAction = String.Format("{0} \t{1}: \t{2}\t{3}\t{4} {5}",
                logTime.ToString(CultureInfo.InvariantCulture), userId, fax.Account, fax.FaxNumber,
                fax.CustomerName, Environment.NewLine);
            File.AppendAllText(logFile, logAction);
        }


        /// <summary>
        ///     Logs an error message.
        /// </summary>
        /// <param name="message">Error to be logged.</param>
        public void LogError(String logFile, string message) {
            DateTime logTime = DateTime.Now;

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