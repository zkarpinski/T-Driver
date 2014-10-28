using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace TDriver {
    public sealed class WorkQueue : IDisposable {
        /// <summary>
        ///     http://social.msdn.microsoft.com/forums/vstudio/en-US/500cb664-e2ca-4d76-88b9-0faab7e7c443/queuing-backgroundworker-tasks
        /// </summary>
        private readonly EventWaitHandle _doQWork = new EventWaitHandle(false, EventResetMode.ManualReset);

        private readonly Queue<Work> _workQueue = new Queue<Work>(50);
        private readonly Object _zLock = new object();

        private Thread _queueWorker;

        private Boolean _quitWork;

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Stops the Queue thread.
        /// </summary>
        public void StopQWorker() {
            _quitWork = true;
            _doQWork.Set();
            _queueWorker.Join(1000);
        }

        /// <summary>
        ///     Starts Queue thread.
        /// </summary>
        public void StartQWorker() {
            _queueWorker = new Thread(QThread) {IsBackground = true};
            _queueWorker.Start();
        }

        /// <summary>
        ///     Adds files to the queue manually.
        /// </summary>
        /// <param name="files">Files to be added to queue.</param>
        /// <param name="workDPAType">User to be faxed out from.</param>
        private void ManualAddition(IEnumerable<string> files, DPAType workDPAType) {
            //Todo Combine with Watcher->NewFileCreated event
            foreach (string file in files) {
                DPA dpa = DPAFactory.Create(file);
                //Add DPA to queue if valid.
                if (dpa.IsValid) {
                    Work work = WorkFactory.Create(dpa, workDPAType);
                    AddToQueue(work);
                }
                else {
                    Debug.WriteLine(dpa.Account + " was skipped.");
                }
            }
        }

        public void AddToQueue(Work work) {
            lock (_zLock) {
                _workQueue.Enqueue(work);
            }
            _doQWork.Set();
        }

        /// <summary>
        ///     Adds all files to the queue from within the settings directories.
        /// </summary>
        public void QueueDirectory(string directoryToQueue, DPAType dpaType) {
            //Queue each DPA from folders defined in the settings.
            if (!Directory.Exists(directoryToQueue)) return; //Skip if the directory doesn't exist
            string[] existingDPAFiles = Directory.GetFiles(directoryToQueue);
            if (existingDPAFiles.Any()) {
                ManualAddition(existingDPAFiles, dpaType);
            }
        }


        /// <summary>
        ///     Background Thread function
        ///     Handles the work queue.
        /// </summary>
        private void QThread() {
            Debug.WriteLine("Thread Started.");

            do {
                //Wait for _doQWork event to start or _quitWork to stop.
                Debug.WriteLine("Waiting for work.");
                _doQWork.WaitOne(-1, false);
                if (_quitWork) {
                    break;
                }

                //Iterate through the work queue.
                Work dequeuedWork;
                do {
                    dequeuedWork = null;
                    //Lock the queue and grab next item.
                    lock (_zLock) {
                        if (_workQueue.Count > 0) {
                            dequeuedWork = _workQueue.Dequeue();
                            Debug.WriteLine(dequeuedWork.GetType() + " Found: " + dequeuedWork.DPAFile);
                        }
                    }

                    //Process if there is work to do.
                    if (dequeuedWork != null) {
                        Debug.WriteLine("Working");
                        if (dequeuedWork.Process()) {
                            dequeuedWork.Move();
                            dequeuedWork.Completed = true;
                            Debug.WriteLine(dequeuedWork.GetType() + " Completed!");
                        }
                        else {
                            Debug.WriteLine(dequeuedWork.GetType() + " Failed!");
                        }
                    }
                } while (dequeuedWork != null);

                //Verify the work queue is complete
                //Reset the _doQWork event if it is.
                lock (_zLock) {
                    if (_workQueue.Count == 0) {
                        _doQWork.Reset();
                    }
                }
            } while (true);
            //End thread and clean up.
            Debug.WriteLine("THREAD ENDED");
            _quitWork = false;
        }

        private void Dispose(bool disposing) {
            if (disposing) {
                // free managed resources
                if (_workQueue != null) {
                    _workQueue.Clear();
                }
            }
        }
    }
}