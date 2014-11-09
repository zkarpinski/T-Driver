using System;
using System.IO;

namespace TDriver {
    //TODO Design email DPA
    public class Email : DPA {
        /// <summary>
        ///     Email constructor from DPAFactory.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="emailAddress"></param>
        /// <param name="accountNumber"></param>
        public Email(String document, String emailAddress, String accountNumber) {
            SharedConstructs(document);
            EmailAddress = emailAddress;
            Account = accountNumber;
            //TODO Grab Email Address
        }

        /// <summary>
        ///     Email constructor where data is parsed from filename.
        /// </summary>
        /// <param name="document"></param>
        public Email(String document) {
            SharedConstructs(document);
            Account = RegexFileName(@"\d{5}-\d{5}");
            //TODO Grab Email Address
        }

        public string EmailAddress { get; set; }

        private void SharedConstructs(String document) {
            DeliveryMethod = DeliveryMethodTypes.Email;
            Document = document;
            FileName = Path.GetFileNameWithoutExtension(document);
        }
    }
}