using System;
using System.Diagnostics;
using System.IO;
using Timer = System.Timers.Timer;

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

                // Watch the directory for new files.
                var fsw = new FileSystemWatcher(sPath, "*.doc") {
                    NotifyFilter = NotifyFilters.FileName
                };
                fsw.Created += (sender, e) => ArtificalWait(e, folderDPAType);
                fsw.EnableRaisingEvents = true;
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// An artifical wait to handle duplicate file creations bug from our web application.
        /// </summary>
        /// <param name="fsEventArgs"></param>
        /// <param name="fileDPAType"></param>
        private static void ArtificalWait(FileSystemEventArgs fsEventArgs, DPAType fileDPAType) {
            var aTimer = new Timer(10000) {AutoReset = false};

            aTimer.Elapsed += (sender, e) => NewFileCreated(fsEventArgs, fileDPAType);
            aTimer.Enabled = true;

        }


        private static void NewFileCreated(FileSystemEventArgs e, DPAType fileDPAType) {

            Debug.WriteLine("New File detected!");
            string file = e.FullPath;

            //Create dpa from factory
            DPA dpa = DPAFactory.Create(file);

            //Create a work request and add to queue.
            if (dpa.IsValid) {
                var work = new FaxWork((Fax)dpa, fileDPAType);
                _dpaWorkQueue.AddFaxToQueue(work);
            }
            else {
                Debug.WriteLine(dpa.Account + " was skipped.");
                dpa = null;
            }
        }
    }
}