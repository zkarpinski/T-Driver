using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

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
                    NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName
                };
                fsw.Created += (sender, e) => NewFileCreated(sender, e, folderDPAType);
                fsw.EnableRaisingEvents = true;
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.Message);
            }
        }

        private static void NewFileCreated(object sender, FileSystemEventArgs e, DPAType fileDPAType) {
            Debug.WriteLine("New File detected!");
            string file = e.FullPath;

            //Wait 2 seconds incase the file is being created still.
            Thread.Sleep(2000);

            //Create each fax object from the file.
            var fax = new Fax(file);

            //Add fax to queue if valid.
            if (fax.IsValid) {
                var work = new FaxWork(fax, fileDPAType);
                _dpaWorkQueue.AddFaxToQueue(work);
            }
            else {
                Debug.WriteLine(fax.Account + " was skipped.");
                fax = null;
            }
        }
    }
}