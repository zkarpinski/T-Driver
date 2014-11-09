using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace TDriver {
    public class Fax : DPA {
        public string FaxNumber { get { return SendTo; } }

        /// <summary>
        ///     Fax constructor from DPAFactory.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="faxNumber"></param>
        /// <param name="accountNumber"></param>
        public Fax(String document, String faxNumber, String accountNumber) {
            Account = accountNumber;
            SharedConstructs(document);
            SendTo = faxNumber;
            ValidateFaxNumber();
        }

        /// <summary>
        ///     Fax constructor where data is parsed from filename.
        /// </summary>
        /// <param name="document"></param>
        public Fax(String document) {
            SharedConstructs(document);
            Account = RegexFileName(@"\d{5}-\d{5}");
            SendTo = RegexFileName(@"\d{3}-\d{3}-\d{4}"); //Regex without US code
            ValidateFaxNumber();
        }



        private void SharedConstructs(String document) {
            DeliveryMethod = DeliveryMethodTypes.Fax;
            Document = document;
            FileName = Path.GetFileNameWithoutExtension(document);
            FileCreationTime = RemoveMilliseconds(File.GetCreationTime(document));
            ParseFileName(FileName);
        }


        //~Fax() { }

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
        ///     999-999-999 and 888-888-8888 are common numbers used when incorrect form is used.
        /// </remarks>
        private void ValidateFaxNumber() {
            if ((FaxNumber == "999-999-9999") || (FaxNumber == "888-888-8888")) {
                IsValid = false;
            }
        }
    }
}