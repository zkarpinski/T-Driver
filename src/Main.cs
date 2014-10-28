using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace TDriver {
    public partial class Main : Form {
        private readonly Settings _settings;

        private WorkQueue _dpaWorkQueue;
        private List<Watcher> _folderWatchList;

        public Main() {
            //Loads embedded dlls
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => {
                string resourceName = new AssemblyName(args.Name).Name + ".dll";
                string resource = Array.Find(GetType().Assembly.GetManifestResourceNames(),
                    element => element.EndsWith(resourceName));
                if (resource == null) return null;

                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource)) {
                    if (stream != null) {
                        var assemblyData = new Byte[stream.Length];
                        stream.Read(assemblyData, 0, assemblyData.Length);
                        return Assembly.Load(assemblyData);
                    }
                    return null;
                }
            };

            InitializeComponent();
            _settings = new Settings(Application.StartupPath + "\\Settings.ini");
            // TODO Update Log settings if default
            // TODO Prompt user to close and update ini file.
        }

        #region UI Interaction

        private Boolean _isPolling;

        private void tsBtnStart_Click(object sender, EventArgs e) {
            //Start the watcher
            tbtnStart.Enabled = false;
            _dpaWorkQueue = new WorkQueue();
            _folderWatchList = new List<Watcher>(Settings.MaxWatchlistSize);
            Debug.WriteLine("Poll requested.");

            _dpaWorkQueue.StartQWorker();

            foreach (DPAType dpaType in _settings.WatchList) {
                //Queue Existing Files in the folder
                _dpaWorkQueue.QueueDirectory(dpaType.WatchFolder, dpaType);
                //Setup watcher for the folder.
                _folderWatchList.Add(new Watcher(dpaType.WatchFolder, dpaType, ref _dpaWorkQueue));
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
                _dpaWorkQueue.StopQWorker();
                _isPolling = false;
                _dpaWorkQueue = null;
                Debug.WriteLine("Poll haulted.");
                tslblStatus.Text = "Stopped";
                tslblStatus.ForeColor = Color.DarkRed;
                tbtnStart.Enabled = true;
            }
        }

        /// <summary>
        ///     Handles minimize to traybar. (Taskbar is very crowded at work)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmMain_Resize(object sender, EventArgs e) {
            if (FormWindowState.Minimized == WindowState) {
                notifyIcon1.BalloonTipTitle = "T: Driver";
                notifyIcon1.BalloonTipText = "Minimized to traybar.";
                notifyIcon1.ShowBalloonTip(50);
                ShowInTaskbar = false;
                Hide();
            }

            else if (FormWindowState.Normal == WindowState) {
                ShowInTaskbar = true;
            }
        }

        private void notifyIcon1_Click(object sender, EventArgs e) {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void exitMenuItem_Click(object sender, EventArgs e) {
            Close();
        }

        #endregion
    }
}