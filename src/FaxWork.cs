using System;
using System.Diagnostics;

#if !DEBUG
using RFCOMAPILib;

#endif

namespace TDriver {
    public class FaxWork : Work {
        private readonly string _comment;
        private readonly AP_Document _fax;
        private readonly string _server;
        private readonly string _userId;

        public FaxWork(string moveLocation, string origDocument, string faxNumber, string recipient, string fileToSend,
            AP_Document fax, AP_Subsection subsection) : base(moveLocation, origDocument) {
            _fax = fax;
            _userId = subsection.UserId;
            _server = subsection.Server;
            _comment = subsection.FaxComment;
            this.Recipient = recipient;
            this.SubSection = subsection.Name;
        }

        public FaxWork(AP_Document fax, AP_Subsection subsection) : base(subsection.MoveFolder, fax.Document) {
            _fax = fax;
            _userId = subsection.UserId;
            _server = subsection.Server;
            _comment = subsection.FaxComment;
            this.Recipient = fax.SendTo;
            this.SubSection = subsection.Name;
        }

        public override AP_Document DocObject => _fax;

        public string FaxNumber {
            get { return _fax.SendTo.Replace("-", String.Empty); }
        }

        public string Recipient { get; set; }

        public string Attachment {
            get { return _fax.FileToSend; }
        }

        public override Boolean Process() {
            //Verify the fax meets critera.
            //TODO Add a Validate function to Fax class
            if (!_fax.IsValid) return false;

#if DEBUG //Allow simulating a successful/failed fax, outside of production system.
            //Debug result :: Faxing Success.
            _fax.AddSentTime();
            Debug.WriteLine(String.Format("Faxed {0} to {1} for {2} with account {3} using Server:{4}, User:{5}.",
                Attachment, FaxNumber, Recipient, _fax.Account, _server, _userId));
            return true;

#else //Release:: Fax Process
            try {
                //Setup Rightfax Server Connection
                FaxServer faxsvr = SetupRightFaxServer();
                faxsvr.OpenServer();

                //Create the fax and send.
                RFCOMAPILib.Fax newFax = CreateRightFax_Fax(faxsvr);
                newFax.Send();
                _fax.AddSentTime();

                // TODO move the fax, within RightFax, to the sent folder.
                // newFax.MoveToFolder

                faxsvr.CloseServer();
                return true;
            }

            catch (Exception e) {
                Logger.AddError(Settings.ErrorLogfile, e.Message);
                return false;
            }
#endif
        }


#if !DEBUG //Directive used here so application can be debugged and tested on a machine WITHOUT the RFCOMAPI.dll

    /// <summary>
    ///     Setup RightFax server connection.
    /// </summary>
    /// <returns></returns>
        private FaxServer SetupRightFaxServer() {
            var faxsvr = new FaxServer {
                ServerName = _server,
                AuthorizationUserID = _userId,
                Protocol = CommunicationProtocolType.cpTCPIP,
                UseNTAuthentication = BoolType.False
            };
            return faxsvr;
        }

        /// <summary>
        ///     Create the fax on the RightFax server.
        /// </summary>
        /// <param name="faxsvr"></param>
        /// <returns></returns>
        private RFCOMAPILib.Fax CreateRightFax_Fax(FaxServer faxsvr) {
            var newFax = (RFCOMAPILib.Fax) faxsvr.CreateObject[CreateObjectType.coFax];
            newFax.ToName = this.Recipient;
            newFax.ToFaxNumber = this.FaxNumber;
            newFax.Attachments.Add(this.Attachment);
            newFax.UserComments = _comment;
            return newFax;
        }
#endif
    }
}