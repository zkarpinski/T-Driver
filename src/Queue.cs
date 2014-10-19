using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace TDriver {
    public class Queue {
        private static FaxWork EmptyWork;

        /// <summary>
        ///     http://social.msdn.microsoft.com/forums/vstudio/en-US/500cb664-e2ca-4d76-88b9-0faab7e7c443/queuing-backgroundworker-tasks
        /// </summary>
        private readonly EventWaitHandle _doQWork = new EventWaitHandle(false, EventResetMode.ManualReset);

        private readonly Queue<FaxWork> _faxQueue = new Queue<FaxWork>(50);
        private readonly Object _zLock = new object();

        private Thread _queueWorker;

        private Boolean _quitWork;

        /// <summary>
        ///     Adds files to the queue manually.
        /// </summary>
        /// <param name="files">Files to be added to queue.</param>
        /// <param name="workDPAType">User to be faxed out from.</param>
        private void ManualAddition(IEnumerable<string> files, DPAType workDPAType) {
            foreach (var file in files) {
                var fax = new Fax(file);
                //Add fax to queue if valid.
                if (fax.IsValid) {
                    var work = new FaxWork(fax, workDPAType);
                    AddFaxToQueue(work);
                }
                else {
                    Debug.WriteLine(fax.Account + " was skipped.");
                    fax = null;
                }
            }
        }

        /// <summary>
        ///     Adds all files to the queue from within the settings directories.
        /// </summary>
        public void QueueDirectory(string directoryToQueue, DPAType dpaType) {
            //Queue each DPA from folders defined in the settings.
            if (!Directory.Exists(directoryToQueue)) return; //Skip if the directory doesn't exist
            var existingDPAFiles = Directory.GetFiles(directoryToQueue);
            if (existingDPAFiles.Any()) {
                ManualAddition(existingDPAFiles, dpaType);
            }
        }

        public void StopQWorker() {
            _quitWork = true;
            _doQWork.Set();
            _queueWorker.Join(1000);
        }

        public void StartQWorker() {
            _queueWorker = new Thread(QThread) {IsBackground = true};
            _queueWorker.Start();
        }

        public void AddFaxToQueue(FaxWork work) {
            lock (_zLock) {
                _faxQueue.Enqueue(work);
            }
            _doQWork.Set();
        }

        private void QThread() {
            Debug.WriteLine("Thread Started.");
            do {
                Debug.WriteLine("Thread Waiting.");
                _doQWork.WaitOne(-1, false);
                Debug.WriteLine("Checking for work.");
                if (_quitWork) {
                    break;
                }
                FaxWork dequeuedWork;
                do {
                    dequeuedWork = null;
                    Debug.WriteLine("Dequeueing");
                    lock (_zLock) {
                        if (_faxQueue.Count > 0) {
                            dequeuedWork = _faxQueue.Dequeue();
                        }
                    }

                    if (dequeuedWork != null) {
                        Debug.WriteLine("Working");
                        if (dequeuedWork.Process()) {
                            dequeuedWork.Completed = true;
                        }
                        Debug.WriteLine("Work Completed!");
                    }
                } while (dequeuedWork != null);

                lock (_zLock) {
                    if (_faxQueue.Count == 0) {
                        _doQWork.Reset();
                    }
                }
            } while (true);
            Debug.WriteLine("THREAD ENDED");
            _quitWork = false;
        }
    }
}