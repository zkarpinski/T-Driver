using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using RFCOMAPILib;

namespace TDriver {
    internal static class Watcher {
        /// <summary>
        ///     Create a new watcher for every folder we want to monitor.
        /// </summary>
        /// <param name="sPath">Folder to monitor.</param>
        /// <param name="folderDPAType">DPAType the folder is..</param>
        static public void WatchFolder(string sPath, DPAType folderDPAType, Queue dpaQueue) {
            try {
                //Check if the directory exists.
                if (!Directory.Exists(sPath)) {
                    //Form.LogError(sPath + " does not exist!");
                    return;
                }

                // Watch the directory for new files.
                var fsw = new FileSystemWatcher(sPath, "*.doc") {
                    NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName
                };
                fsw.Created += (sender, e) => NewFileCreated(sender, e, folderDPAType, dpaQueue);
                fsw.EnableRaisingEvents = true;
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.Message);
            }
        }

        private static void NewFileCreated(object sender, FileSystemEventArgs e, DPAType fileDPAType, Queue dpaQueue) {
            Debug.WriteLine("New File detected!");
            string file = e.FullPath;

            //Wait 2 seconds incase the file is being created still.
            Thread.Sleep(2000);

            //Create each fax object from the file.
            var fax = new Fax(file);

            //Add fax to queue if valid.
            if (fax.IsValid) {
                var work = new FaxWork(fax, fileDPAType);
            dpaQueue.AddFaxToQueue(work);
            }
            else {
                Debug.WriteLine(fax.Account + " was skipped.");
                fax = null;
            }
        }
    }
}