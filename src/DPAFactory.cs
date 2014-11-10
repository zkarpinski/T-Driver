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
        private static readonly Parser parser = new Parser();

        public static DPA Create(string file) {
            //Verify it follows general naming convention
            String fileName = Path.GetFileNameWithoutExtension(file);
            String accountNumber = RegexFileName(@"\d{5}-\d{5}", fileName);
            String faxNumber = RegexFileName(@"\d{3}-\d{3}-\d{4}", fileName); //Without US-code

            //Check if Fax by standard naming convention.
            if ((faxNumber != "NOT_FOUND") && (accountNumber != "NOT_FOUND")) {
                return new Fax(file, faxNumber, accountNumber);
            }

            //TODO Check for email/mail/fax by opening with RTF parser


            DPA placeHolder = parser.FindData(file);
            if (placeHolder == null) return null;

            Tuple<DPA.DeliveryMethodTypes, string> result = DetermineDeliveryMethod(placeHolder.SendTo);
            if (result == null) return null;

            //Return the correct DPA child.
            switch (result.Item1) {
                case DPA.DeliveryMethodTypes.Email:
                    return new Email(placeHolder, result.Item2);
                case DPA.DeliveryMethodTypes.Fax:
                    return new Fax(placeHolder, result.Item2);
                case DPA.DeliveryMethodTypes.Mail:
                    //return new Mail();
                    return null;
                    break;
                case DPA.DeliveryMethodTypes.Err:
                default:
                    return null;
                    
            }
#if DEBUG //TESTING CODE
            //Create Word Object
            //Check if Email @
            //return new Email(file);
            //Check if Address City NY 11111

            //Check again for fax number
#endif
        }

        private static string RegexFileName(string pattern, string fileName) {
            var rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = rgx.Matches(fileName);
            return matches.Count == 1 ? matches[0].Value : "NOT_FOUND";
        }

        /// <summary>
        ///     Find the delivery method for the DPA.
        /// </summary>
        /// <param name="sendToField"></param>
        /// <returns></returns>
        private static Tuple<DPA.DeliveryMethodTypes, string> DetermineDeliveryMethod(string sendToField) {
            const String rgxEmailPattern =
                @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?";
            const String rgxFaxPattern = @"\d{3}-\d{3}-\d{4}";

            //Check for email address.
            var rgx = new Regex(rgxEmailPattern, RegexOptions.IgnoreCase);
            MatchCollection emailMatches = rgx.Matches(sendToField);
            if (emailMatches.Count == 1) {
                return Tuple.Create(DPA.DeliveryMethodTypes.Email, emailMatches[0].Value);
            }

            //Check for fax number
            rgx = new Regex(rgxFaxPattern, RegexOptions.IgnoreCase);
            MatchCollection faxMatches = rgx.Matches(sendToField);
            if (faxMatches.Count == 1) {
                return Tuple.Create(DPA.DeliveryMethodTypes.Fax, faxMatches[0].Value);
            }

            return null;
        }
    }
}