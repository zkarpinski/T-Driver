using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using RFCOMAPILib;
using Form = System.Windows.Forms.Form;

namespace TDriver {
    public partial class Main : Form {
        private readonly Settings _Settings;
        public List<Fax> Faxes;

        private struct FaxWork {
            public FaxWork(Fax fFax, DPAType faxWorkDPAType) {
                fax = fFax;
                UserId = faxWorkDPAType.UserId;
                Server = faxWorkDPAType.Server;
                MoveLocation = faxWorkDPAType.MoveFolder;
                sDPAType = faxWorkDPAType.Name;
            }
            readonly public Fax fax;
            readonly public string UserId;
            readonly public string Server;
            readonly public string MoveLocation;
            readonly public string sDPAType;
            //Define operator overloads for FaxWork Comparisons.
            public static bool operator == (FaxWork work1, FaxWork work2) {
                   return work1.Equals(work2);
               }
            public static bool operator !=(FaxWork work1, FaxWork work2){
                   return !work1.Equals(work2);
               }
        }

        private static FaxWork EmptyWork;

        public Main() {
            InitializeComponent();
            _Settings = new Settings(Application.StartupPath + "\\Settings.ini");

            // TODO Update Log settings if default
            // TODO Prompt user to close and update ini file.
        }

        /// <summary>
        /// Process a fax by sending and moving it if successful.
        /// </summary>
        /// <param name="work">Work to be performed.</param>
        private void ProcessFax(FaxWork work) {
            if (SendFax(work.fax, work.Server, work.UserId))
            {
               
                try {
                    MoveCompletedFax(work.fax.Document, work.MoveLocation);
                }
                catch (Exception ex) {
                    LogError(ex.Message);
                }

                LogFax(work.fax, work.sDPAType);
            }
        }

        /// <summary>
        ///     Creates a single fax.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="faxInfoType"></param>
        /// <param name="recipient"></param>
        /// <param name="faxNumber"></param>
        /// <returns></returns>
        private Fax CreateFax(string file, FaxInfoType faxInfoType = FaxInfoType.Manual, string recipient = "DEFAULT",
            string faxNumber = "DEFAULT") {
            Fax fax;
            if (faxInfoType == FaxInfoType.Parsed) {
                fax = new Fax(file);
            }
            else if (faxInfoType == FaxInfoType.Manual) {
                fax = new Fax(file, recipient, faxNumber);
            }
            else {
                fax = new Fax();
            }
            return fax;
        }

        private bool SendFax(Fax fax, String userId, String faxServerName) {
            try {
                //Setup Rightfax Server Connection
                var faxsvr = new FaxServer {
                    ServerName = faxServerName,
                    AuthorizationUserID = userId,
                    Protocol = CommunicationProtocolType.cpTCPIP,
                    UseNTAuthentication = BoolType.False
                };
                faxsvr.OpenServer();

                //Create the fax and send.
                if (fax.IsValid) {
                    try {
                        var newFax = (RFCOMAPILib.Fax) faxsvr.get_CreateObject(CreateObjectType.coFax);
                        newFax.ToName = fax.CustomerName;
                        newFax.ToFaxNumber = Regex.Replace(fax.FaxNumber, "-", "");
                        newFax.Attachments.Add(fax.Document);
                        newFax.UserComments = "Sent via SAMuel.";
                        newFax.Send();
                        // TODO newFax.MoveToFolder 
                    }
                    catch (Exception ex) {
                        LogError(ex.Message);
                    }
                }
                else {
                    return false;
                }
                faxsvr.CloseServer();
                return true;
            }

            catch (Exception e) {
                MessageBox.Show(Environment.NewLine + e, "RightFax Error");
                return false;
            }
        }

        private void MoveCompletedFax(string pathToDocument, string folderDestination) {
            string fileName = Path.GetFileName(pathToDocument);
            if (fileName == null) return;
            string saveTo = Path.Combine(folderDestination, fileName);

            //Delete the file in the destination if it exists already.
            // since File.Move does not overwrite.
            if (File.Exists(saveTo)) {
                File.Delete(saveTo);
            }
            File.Move(pathToDocument, saveTo);
        }

        private void LogFax(Fax fax, string userId) {
            string logFile = _Settings.LogFile;
            DateTime logTime = DateTime.Now;

            if (!File.Exists(logFile)) {
                FileStream fs = File.Create(logFile);
                fs.Close();
            }
            string logAction = String.Format("{0} \t{1}: \t{2}\t{3}\t{4} {5}",
                logTime.ToString(CultureInfo.InvariantCulture), userId, fax.Account, fax.FaxNumber,
                fax.CustomerName, Environment.NewLine);
            File.AppendAllText(logFile, logAction);
        }


        /// <summary>
        ///     Logs an error message.
        /// </summary>
        /// <param name="message">Error to be logged.</param>
        private void LogError(string message) {
            string logFile = _Settings.ErrorFile;
            DateTime logTime = DateTime.Now;

            if (!File.Exists(logFile)) {
                FileStream fs = File.Create(logFile);
                fs.Close();
            }
            string logAction = String.Format("{0} \t ERROR: {1} {2}", logTime.ToString(CultureInfo.InvariantCulture),
                message, Environment.NewLine);
            File.AppendAllText(logFile, logAction);
        }

        #region Queue Additions

        /// <summary>
        ///     Adds files to the queue manually.
        /// </summary>
        /// <param name="files">Files to be added to queue.</param>
        /// <param name="workDPAType">User to be faxed out from.</param>
        private void ManualAddition(IEnumerable<string> files, DPAType workDPAType) {
            foreach (string file in files) {
                Fax fax = CreateFax(file, FaxInfoType.Parsed);
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
        private void QueueDirectory() {
            //Queue each DPA from folders defined in the settings.
            foreach (DPAType dpaType in _Settings.WatchList) {
                if (!Directory.Exists(dpaType.WatchFolder)) return; //Skip if the directory doesn't exist
                string[] existingDPAFiles = Directory.GetFiles(dpaType.WatchFolder);
                if (existingDPAFiles.Any()) {
                    ManualAddition(existingDPAFiles, dpaType);
                }
            }
        }

        #endregion

        #region UI Interaction

        private Boolean _isPolling;

        private void btnFax_Click(object sender, EventArgs e) {
            // Stops the watcher if it's currently polling.
            if (_isPolling) {
                Debug.WriteLine("Poll haulted.");
                btnFax.Enabled = false;
                btnFax.Text = "Start Faxing";
                StopQWorker();
                _isPolling = false;
                btnFax.Enabled = true;
            }
            //Start the watcher if it isn't polling.
            else {
                Debug.WriteLine("Poll requested.");
                btnFax.Enabled = false;

                QueueDirectory();

                StartQWorker();

                //Watch each folder from the settings
                foreach (DPAType dpaType in _Settings.WatchList) {
                    WatchFolder(dpaType.WatchFolder,dpaType);
                    //Update UI with folders being watched.
                }
                btnFax.Text = "Stop Faxing";
                _isPolling = true;
                btnFax.Enabled = true;
            }
        }

        #endregion

        #region Folder Watching

        /// <summary>
        ///     Create a new watcher for every folder we want to monitor.
        /// </summary>
        /// <param name="sPath">Folder to monitor.</param>
        /// <param name="folderDPAType">DPAType the folder is..</param>
        private void WatchFolder(string sPath, DPAType folderDPAType) {
            try {
                //Check if the directory exists.
                if (!Directory.Exists(sPath)) {
                    LogError(sPath + " does not exist!");
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

        private void NewFileCreated(object sender, FileSystemEventArgs e, DPAType fileDPAType) {
            Debug.WriteLine("New File detected!");
            string file = e.FullPath;

            //Wait 2 seconds incase the file is being created still.
            Thread.Sleep(2000);

            //Create each fax object from the file.
            Fax fax = CreateFax(file, FaxInfoType.Parsed);

            //Add fax to queue if valid.
            if (fax.IsValid) {
                var work = new FaxWork(fax,fileDPAType);
                AddFaxToQueue(work);
            }
            else {
                Debug.WriteLine(fax.Account + " was skipped.");
                fax = null;
            }
        }

        #endregion

        #region FaxingQueue Backend

        /// <summary>
        ///     http://social.msdn.microsoft.com/forums/vstudio/en-US/500cb664-e2ca-4d76-88b9-0faab7e7c443/queuing-backgroundworker-tasks
        /// </summary>
        private readonly EventWaitHandle _doQWork = new EventWaitHandle(false, EventResetMode.ManualReset);

        private readonly Queue<FaxWork> _faxQueue = new Queue<FaxWork>(50);
        private readonly Object _zLock = new object();


        private Thread _queueWorker;

        private Boolean _quitWork;

        private void StopQWorker() {
            _quitWork = true;
            _doQWork.Set();
            _queueWorker.Join(1000);
        }

        private void StartQWorker() {
            _queueWorker = new Thread(QThread) {IsBackground = true};
            _queueWorker.Start();
        }

        private void AddFaxToQueue(FaxWork work) {
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
                    dequeuedWork = EmptyWork;
                    Debug.WriteLine("Dequeueing");
                    lock (_zLock) {
                        if (_faxQueue.Count > 0) {
                            dequeuedWork = _faxQueue.Dequeue();
                        }
                    }

                    if (dequeuedWork != EmptyWork)
                    {
                        Debug.WriteLine("Working");
                        ProcessFax(dequeuedWork);
                        Debug.WriteLine("Work Completed!");
                    }
                } while (dequeuedWork != EmptyWork);

                lock (_zLock) {
                    if (_faxQueue.Count == 0) {
                        _doQWork.Reset();
                    }
                }
            } while (true);
            Debug.WriteLine("THREAD ENDED");
            _quitWork = false;
        }

        #endregion

        [Flags]
        private enum FaxInfoType {
            Parsed,
            Manual
        };
    }
}