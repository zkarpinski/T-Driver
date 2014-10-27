using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

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
            //Loads embedded dlls
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => {
                string resourceName = new AssemblyName(args.Name).Name + ".dll";
                string resource = Array.Find(GetType().Assembly.GetManifestResourceNames(),
                    element => element.EndsWith(resourceName));
                if (resource == null) return null;

                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource)) {
                    var assemblyData = new Byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            };
            InitializeComponent();
            _settings = new Settings(Application.StartupPath + "\\Settings.ini");
            // TODO Update Log settings if default
            // TODO Prompt user to close and update ini file.
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

        private void tsBtnStart_Click(object sender, EventArgs e) {
            //Start the watcher
            tbtnStart.Enabled = false;
            DPAWorkQueue = new WorkQueue();
            _folderWatchList = new List<Watcher>(Settings.MaxWatchlistSize);
            Debug.WriteLine("Poll requested.");

            DPAWorkQueue.StartQWorker();

            foreach (DPAType dpaType in _settings.WatchList) {
                //Queue Existing Files in the folder
                DPAWorkQueue.QueueDirectory(dpaType.WatchFolder, dpaType);
                //Setup watcher for the folder.
                _folderWatchList.Add(new Watcher(dpaType.WatchFolder, dpaType, ref DPAWorkQueue));
                //TODO Update UI with folders being watched.
            }
            tbtnStop.Enabled = true;
            tslblStatus.Text = "Running";
            tslblStatus.ForeColor = Color.Green;
            _isPolling = true;
        }

        private void tbtnStop_Click(object sender, EventArgs e) {
            // Stops the watchers.
            if (_isPolling) {
                tbtnStop.Enabled = false;
                Debug.WriteLine("Poll haulted.");
                tslblStatus.Text = "Stopped";
                tslblStatus.ForeColor = Color.DarkRed;
                DPAWorkQueue.StopQWorker();
                _isPolling = false;
                DPAWorkQueue = null;
                tbtnStart.Enabled = true;
            }
        }

        private void exitMenuItem_Click(object sender, EventArgs e) {
            Close();
        }

        #endregion
    }
}