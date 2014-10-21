using System;
using System.Text.RegularExpressions;
using RFCOMAPILib;

namespace TDriver {
    public class FaxWork : Work {
        private readonly string Server;
        private readonly string UserId;
        public readonly Fax fax;


        public FaxWork(Fax fFax, DPAType faxWorkDPAType) {
            fax = fFax;
            UserId = faxWorkDPAType.UserId;
            Server = faxWorkDPAType.Server;
            MoveLocation = faxWorkDPAType.MoveFolder;
            KindOfDPA = faxWorkDPAType.Name;
            DPAFile = fFax.Document;
        }

        public override Boolean Process()
        {
#if DEBUG
            //Debug result :: Faxing Success.
            return true;
#else
            //Release Fax Process
            try {
                //Setup Rightfax Server Connection
                FaxServer faxsvr = SetupRightFaxServer();
                faxsvr.OpenServer();

                //Create the fax and send.
                if (fax.IsValid) {
                    RFCOMAPILib.Fax newFax = CreateRightFax_Fax(faxsvr);
                    newFax.Send();
                    // TODO newFax.MoveToFolder 
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

        private FaxServer SetupRightFaxServer() {
            var faxsvr = new FaxServer {
                ServerName = Server,
                AuthorizationUserID = UserId,
                Protocol = CommunicationProtocolType.cpTCPIP,
                UseNTAuthentication = BoolType.False
            };
            return faxsvr;
        }

        private RFCOMAPILib.Fax CreateRightFax_Fax(FaxServer faxsvr) {
            var newFax = (RFCOMAPILib.Fax) faxsvr.CreateObject[CreateObjectType.coFax];
            newFax.ToName = fax.CustomerName;
            newFax.ToFaxNumber = Regex.Replace(fax.FaxNumber, "-", "");
            newFax.Attachments.Add(fax.Document);
            newFax.UserComments = "Sent via SAMuel.";
            return newFax;
        }
    }
}