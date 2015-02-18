using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace TDriver {
    public partial class Main : Form {
        private WorkQueue _docWorkQueue;
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

            //Clear example data from UI.
            listFoldersWatched.Items.Clear();
            listFoldersWatched.Size = new Size(this.Width - 16, this.Height - 110);

            //Load the settings or generate if it doesn't exist.
            string settingsFile = Application.StartupPath + @"\Settings.ini";
            if (File.Exists(settingsFile)) {
                Settings.Setup(settingsFile);
                SetupWatchFolderListview();
            }
            else {
                Settings.CreateSettingsTemplate(settingsFile);
            }
        }

        /// <summary>
        ///     Fills and resizes the WatchFolderListview
        /// </summary>
        private void SetupWatchFolderListview() {
            Settings.WatchList.ForEach(delegate(AP_Subsection subSection) {
                // ListView Column Order: Server, UserID, WatchFolder, MoveFolder
                ListViewItem lvi = new ListViewItem(subSection.Name) {
                    SubItems = {subSection.Server, subSection.UserId, subSection.WatchFolder, subSection.MoveFolder,},
                    Tag = subSection
                };
                if (subSection.IsValid == false) {
                    lvi.ForeColor = Color.Red;
                }

                listFoldersWatched.Items.Add(lvi);
            });

            //Resize the columns
            listFoldersWatched.Columns[0].Width = -2; // Name
            listFoldersWatched.Columns[1].Width = -2; // Server
            listFoldersWatched.Columns[2].Width = -2; // User Id
            listFoldersWatched.Columns[3].Width = -2; // Watch Folder
            listFoldersWatched.Columns[4].Width = -2; // Move Folder
        }

        #region UI Interaction

        private Boolean _firstRun = true;

        private void tsBtnStart_Click(object sender, EventArgs e) {
            tbtnStart.Enabled = false;
            

            // Create the queue and watcher list the first time started it'S.
            if (_firstRun) {
                _folderWatchList = new List<Watcher>(Settings.MAX_WATCHLIST_SIZE);
                _docWorkQueue = new WorkQueue(Settings.DatabaseFile);
                
            }

            //Start the DPA Queue Worker
            _docWorkQueue.StartQWorker();

            //Queue Directories and create the watchers
            foreach (AP_Subsection subsection in Settings.WatchList) {
                if (subsection.IsValid && Directory.Exists(subsection.WatchFolder)) {
                    //Queue Existing Files in the folder
                    _docWorkQueue.QueueDirectory(subsection.WatchFolder, subsection);

                    //Setup watcher for the folder.
                    _folderWatchList.Add(new Watcher(subsection.WatchFolder, subsection, ref _docWorkQueue, Settings.FileDelayTime));
                    }
            }

            //Start each folder watcher.
            if (_folderWatchList.Any()) {
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
            else {
                _docWorkQueue.StopQWorker();
                MessageBox.Show("No valid folder watchers. Please check settings and try again.");
                tbtnStart.Enabled = true;
            }
        }

        private void tbtnStop_Click(object sender, EventArgs e) {
            tbtnStop.Enabled = false;

            //Stop each folder watcher and clear list.
            foreach (Watcher watcher in _folderWatchList) {
                watcher.Stop();
            }
            _folderWatchList.Clear();

            //Stop DPA Queue Worker
            _docWorkQueue.StopQWorker();

            //Update UI
            Debug.WriteLine("Watchers halted.");
            tslblStatus.Text = "Stopped";
            tslblStatus.ForeColor = Color.DarkRed;
            tbtnStart.Enabled = true;
        }


        private void frmMain_Resize(object sender, EventArgs e) {
            // Minimize to tray bar. (Task-bar is very crowded at work)
            if (FormWindowState.Minimized == WindowState) {
                notifyIcon1.BalloonTipTitle = "T: Driver";
                notifyIcon1.BalloonTipText = "Minimized to tray bar.";
                //notifyIcon1.ShowBalloonTip(50);
                //ShowInTaskbar = false;
            }
            // Resize the list view
            else if (FormWindowState.Normal == WindowState) {
                listFoldersWatched.Size = new Size(this.Width - 16, this.Height - 110);
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

        private void MainFormClosing(object sender, FormClosingEventArgs e) {
            if (_folderWatchList  != null) {
                //Stop each folder watcher and clear list.
                foreach (Watcher watcher in _folderWatchList) {
                    watcher.Stop();
                }
                _folderWatchList.Clear();
            }

            //Dispose work queue and queue worker.
            if (_docWorkQueue != null) {
                if (_docWorkQueue.IsRunning)
                    _docWorkQueue.StopQWorker();
                _docWorkQueue.Dispose();
            }
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e) {
            Process.Start("mailto:Zachary.Karpinski@nationalgrid.com?subject=TDriver");
        }
    }
}