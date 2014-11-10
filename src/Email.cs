using System;
using System.IO;

namespace TDriver {
    //TODO Design email DPA
    public class Email : DPA {

        public string EmailAddress { get { return SendTo; } }

        /// <summary>
        ///     Email constructor from DPAFactory.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="emailAddress"></param>
        /// <param name="accountNumber"></param>
        public Email(String document, String emailAddress, String accountNumber) {
            this.DeliveryMethod = DeliveryMethodTypes.Email;
            this.Document = document;
            this.FileName = Path.GetFileNameWithoutExtension(document);
            this.SendTo = emailAddress;
            this.Account = accountNumber;
            //TODO Grab Email Address
        }


        /// <summary>
        ///     Email constructor where data is parsed from a DPA base object.
        /// </summary>
        /// <param name="baseDPA"></param>
        /// <param name="emailAddress"></param>
        public Email(DPA baseDPA, string emailAddress)
        {
            this.SendTo = emailAddress;
            DeliveryMethod = DeliveryMethodTypes.Email;
            this.Account = baseDPA.Account;
            this.Document = baseDPA.Document;
            this.CustomerName = baseDPA.CustomerName;
            this.FileName = baseDPA.FileName;
            this.FileCreationTime = baseDPA.FileCreationTime;
            this.FileName = Path.GetFileNameWithoutExtension(this.Document);
            this.FileCreationTime = RemoveMilliseconds(File.GetCreationTime(this.Document));
        }

    }
}