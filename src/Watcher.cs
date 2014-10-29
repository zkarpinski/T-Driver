using System;
using System.Diagnostics;
using System.IO;
using System.Timers;

namespace TDriver {
    public class Watcher {
        private static WorkQueue _dpaWorkQueue;

        /// <summary>
        ///     Create a new watcher for every folder we want to monitor.
        /// </summary>
        /// <param name="sPath">Folder to monitor.</param>
        /// <param name="folderDPAType">DPAType the folder is..</param>
        /// <param name="workQueue">Queue for worked to be added to.</param>
        public Watcher(string sPath, DPAType folderDPAType, ref WorkQueue workQueue) {
            try {
                _dpaWorkQueue = workQueue;
                //Check if the directory exists.
                if (!Directory.Exists(sPath)) {
                    //Form.LogError(sPath + " does not exist!");
                    return;
                }

                // Watch the directory for new word documents.
                var fsw = new FileSystemWatcher(sPath, "*.doc") {
                    NotifyFilter = NotifyFilters.FileName
                };
                fsw.Created += (sender, e) => NewFileCreated(e.FullPath, folderDPAType);
                fsw.EnableRaisingEvents = true;
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
        private static void NewFileCreated(string file, DPAType fileDPAType) {
            Debug.WriteLine("New File detected!");
            //An artifical wait to handle duplicate file creations bug from our web application.
            //BUG check for resource usage for high volume of undisposed timers.
            var aTimer = new Timer(10000) {AutoReset = false};

            aTimer.Elapsed += (sender, e) => _dpaWorkQueue.FoundFileCheck(file, fileDPAType);
            aTimer.Enabled = true;
        }
    }
}