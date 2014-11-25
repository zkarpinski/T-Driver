using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace TDriver {
    public partial class Main : Form {
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

            //Load the settings or generate if it doesn't exist.
            string settingsFile = Application.StartupPath + @"\Settings.ini";
            if (File.Exists(settingsFile)) {
                Settings.Setup(settingsFile);
            }
            else {
                Settings.CreateSettingsTemplate(settingsFile);
            }
        }

        #region UI Interaction

        private Boolean _firstRun = true;

        private void tsBtnStart_Click(object sender, EventArgs e) {
            //Start the watcher
            tbtnStart.Enabled = false;

            // Create the queue and watchlist when its the first time being started.
            if (_firstRun) {
                _dpaWorkQueue = new WorkQueue(Settings.DatabaseFile);
                _dpaWorkQueue.StartQWorker();

                _folderWatchList = new List<Watcher>(Settings.MAX_WATCHLIST_SIZE);
            }

            //Start the DPA Queue Worker
            _dpaWorkQueue.StartQWorker();


            foreach (DPAType dpaType in Settings.WatchList) {
                //Queue Existing Files in the folder
                _dpaWorkQueue.QueueDirectory(dpaType.WatchFolder, dpaType);

                //Setup watcher for the folder.
                if (_firstRun) {
                    _folderWatchList.Add(new Watcher(dpaType.WatchFolder, dpaType, ref _dpaWorkQueue,
                        Settings.FileDelayTime));

                    //TODO Update UI with folders being watched.
                }
            }

            //Start each foloder watcher.
            foreach (Watcher watcher in _folderWatchList) {
                watcher.Start();
            }

            //Update local variables
            _firstRun = false;

            //Update UI
            tbtnStop.Enabled = true;
            tslblStatus.Text = "Running";
            tslblStatus.ForeColor = Color.Green;
        }

        private void tbtnStop_Click(object sender, EventArgs e) {
            tbtnStop.Enabled = false;

            //Stop each folder watcher
            foreach (Watcher watcher in _folderWatchList) {
                watcher.Stop();
            }
            //Stop DPA Queue Worker
            _dpaWorkQueue.StopQWorker();

            //Update UI
            Debug.WriteLine("Poll haulted.");
            tslblStatus.Text = "Stopped";
            tslblStatus.ForeColor = Color.DarkRed;
            tbtnStart.Enabled = true;
        }


        private void frmMain_Resize(object sender, EventArgs e) {
            // Minimize to traybar. (Taskbar is very crowded at work)
            if (FormWindowState.Minimized == WindowState) {
                notifyIcon1.BalloonTipTitle = "T: Driver";
                notifyIcon1.BalloonTipText = "Minimized to traybar.";
                //notifyIcon1.ShowBalloonTip(50);
                //ShowInTaskbar = false;
            }

            else if (FormWindowState.Normal == WindowState) {
                //ShowInTaskbar = true;
            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e) {
            // Show the form when the user double clicks on the notify icon. 
            WindowState = FormWindowState.Normal;

            // Focus the form.
            Activate();
        }

        private void exitMenuItem_Click(object sender, EventArgs e) {
            Close();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e) {
            Form about = new About();
            about.Show();
        }

        #endregion
    }
}