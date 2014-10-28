using System;
using System.IO;
using System.Text.RegularExpressions;

namespace TDriver {
    /// <summary>
    ///     Determines the DPA type and creates the corresponding object.
    /// </summary>
    /// <remarks>
    ///     Valid File Names:
    ///     DPA-99999-99999-FaxTo-1-999-999-9999-To-NAME_HERE.doc
    ///     DPA-99999-99999.doc
    /// </remarks>
    public static class DPAFactory {
        public static DPA Create(string file) {
            //Verify it follows general naming convention
            String fileName = Path.GetFileNameWithoutExtension(file);
            String accountNumber = RegexFileName(@"\d{5}-\d{5}", fileName);
            String faxNumber = RegexFileName(@"\d{3}-\d{3}-\d{4}", fileName); //Without US-code

            //Check if Fax by standard naming convention.
            if ((faxNumber != "NOT_FOUND") && (accountNumber != "NOT_FOUND")) {
                return new Fax(file,faxNumber,accountNumber);
            }

            //TODO Check for email/mail/fax by opening with word.

#if DEBUG  //TESTING CODE

            //Create Word Object
            //Check if Email @
            //return new Email(file);
            //Check if Address City NY 11111

            //Check again for fax number
#endif
            return new Fax(file); //Keep for now
        }

        private static string RegexFileName(string pattern, string fileName) {
            var rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = rgx.Matches(fileName);
            return matches.Count == 1 ? matches[0].Value : "NOT_FOUND";
        }
    }
}