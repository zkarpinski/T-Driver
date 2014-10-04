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


namespace TDriver
{
    
    public partial class Main : Form
    {
        public List<Fax> Faxes;
        private Settings tSettings;
        public Main()
        {
            InitializeComponent();
            tSettings = new Settings(Application.ExecutablePath + "Settings.ini");

            // TODO Update Log settings if default
            // TODO Prompt user to close and update ini file.

        }


        private void ProcessFax(Tuple<Fax, String> work)
        {
            if (SendFax(work.Item1, work.Item2))
            {
                //Determine where to move the files.
                String moveFolder;
                switch (work.Item2)
                {
                    case "active":
                        moveFolder = TDriver.Settings.Default.ActiveMoveLocation;
                        break;
                    case "cutin":
                        moveFolder = TDriver.Settings.Default.CutInMoveLocation;
                        break;
                    default:
                        moveFolder = TDriver.Settings.Default.ActiveMoveLocation;
                        break;
                }
                try
                {
                    MoveCompletedFax(work.Item1.Document, moveFolder);
                }
                catch (Exception ex)
                {
                    LogError(ex.Message);
                }

                LogFax(work.Item1, work.Item2);
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
            string faxNumber = "DEFAULT")
        {
            Fax fax;
            if (faxInfoType == FaxInfoType.Parsed)
            {
                fax = new Fax(file);
            }
            else if (faxInfoType == FaxInfoType.Manual)
            {
                fax = new Fax(file, recipient, faxNumber);
            }
            else
            {
                fax = new Fax();
            }
            return fax;
        }

        private bool SendFax(Fax fax, String userId)
        {
            try
            {
                //Setup Rightfax Server Connection
                var faxsvr = new FaxServer
                {
                    ServerName = TDriver.Settings.Default.FaxServerName,
                    AuthorizationUserID = userId,
                    Protocol = CommunicationProtocolType.cpTCPIP,
                    UseNTAuthentication = BoolType.False
                };
                faxsvr.OpenServer();

                //Create the fax and send.
                if (fax.IsValid)
                {
                    try
                    {
                        var newFax = (RFCOMAPILib.Fax) faxsvr.get_CreateObject(CreateObjectType.coFax);
                        newFax.ToName = fax.CustomerName;
                        newFax.ToFaxNumber = Regex.Replace(fax.FaxNumber, "-", "");
                        newFax.Attachments.Add(fax.Document);
                        newFax.UserComments = "Sent via SAMuel.";
                        newFax.Send();
                        // TODO newFax.MoveToFolder 
                    }
                    catch (Exception ex)
                    {
                        LogError(ex.Message);
                    }
                }
                else
                {
                    return false;
                }
                faxsvr.CloseServer();
                return true;
            }

            catch (Exception e)
            {
                MessageBox.Show(Environment.NewLine + e, "RightFax Error");
                return false;
            }
        }

        private void MoveCompletedFax(string pathToDocument, string folderDestination)
        {
            string fileName = Path.GetFileName(pathToDocument);
            if (fileName == null) return;
            string saveTo = Path.Combine(folderDestination, fileName);

            //Delete the file in the destination if it exists already.
            // since File.Move does not overwrite.
            if (File.Exists(saveTo))
            {
                File.Delete(saveTo);
            }
            File.Move(pathToDocument, saveTo);
        }

        private void LogFax(Fax fax, string userId)
        {
            string logFile = tSettings.LogFile;
            DateTime logTime = DateTime.Now;

            if (!File.Exists(logFile))
            {
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
        private void LogError(string message)
        {
            string logFile = tSettings.ErrorFile;
            DateTime logTime = DateTime.Now;

            if (!File.Exists(logFile))
            {
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
        /// <param name="rightFaxUser">User to be faxed out from.</param>
        private void ManualAddition(IEnumerable<string> files, String rightFaxUser)
        {
            foreach (string file in files)
            {
                Fax fax = CreateFax(file, FaxInfoType.Parsed);
                //Add fax to queue if valid.
                if (fax.IsValid)
                {
                    var work = new Tuple<Fax, string>(fax, rightFaxUser);
                    AddFaxToQueue(work);
                }
                else
                {
                    Debug.WriteLine(fax.Account + " was skipped.");
                    fax = null;
                }
            }
        }

        /// <summary>
        ///     Adds all files to the queue from within the settings directories.
        /// </summary>
        private void QueueDirectory()
        {
            // Define the folders from settings.
            String activeDirectory = TDriver.Settings.Default.ActiveFolder;
            String cutinDirectory = TDriver.Settings.Default.CutinFolder;

            // If the active folder exists, take all files and check if they are valid faxes
            if (!Directory.Exists(activeDirectory)) return;
            string[] activeFiles = Directory.GetFiles(activeDirectory);
            if (activeFiles.Any())
            {
                ManualAddition(activeFiles, "active");
            }
            // If the cutin folder exists, take all files and check if they are valid faxes
            if (!Directory.Exists(cutinDirectory)) return;
            string[] cutinFiles = Directory.GetFiles(cutinDirectory);
            if (cutinFiles.Any())
            {
                ManualAddition(cutinFiles, "active");
            }
        }

        #endregion

        #region UI Interaction

        private Boolean _isPolling;

        /// <summary>
        ///     Polls the desired folder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPoll_Click(object sender, RoutedEventArgs e)
        {
            // Stops the watcher if it's currently polling.
            if (_isPolling)
            {
                Debug.WriteLine("Poll haulted.");
                btnPoll.IsEnabled = false;
                btnPoll.Content = "Start Faxing";
                StopQWorker();
                _isPolling = false;
                btnPoll.IsEnabled = true;
            }
            else //Start the watcher if it isn't polling.
            {
                Debug.WriteLine("Poll requested.");
                btnPoll.IsEnabled = false;

                QueueDirectory();

                StartQWorker();

                WatchFolder(TDriver.Settings.Default.ActiveFolder, "active");
                WatchFolder(TDriver.Settings.Default.CutinFolder, "cutin");
                btnPoll.Content = "Stop Faxing";
                _isPolling = true;
                btnPoll.IsEnabled = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Setup file dialog box
            var dlg = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Word Documents|*.doc;*.docx|All files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Title = "Select the documents you wish to fax.",
                ReadOnlyChecked = true
            };
            dlg.ShowDialog();

            //Stop if no files were selected.
            if (dlg.FileNames.Length <= 0)
            {
                Debug.Print("No files selected to be faxed.");
            }
            string[] files = dlg.FileNames;
            string selectedUser = GetSelectedRightFaxUser();
            ManualAddition(files, selectedUser);
        }

        private void FileDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[]) e.Data.GetData(DataFormats.FileDrop);
                string selectedUser = GetSelectedRightFaxUser();
                ManualAddition(files, selectedUser);
            }
        }

        private string GetSelectedRightFaxUser()
        {
            string selectedUser;
            if ((bool) ActiveUserRatio.IsChecked)
            {
                selectedUser = "active";
            }
            else if ((bool) CutinUserRatio.IsChecked)
            {
                selectedUser = "cutin";
            }
            else
            {
                selectedUser = "active";
            }
            return selectedUser;
        }

        private void FileDrag(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Link;
            }
        }

        /// <summary>
        ///     Closes the form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OptionsItem_Click(object sender, RoutedEventArgs e)
        {
            //var newWindow = new Options();
            //newWindow.Show();
        }

        private void AboutItem_Click(object sender, RoutedEventArgs e)
        {
            var newWindow = new About();
            newWindow.Show();
        }

        #endregion

        #region Folder Watching

        /// <summary>
        ///     Create a new watcher for every folder we want to monitor.
        /// </summary>
        /// <param name="sPath">Folder to monitor.</param>
        /// <param name="rightFaxUser">RightFax user to send faxes out as.</param>
        private void WatchFolder(string sPath, string rightFaxUser)
        {
            try
            {
                //Check if the directory exists.
                if (!Directory.Exists(sPath))
                {
                    LogError(sPath + " does not exist!");
                    return;
                }

                // Watch the directory for new files.
                var fsw = new FileSystemWatcher(sPath, "*.doc")
                {
                    NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName
                };
                fsw.Created += (sender, e) => NewFileCreated(sender, e, rightFaxUser);
                fsw.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void NewFileCreated(object sender, FileSystemEventArgs e, String rightFaxUser)
        {
            Debug.WriteLine("New File detected!");
            string file = e.FullPath;

            //Wait 2 seconds incase the file is being created still.
            Thread.Sleep(2000);

            //Create each fax object from the file.
            Fax fax = CreateFax(file, FaxInfoType.Parsed);

            //Add fax to queue if valid.
            if (fax.IsValid)
            {
                var work = new Tuple<Fax, string>(fax, rightFaxUser);
                AddFaxToQueue(work);
            }
            else
            {
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

        private readonly Queue<Tuple<Fax, String>> _faxQueue = new Queue<Tuple<Fax, String>>(50);
        private readonly Object _zLock = new object();


        private Thread _queueWorker;

        private Boolean _quitWork;

        private void StopQWorker()
        {
            _quitWork = true;
            _doQWork.Set();
            _queueWorker.Join(1000);
        }

        private void StartQWorker()
        {
            _queueWorker = new Thread(QThread) {IsBackground = true};
            _queueWorker.Start();
        }

        private void AddFaxToQueue(Tuple<Fax, String> work)
        {
            lock (_zLock)
            {
                _faxQueue.Enqueue(work);
            }
            _doQWork.Set();
        }

        private void QThread()
        {
            Debug.WriteLine("Thread Started.");
            do
            {
                Debug.WriteLine("Thread Waiting.");
                _doQWork.WaitOne(-1, false);
                Debug.WriteLine("Checking for work.");
                if (_quitWork)
                {
                    break;
                }
                Tuple<Fax, String> dequeuedWork;
                do
                {
                    dequeuedWork = null;
                    Debug.WriteLine("Dequeueing");
                    lock (_zLock)
                    {
                        if (_faxQueue.Count > 0)
                        {
                            dequeuedWork = _faxQueue.Dequeue();
                        }
                    }

                    if (dequeuedWork != null)
                    {
                        Debug.WriteLine("Working");
                        ProcessFax(dequeuedWork);
                        Debug.WriteLine("Work Completed!");
                    }
                } while (dequeuedWork != null);

                lock (_zLock)
                {
                    if (_faxQueue.Count == 0)
                    {
                        _doQWork.Reset();
                    }
                }
            } while (true);
            Debug.WriteLine("THREAD ENDED");
            _quitWork = false;
        }

        #endregion

        [Flags]
        private enum FaxInfoType
        {
            Parsed,
            Manual
        };
    }
}