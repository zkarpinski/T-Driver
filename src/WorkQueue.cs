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

        private readonly Queue<FaxWork> _workQueue = new Queue<FaxWork>(50);
        private readonly Object _zLock = new object();

        private Thread _queueWorker;

        private Boolean _quitWork;

        /// <summary>
        /// Stops the Queue thread.
        /// </summary>
        public void StopQWorker()
        {
            _quitWork = true;
            _doQWork.Set();
            _queueWorker.Join(1000);
        }

        /// <summary>
        /// Starts Queue thread.
        /// </summary>
        public void StartQWorker()
        {
            _queueWorker = new Thread(QThread) { IsBackground = true };
            _queueWorker.Start();
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Adds files to the queue manually.
        /// </summary>
        /// <param name="files">Files to be added to queue.</param>
        /// <param name="workDPAType">User to be faxed out from.</param>
        private void ManualAddition(IEnumerable<string> files, DPAType workDPAType) {
            foreach (string file in files) {
                var fax = new Fax(file);
                //Add fax to queue if valid.
                if (fax.IsValid) {
                    var work = new FaxWork(fax, workDPAType);
                    AddFaxToQueue(work);
                }
                else {
                    Debug.WriteLine(fax.Account + " was skipped.");
                }
            }
        }

        public void AddFaxToQueue(FaxWork work) {
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
        /// Background Thread function
        /// Handles the work queue.
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
                FaxWork dequeuedWork;
                do {
                    dequeuedWork = null;
                    //Lock the queue and grab next item.
                    lock (_zLock) {
                        if (_workQueue.Count > 0) {
                            dequeuedWork = _workQueue.Dequeue();
                            Debug.WriteLine("Work Found: " + dequeuedWork.fax.Document);
                        }
                    }
                    
                    //Process if there is work to do.
                    if (dequeuedWork != null) {
                        Debug.WriteLine("Working");
                        if (dequeuedWork.Process()) {
                            dequeuedWork.Move();
                            dequeuedWork.Completed = true;
                            Debug.WriteLine("Work Completed!");
                        }
                        else {
                            Debug.WriteLine("Work Failed!");
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