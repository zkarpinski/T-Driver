using System;
using System.Text.RegularExpressions;
using RFCOMAPILib;

namespace TDriver {
    public class FaxWork : Work {
        private readonly string _comment;
        private readonly string _server;
        private readonly string _userId;
        public readonly Fax fax;


        public FaxWork(Fax fFax, DPAType typeOfDPA) {
            fax = fFax;
            _userId = typeOfDPA.UserId;
            _server = typeOfDPA.Server;
            _comment = typeOfDPA.FaxComment;
            MoveLocation = typeOfDPA.MoveFolder;
            KindOfDPA = typeOfDPA.Name;
            DPAFile = fFax.Document;
        }

        public override Boolean Process() {
#if DEBUG
    //Debug result :: Faxing Success.
            return true;
#else
            //Release:: Fax Process
            try {
                //Setup Rightfax Server Connection
                FaxServer faxsvr = SetupRightFaxServer();
                faxsvr.OpenServer();

                //Create the fax and send.
                if (fax.IsValid) {
                    RFCOMAPILib.Fax newFax = CreateRightFax_Fax(faxsvr);
                    newFax.Send();
                    // TODO move the fax, within RighFax, to the sent folder.
                    // newFax.MoveToFolder
                }

                else {
                    return false;
                }
                faxsvr.CloseServer();
                return true;
            }

            catch (Exception e) {
                return false;
            }
#endif
        }

        /// <summary>
        /// Setup RightFax server connection.
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
        /// Create the fax on the RightFax server.
        /// </summary>
        /// <param name="faxsvr"></param>
        /// <returns></returns>
        private RFCOMAPILib.Fax CreateRightFax_Fax(FaxServer faxsvr) {
            var newFax = (RFCOMAPILib.Fax) faxsvr.CreateObject[CreateObjectType.coFax];
            newFax.ToName = fax.CustomerName;
            newFax.ToFaxNumber = "1" + Regex.Replace(fax.FaxNumber, "-", ""); //Add US Code (1)
            newFax.Attachments.Add(fax.Document);
            newFax.UserComments = _comment;
            return newFax;
        }
    }
}