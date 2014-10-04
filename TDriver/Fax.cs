using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace TDriver
{
    public class Fax
    {
        //Class Properties
        public Fax()
        {
        }

        /// <summary>
        ///     Fax with predetermined info
        /// </summary>
        /// <param name="document"></param>
        /// <param name="customerName"></param>
        /// <param name="number"></param>
        public Fax(String document, String customerName, String number)
        {
            CustomerName = customerName;
            Document = document;
            FaxNumber = number;
            FileName = Path.GetFileNameWithoutExtension(document);
        }

        /// <summary>
        ///     Fax constructor where data is parsed from filename.
        /// </summary>
        /// <param name="document"></param>
        public Fax(String document)
        {
            IsValid = true;
            Sent = false;
            Rejected = false;
            Document = document;
            FileName = Path.GetFileNameWithoutExtension(document);
            ParseFileName(FileName);
            Account = RegexFileName(@"\d{5}-\d{5}");
            FaxNumber = RegexFileName(@"\d{1}-\d{3}-\d{3}-\d{4}");
            if ((FaxNumber == "1-999-999-9999") || (FaxNumber == "1-888-888-8888"))
            {
                IsValid = false;
            }
        }

        public string CustomerName { get; set; }
        public string FaxNumber { get; set; }
        public string Document { get; set; }
        public string Account { get; set; }
        public string FileName { get; set; }
        public Boolean IsValid { get; set; }
        public Boolean Sent { get; set; }
        public Boolean Rejected { get; set; }

        //~Fax() { }

        /// <summary>
        ///     Parses the filename to retrieve fax recipient info.
        /// </summary>
        /// <remarks>
        ///     Strictly follows the formart of DPA-#####-#####-For-NAME_HERE-FaxTo-1-555-555-5555...
        /// </remarks>
        private void ParseFileName(string fileName)
        {
            //Split the file string and determine if it's in the fax format.
            var strSplit = fileName.Split('-');
            if (strSplit.Count() < 5)
            {
                //Not a valid fax document.
                CustomerName = "NOT_FOUND";
                IsValid = false;
                return;
            }

            CustomerName = strSplit[4].Replace('_', ' ');
            //TODO If strSplit[5] != "FaxTo", append it. (Add names with Hyphens)
        }

        private string RegexFileName(string pattern)
        {
            var rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = rgx.Matches(FileName);
            if (matches.Count > 0)
            {
                return matches[0].Value;
            }
            IsValid = false;
            return "NOT_FOUND";
        }
    }
}