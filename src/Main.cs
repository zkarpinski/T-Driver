using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using RFCOMAPILib;
using Form = System.Windows.Forms.Form;

namespace TDriver {
    [Flags]
    public enum FaxInfoType {
        Parsed,
        Manual
    };

  
    public partial class Main : Form {
        private readonly Settings _settings;
        public Queue DPAQueue;

        public Main() {
            InitializeComponent();
            _settings = new Settings(Application.StartupPath + "\\Settings.ini");
            // TODO Update Log settings if default
            // TODO Prompt user to close and update ini file.
        }

        /// <summary>
        ///     Process a fax by sending and moving it if successful.
        /// </summary>
        /// <param name="work">Work to be performed.</param>
        private void ProcessFax(FaxWork work) {
            if (SendFax(work.fax, work.Server, work.UserId)) {
                try {
                    MoveCompletedFax(work.fax.Document, work.MoveLocation);
                }
                catch (Exception ex) {
                    LogError(ex.Message);
                }

                LogFax(work.fax, work.KindOfDPA);
            }
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
            string logFile = _settings.LogFile;
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
        public void LogError(string message) {
            string logFile = _settings.ErrorFile;
            DateTime logTime = DateTime.Now;

            if (!File.Exists(logFile)) {
                FileStream fs = File.Create(logFile);
                fs.Close();
            }
            string logAction = String.Format("{0} \t ERROR: {1} {2}", logTime.ToString(CultureInfo.InvariantCulture),
                message, Environment.NewLine);
            File.AppendAllText(logFile, logAction);
        }



        #region UI Interaction

        private Boolean _isPolling;

        private void btnFax_Click(object sender, EventArgs e) {
            // Stops the watcher if it's currently polling.
            if (_isPolling) {
                Debug.WriteLine("Poll haulted.");
                btnFax.Enabled = false;
                btnFax.Text = "Start Faxing";
                DPAQueue.StopQWorker();
                _isPolling = false;
                btnFax.Enabled = true;
                DPAQueue = null;
            }
                //Start the watcher if it isn't polling.
            else {
                DPAQueue = new Queue();
                Debug.WriteLine("Poll requested.");
                btnFax.Enabled = false;

                DPAQueue.StartQWorker();

                foreach (DPAType dpaType in _settings.WatchList) {
                    //Queue Existing Files in the folder
                    DPAQueue.QueueDirectory(dpaType.WatchFolder, dpaType);
                    //Setup watcher for the folder.
                    Watcher.WatchFolder(dpaType.WatchFolder, dpaType,DPAQueue);
                    //TODO Update UI with folders being watched.
                }
                btnFax.Text = "Stop Faxing";
                _isPolling = true;
                btnFax.Enabled = true;
            }
        }

        #endregion
    }
}