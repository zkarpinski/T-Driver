using System;
using System.IO;

namespace TDriver {
    public class Mail : DPA {
        /// <summary>
        ///     Mail constructor from DPAFactory.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="mailingAddress"></param>
        /// <param name="accountNumber"></param>
        public Mail(String document, String mailingAddress, String accountNumber) {
            DeliveryMethod = DeliveryMethodTypes.Email;
            Document = document;
            FileName = Path.GetFileNameWithoutExtension(document);
            SendTo = mailingAddress;
            Account = accountNumber;
        }


        /// <summary>
        ///     Mail constructor; where data is parsed from the DPA base object.
        /// </summary>
        /// <param name="baseDPA"></param>
        /// <param name="mailingAddress"></param>
        public Mail(DPA baseDPA, String mailingAddress) {
            SendTo = CleanMailingAddress(mailingAddress);
            DeliveryMethod = DeliveryMethodTypes.Mail;
            Account = baseDPA.Account;
            Document = baseDPA.Document;
            CustomerName = baseDPA.CustomerName;
            FileName = baseDPA.FileName;
            FileCreationTime = baseDPA.FileCreationTime;
            FileName = Path.GetFileNameWithoutExtension(Document);
            FileCreationTime = RemoveMilliseconds(File.GetCreationTime(Document));
        }

        public string MailingAddress {
            get { return SendTo; }
        }

        private string CleanMailingAddress(String mailingAddress) {
            string[] junkToRemove = {"Email to:", "Mail to:", "Fax to:"};
            foreach (string s in junkToRemove) {
                mailingAddress = mailingAddress.Replace(s, "");
            }
            return (mailingAddress.Trim());
        }
    }
}