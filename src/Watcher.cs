using System;
using System.Diagnostics;
using System.IO;
using System.Timers;
using FileSystemWatcherEx;

namespace TDriver {
    public class Watcher {
        private readonly int _fileDelay;
        private readonly WatcherEx _watcher;
        private readonly WorkQueue _workQueue;

        /// <summary>
        ///     Create a new watcher for every folder we want to monitor.
        /// </summary>
        /// <param name="sPath">Folder to monitor.</param>
        /// <param name="folderSubsection">AP_Subsection the folder is..</param>
        /// <param name="workQueue">Queue for worked to be added to.</param>
        /// <param name="delay">Delay time in milliseconds, </param>
        public Watcher(string sPath, AP_Subsection folderSubsection, ref WorkQueue workQueue, int delay) {
            try {
                _fileDelay = delay;
                _workQueue = workQueue;
                //Check if the directory exists.
                if (!Directory.Exists(sPath)) {
                    Logger.AddError(Settings.ErrorLogfile, sPath + " does not exist!");
                    return;
                }

                // Setup the watcher info.
                WatcherInfo wInfo = new WatcherInfo {
                    ChangesFilters = NotifyFilters.CreationTime |
                                     NotifyFilters.FileName |
                                     NotifyFilters.Size,
                    IncludeSubFolders = false,
                    WatchesFilters = WatcherChangeTypes.Created,
                    WatchForDisposed = true,
                    WatchForError = false,
                    WatchPath = sPath, //Path to Watch
                    BufferKBytes = 8, //Default Buffer
                    MonitorPathInterval = 2*60*1000 //Check folder availability every two minutes.
                };

                // Watch the directory for new word documents.
                _watcher = new WatcherEx(wInfo);
                _watcher.EventCreated +=
                    (sender, e) => NewFileCreated(((FileSystemEventArgs) (e.Arguments)).FullPath, folderSubsection);
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        ///     Wait then handle the newly created file.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="fileSubsection"></param>
        private void NewFileCreated(string file, AP_Subsection fileSubsection) {
            //Skip the file if it's hidden
            //Used to ignore temp files created from Word.
            if ((File.GetAttributes(file) & FileAttributes.Hidden) == FileAttributes.Hidden) {
                return;
            }

            Debug.WriteLine("New File detected!");
            // An artifical wait to handle duplicate file creations bug from our web application.
            // Multiple FileDelay by 1000 to get milliseconds.
            //BUG check for resource usage for high volume of undisposed timers.
            var aTimer = new Timer((double) _fileDelay*1000) {AutoReset = false};

            aTimer.Elapsed += (sender, e) => _workQueue.FoundFileCheck(file, fileSubsection);
            aTimer.Enabled = true;
        }

        public void Stop() {
            _watcher.Stop();
        }

        public void Start() {
            _watcher.Start();
        }

        public void Dispose() {
            _watcher.Dispose();
        }
    }
}