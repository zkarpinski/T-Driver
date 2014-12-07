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
    ///     Acct-99999-99999-To-1-999-999-9999-To-NAME_HERE.doc                     (Medical Fax Document) *when US-code added.
    ///     DPA-99999-99999.doc                                                     (Email, Mail, Fax or anomaly Medical Document)
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
            if ((faxNumber != null) && (accountNumber !=  null)) {
                return new Fax(file, faxNumber, accountNumber);
            }

            //Open document with RTF Parser and parse the table.
            DPA placeHolder = parser.FindData(file);
            if (placeHolder == null) return null;

            //Determine the delivery method by analyzing the "SendTo" field's contents.
            Tuple<DPA.DeliveryMethodTypes, string> result = DetermineDeliveryMethod(placeHolder.SendTo);
            if (result == null) return null;

            //Return the correct DPA child.
            switch (result.Item1) {
                case DPA.DeliveryMethodTypes.Email:
                    return new Email(placeHolder, result.Item2);
                case DPA.DeliveryMethodTypes.Fax:
                    return new Fax(placeHolder, result.Item2);
                case DPA.DeliveryMethodTypes.Mail:
                    return new Mail(placeHolder, result.Item2);
                default:
                    return null;
            }
        }

        private static string RegexFileName(string pattern, string fileName) {
            var rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = rgx.Matches(fileName);
            return matches.Count == 1 ? matches[0].Value : null;
        }

        /// <summary>
        ///     Find the delivery method for the DPA.
        /// </summary>
        /// <param name="sendToField"></param>
        /// <returns></returns>
        private static Tuple<DPA.DeliveryMethodTypes, string> DetermineDeliveryMethod(string sendToField) {
            //Check for email address.
            const String rgxEmailPattern =
                @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?";
            var rgx = new Regex(rgxEmailPattern, RegexOptions.IgnoreCase);
            MatchCollection emailMatches = rgx.Matches(sendToField);
            if (emailMatches.Count == 1) {
                return Tuple.Create(DPA.DeliveryMethodTypes.Email, emailMatches[0].Value);
            }

            //Check for a mailing address.
            //Look for space, two letters, space then 5 digits
            //ex. ' NY 13219'
            const String rgxAddressPattern = @"\s[a-z]{2}\s\d{5}";
            rgx = new Regex(rgxAddressPattern, RegexOptions.IgnoreCase);
            MatchCollection mailMatches = rgx.Matches(sendToField);
            if (mailMatches.Count == 1) {
                return Tuple.Create(DPA.DeliveryMethodTypes.Mail, sendToField);
            }

            //Check for fax number
            const String rgxFaxPattern = @"\d{3}-\d{3}-\d{4}";
            rgx = new Regex(rgxFaxPattern, RegexOptions.IgnoreCase);
            MatchCollection faxMatches = rgx.Matches(sendToField);
            if (faxMatches.Count == 1) {
                return Tuple.Create(DPA.DeliveryMethodTypes.Fax, faxMatches[0].Value);
            }

            return null;
        }
    }
}