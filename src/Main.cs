using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Form = System.Windows.Forms.Form;
namespace TDriver {
    [Flags]
    public enum FaxInfoType {
        Parsed,
        Manual
    };

    public partial class Main : Form {
        private readonly Settings _settings;

        public WorkQueue DPAWorkQueue;
        private List<Watcher> _folderWatchList;

        public Main() {
            InitializeComponent();
            _settings = new Settings(Application.StartupPath + "\\Settings.ini");
            // TODO Update Log settings if default
            // TODO Prompt user to close and update ini file.
        }

        private void MoveCompletedFax(string pathToDocument, string folderDestination) {
            string fileName = Path.GetFileName(pathToDocument);
            if (fileName == null) return;
            string saveTo = Path.Combine(folderDestination, fileName);

            //Delete the file in the destination if it exists already.
            // since File.Move does not overwrite.
            if (File.Exists(saveTo)) {
                File.Delete(saveTo);
            }
            File.Move(pathToDocument, saveTo);
        }

        private void LogFax(Fax fax, string userId) {
            string logFile = _settings.LogFile;
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
        public void LogError(string message) {
            string logFile = _settings.ErrorFile;
            DateTime logTime = DateTime.Now;

            if (!File.Exists(logFile)) {
                FileStream fs = File.Create(logFile);
                fs.Close();
            }
            string logAction = String.Format("{0} \t ERROR: {1} {2}", logTime.ToString(CultureInfo.InvariantCulture),
                message, Environment.NewLine);
            File.AppendAllText(logFile, logAction);
        }



        #region UI Interaction

        private Boolean _isPolling;

        private void tsBtnStart_Click(object sender, EventArgs e)
        {
            //Start the watcher
                tbtnStart.Enabled = false;
                DPAWorkQueue = new WorkQueue();
                _folderWatchList = new List<Watcher>(Settings.MaxWatchlistSize);
                Debug.WriteLine("Poll requested.");

                DPAWorkQueue.StartQWorker();

                foreach (DPAType dpaType in _settings.WatchList)
                {
                    //Queue Existing Files in the folder
                    DPAWorkQueue.QueueDirectory(dpaType.WatchFolder, dpaType);
                    //Setup watcher for the folder.
                    _folderWatchList.Add(new Watcher(dpaType.WatchFolder, dpaType, ref DPAWorkQueue));
                    //TODO Update UI with folders being watched.
                }
                tbtnStop.Enabled = true;
                tslblStatus.Text = "Running";
                tslblStatus.ForeColor = System.Drawing.Color.Green;
                _isPolling = true;
            
        }

        private void tbtnStop_Click(object sender, EventArgs e)
        {
            // Stops the watchers.
            if (_isPolling)
            {
                tbtnStop.Enabled = false;
                Debug.WriteLine("Poll haulted.");
                tslblStatus.Text = "Stopped";
                tslblStatus.ForeColor = System.Drawing.Color.DarkRed;
                DPAWorkQueue.StopQWorker();
                _isPolling = false;
                DPAWorkQueue = null;
                tbtnStart.Enabled = true;
            }
        }

        #endregion


    }
}