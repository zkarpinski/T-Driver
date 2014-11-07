using System;
using System.IO;
using System.Text.RegularExpressions;

namespace TDriver {
    /// <summary>
    ///     Determines the DPA type and create the corresponding object.
    /// </summary>
    /// <remarks>
    ///     Valid File Names:
    ///     Acct-99999-99999-To-999-999-9999-To-NAME_HERE.doc                       (Medical Fax Document)
    ///     DPA-99999-99999.doc                                                     (Email or Mail or Fax Document)
    ///     DPA-99999-99999-For-NAME_HERE-FaxTo-1-999-999-9999-ATTN-NAME_HERE.doc   (Fax Document)
    ///     DPA-99999-99999-For-NAME_HERE-FaxTo-1-999-999-9999                      (Fax Document)
    /// </remarks>
    public static class DPAFactory {
        public static DPA Create(string file) {
            //Verify it follows general naming convention
            String fileName = Path.GetFileNameWithoutExtension(file);
            String accountNumber = RegexFileName(@"\d{5}-\d{5}", fileName);
            String faxNumber = RegexFileName(@"\d{3}-\d{3}-\d{4}", fileName); //Without US-code

            //Check if Fax by standard naming convention.
            if ((faxNumber != "NOT_FOUND") && (accountNumber != "NOT_FOUND")) {
                return new Fax(file, faxNumber, accountNumber);
            }

            //TODO Check for email/mail/fax by opening with word.

#if DEBUG //TESTING CODE

            //Create Word Object
            //Check if Email @
            //return new Email(file);
            //Check if Address City NY 11111

            //Check again for fax number
#endif
            return null;
        }

        private static string RegexFileName(string pattern, string fileName) {
            var rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = rgx.Matches(fileName);
            return matches.Count == 1 ? matches[0].Value : "NOT_FOUND";
        }
    }
}