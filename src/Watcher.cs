using System;
using System.Diagnostics;
using System.IO;
using System.Timers;

namespace TDriver {
    public class Watcher {
        private readonly WorkQueue _dpaWorkQueue;
        private readonly int _fileDelay;
        private readonly FileSystemWatcher _watcher;

        /// <summary>
        ///     Create a new watcher for every folder we want to monitor.
        /// </summary>
        /// <param name="sPath">Folder to monitor.</param>
        /// <param name="folderDPAType">DPAType the folder is..</param>
        /// <param name="workQueue">Queue for worked to be added to.</param>
        /// <param name="delay">Delay time in milliseconds, </param>
        public Watcher(string sPath, DPAType folderDPAType, ref WorkQueue workQueue, int delay) {
            try {
                _fileDelay = delay;
                _dpaWorkQueue = workQueue;
                //Check if the directory exists.
                if (!Directory.Exists(sPath)) {
                    //Form.LogError(sPath + " does not exist!");
                    return;
                }

                // Watch the directory for new word documents.
                _watcher = new FileSystemWatcher(sPath, "*.doc") {
                    NotifyFilter = NotifyFilters.FileName
                };
                _watcher.Created += (sender, e) => NewFileCreated(e.FullPath, folderDPAType);
                _watcher.EnableRaisingEvents = true;
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        ///     Wait then handle the newly created file.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="fileDPAType"></param>
        private void NewFileCreated(string file, DPAType fileDPAType) {
            Debug.WriteLine("New File detected!");
            //An artifical wait to handle duplicate file creations bug from our web application.
            // Multiple FileDelay by 1000 to get milliseconds.
            //BUG check for resource usage for high volume of undisposed timers.
            var aTimer = new Timer((double) _fileDelay*1000) {AutoReset = false};

            aTimer.Elapsed += (sender, e) => _dpaWorkQueue.FoundFileCheck(file, fileDPAType);
            aTimer.Enabled = true;
        }

        public void Stop() {
            _watcher.EnableRaisingEvents = false;
        }

        public void Restart() {
            _watcher.EnableRaisingEvents = true;
        }

        public void Dispose() {
            _watcher.Dispose();
        }
    }
}