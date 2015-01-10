using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace TDriver {
    public class MedicalCME : AP_Document {
        //Constructor used by the parser.
        public MedicalCME(string drFaxNumber, string drName, string drPhoneNumber, string drCompany,
            string accountNumber, string accountHolder, string patientName, string serviceAddress,
            string file) : base(file) {
            SendTo = drFaxNumber;
            DrName = drName;
            DrPhoneNumber = drPhoneNumber;
            DrCompany = drCompany;
            Account = accountNumber;
            CustomerName = accountHolder;
            PatientName = patientName;
            ServiceAddress = serviceAddress;
        }

        //Properties unique to Medical CMEs
        public override DocumentType DocumentType => DocumentType.CME;
        public string DrFaxNumber => SendTo;
        public string DrCompany { get; private set; }
        public string DrName { get; private set; }
        public string DrPhoneNumber { get; private set; }
        public string PatientName { get; private set; }

        /// <summary>
        /// Checks the zip code prefix against the region given.
        /// </summary>
        /// <param name="strRegion"></param>
        /// <returns></returns>
        public bool CheckRegion(String strRegion) {
            //Get the zip code using Regular Expressions
            // Acquires the LAST match of a space followed by five digits. ' 13219'
            const string rgxZipPattern = @"\s\d{5}";
            var rgx = new Regex(rgxZipPattern, RegexOptions.IgnoreCase);
            MatchCollection matches = rgx.Matches(ServiceAddress);
            if (matches.Count < 1)
                return false; //If no zip is found, don't continue. (Manager choice)
            String zipcode = matches[matches.Count - 1].Value.Trim(); //Trim to remove space.
             
            //Check zip code against the specified region using Linq.
            return Regions.Region(strRegion).ZIPCODE_PREFIXES.Any(zipPrefix => zipcode.StartsWith(zipPrefix));

        }
    }
}