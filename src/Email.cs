using System;
using System.IO;

namespace TDriver {
    public class Email : DPA {
        public string FileToSend {get; private set; }

        /// <summary>
        ///     Email constructor from DPAFactory.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="emailAddress"></param>
        /// <param name="accountNumber"></param>
        public Email(String document, String emailAddress, String accountNumber) {
            DeliveryMethod = DeliveryMethodTypes.Email;
            Document = document;
            FileName = Path.GetFileNameWithoutExtension(document);
            SendTo = emailAddress;
            Account = accountNumber;
        }


        /// <summary>
        ///     Email constructor where data is parsed from a DPA base object.
        /// </summary>
        /// <param name="baseDPA"></param>
        /// <param name="emailAddress"></param>
        public Email(DPA baseDPA, string emailAddress) {
            SendTo = emailAddress;
            DeliveryMethod = DeliveryMethodTypes.Email;
            Account = baseDPA.Account;
            Document = baseDPA.Document;
            CustomerName = baseDPA.CustomerName;
            FileName = baseDPA.FileName;
            FileCreationTime = baseDPA.FileCreationTime;
            FileName = Path.GetFileNameWithoutExtension(Document);
            FileToSend = Settings.PdfsPath + @"\" + FileName + @".pdf";
            FileCreationTime = RemoveMilliseconds(File.GetCreationTime(Document));
        }

        public string EmailAddress {
            get { return SendTo; }
        }
    }
}