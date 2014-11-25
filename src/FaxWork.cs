using System;
#if !DEBUG
using RFCOMAPILib;

#endif

namespace TDriver {
    public class FaxWork : Work {
        private readonly string _comment;
        private readonly Fax _fax;
        private readonly string _server;
        private readonly string _userId;

        public FaxWork(Fax fFax, ref DPAType typeOfDPA) {
            _fax = fFax;
            _userId = typeOfDPA.UserId;
            _server = typeOfDPA.Server;
            _comment = typeOfDPA.FaxComment;
            MoveLocation = typeOfDPA.MoveFolder;
            KindOfDPA = typeOfDPA.Name;
        }

        public override DPA DPAObject {
            get { return _fax; }
        }

        public override Boolean Process() {
#if DEBUG //Allow simulating faxing, outside of production system.
    //Debug result :: Faxing Success.
            _fax.AddSentTime();
            return true;
#else
            //Release:: Fax Process
            try {
                //Setup Rightfax Server Connection
                FaxServer faxsvr = SetupRightFaxServer();
                faxsvr.OpenServer();

                //Create the fax and send.
                if (_fax.IsValid) {
                    RFCOMAPILib.Fax newFax = CreateRightFax_Fax(faxsvr);
                    newFax.Send();
                    _fax.AddSentTime();
                    // TODO move the fax, within RightFax, to the sent folder.
                    // newFax.MoveToFolder
                }

                else {
                    return false;
                }
                faxsvr.CloseServer();
                return true;
            }

            catch (Exception e) {
                Logger.AddError(Settings.ErrorLogfile, e.Message);
                return false;
            }
#endif
        }

#if !DEBUG
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
            newFax.ToName = DPAObject.CustomerName;
            newFax.ToFaxNumber = "1" + _fax.FaxNumber.Replace("-", ""); //Add US Code (1)
            newFax.Attachments.Add(DPAObject.Document);
            newFax.UserComments = _comment;
            return newFax;
        }
#endif
    }
}