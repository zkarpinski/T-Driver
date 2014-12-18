using System;
using System.IO;
using System.Threading;
using CDO;
using Microsoft.Office.Interop.Word;

namespace TDriver {
    public class EmailWork : Work {
        private readonly string _eMsg;
        private readonly Email _email;
        private readonly string _sendAs;

        public EmailWork(Email eEmail, ref DPAType typeOfDPA) {
            _email = eEmail;
            _sendAs = typeOfDPA.SendEmailFrom;
            MoveLocation = typeOfDPA.MoveFolder;
            KindOfDPA = typeOfDPA.Name;
            _sendAs = typeOfDPA.SendEmailFrom;
            _eMsg = Settings.EmailMsg;
        }

        public override DPA DPAObject {
            get { return _email; }
        }

        public override bool Process() {
#if DEBUG //Allow simulating
    //Debug result :: Email Success.
    // _email.AddSentTime();
    //return false;
            
            if (!CreatePdfToSend()) return false;
            //Send email
            return SendEmail();


#else
            //Release: Does NOT processs
            //Todo: Get working consistantly
            return false;

#endif
        }

        private bool CreatePdfToSend() {
            const string pdfPrinterName = "PDF995";

            try {
                var wApp = new Application {Visible = false, DisplayAlerts = WdAlertLevel.wdAlertsNone};
                //Store the old printer name and change the active to the PDF printer name
                string prevPrinter = wApp.ActivePrinter;
                wApp.ActivePrinter = pdfPrinterName;

                //Open the DPA Document with options:
                //  Don't confirm conversion from RTF to doc
                //  Open as readonly
                //  Don't add to recent list
                _Document oDoc = wApp.Documents.Open(_email.Document, false, true, false); //readonly,
                oDoc.PrintOut(false); //Printout NOT async(background)

                //Changes the default printer back
                wApp.ActivePrinter = prevPrinter;

                //Wait until PDF file is made.
                while (!File.Exists(_email.FileToSend)) {
                    Thread.Sleep(2000);
                }

                //Close document and word application
                oDoc.Close();
                wApp.Quit();

                //Check the file size
                long length = new FileInfo(_email.FileToSend).Length;
                if (length < 29000) {
                    Thread.Sleep(5000);
                    length = new FileInfo(_email.FileToSend).Length;
                    if (length < 29000) {
                        Logger.AddError(Settings.ErrorLogfile, _email.FileName + "file size error. Did NOT send.");
                    }
                    return false;
                }

                return true;
            }
            catch (Exception ex) {
                Logger.AddError(Settings.ErrorLogfile, ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Creates and sends an email using the data from email dpa-type field.
        /// </summary>
        /// <returns>Success or Fail.</returns>
        private bool SendEmail() {
            //Define the connection settings.
            short cdoSendUsingPort = Settings.EmailPort;
            const short smtpConnectionTimeout = 10;
            string smtpServer = Settings.SmtpServer;

            try {
                var eMessage = new Message();

                //Setup the connection settings.
                Configuration iConfg = eMessage.Configuration;
                iConfg.Fields["http://schemas.microsoft.com/cdo/configuration/sendusing"].Value = cdoSendUsingPort;
                iConfg.Fields["http://schemas.microsoft.com/cdo/configuration/smtpserver"].Value = smtpServer;
                iConfg.Fields["http://schemas.microsoft.com/cdo/configuration/smtpconnectiontimeout"].Value =
                    smtpConnectionTimeout;
                iConfg.Fields.Update();

                //Setup the email and send.
                eMessage.Configuration = iConfg;
                eMessage.To = _email.EmailAddress;
                eMessage.From = _sendAs;
                eMessage.Subject = _email.Account + " Deferred Payment Agreement";
                eMessage.HTMLBody = _eMsg;
                eMessage.AddAttachment(_email.FileToSend, "", "");
                eMessage.Send();

                //Add the time the email was sent and report Success.
                _email.AddSentTime();
                return true;
            }
            catch (Exception ex) {
                //Log the error and report Failure.
                Logger.AddError(Settings.ErrorLogfile, ex.Message);
                return false;
            }
        }
    }
}