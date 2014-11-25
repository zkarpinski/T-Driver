using System;
using System.IO;
using System.Linq;

namespace TDriver {
    public class Fax : DPA {
        /// <summary>
        ///     Fax constructor from DPAFactory when filename matches the correct format.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="faxNumber"></param>
        /// <param name="accountNumber"></param>
        public Fax(String document, String faxNumber, String accountNumber) {
            Account = accountNumber;
            DeliveryMethod = DeliveryMethodTypes.Fax;
            Document = document;
            FileName = Path.GetFileNameWithoutExtension(document);
            FileCreationTime = RemoveMilliseconds(File.GetCreationTime(document));
            ParseFileName(FileName);
            SendTo = faxNumber;
            ValidateFaxNumber();
        }

        public Fax(DPA baseDPA, string faxNumber) {
            SendTo = faxNumber;
            DeliveryMethod = DeliveryMethodTypes.Fax;
            Account = baseDPA.Account;
            Document = baseDPA.Document;
            CustomerName = baseDPA.CustomerName;
            FileName = baseDPA.FileName;
            FileCreationTime = baseDPA.FileCreationTime;
            FileName = Path.GetFileNameWithoutExtension(Document);
            FileCreationTime = RemoveMilliseconds(File.GetCreationTime(Document));
            ValidateFaxNumber();
        }

        public string FaxNumber {
            get { return SendTo; }
        }


        /// <summary>
        ///     Parses the filename to retrieve fax recipient info.
        /// </summary>
        /// <remarks>
        ///     Strictly follows the formart of DPA-#####-#####-For-NAME_HERE-FaxTo-1-555-555-5555...
        /// </remarks>
        private void ParseFileName(string fileName) {
            //TODO Handle Medicals different formating.
            //Split the file string and determine if it's in the fax format.
            string[] strSplit = fileName.Split('-');
            if (strSplit.Count() < 5) {
                //Not a valid fax document.
                CustomerName = "NOT_FOUND";
                IsValid = false;
                return;
            }
            CustomerName = strSplit[4].Replace('_', ' ');
            //TODO If strSplit[5] != "FaxTo", append it. (Add names with Hyphens)
        }

        /// <summary>
        ///     Validate the fax number.
        /// </summary>
        /// <remarks>
        ///     999-999-9999 and 888-888-8888 are common numbers used when incorrect form is used.
        /// </remarks>
        private void ValidateFaxNumber() {
            if ((FaxNumber == "999-999-9999") || (FaxNumber == "888-888-8888") || (FaxNumber == "800-000-0000")) {
                IsValid = false;
            }
        }
    }
}