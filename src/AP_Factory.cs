using System;
using System.Text.RegularExpressions;

namespace TDriver {
    /// <summary>
    ///     Determines the AP Document type and creates the corresponding object.
    /// </summary>
    /// <remarks>
    ///     Valid File Names:
    ///     Acct-99999-99999-To-999-999-9999-To-NAME_HERE.doc                       (Medical Fax Document)
    ///     Acct-99999-99999-To-1-999-999-9999-To-NAME_HERE.doc                     (Medical Fax Document) *when US-code added.
    ///     Acct-99999-99999.doc                                                    (Medical Anomaly Document) *still faxed
    ///     DPA-99999-99999.doc                                                     (DPA Email, DPA Mail or DPA Fax Document)
    ///     DPA-99999-99999-For-NAME_HERE-FaxTo-1-999-999-9999-ATTN-NAME_HERE.doc   (DPA Fax Document)
    ///     DPA-99999-99999-For-NAME_HERE-FaxTo-1-999-999-9999                      (DPA Fax Document)
    /// </remarks>
    public static class AP_Factory {
        private static readonly Parser parser = new Parser();

        internal static AP_Document Create(string file, AP_Subsection subsection) {
            //Determine the document type by checking against the general naming conventions
            DocumentType docType = DetermineDocumentType(file);
            if (docType == DocumentType.ERROR) {
                Logger.AddError(Settings.ErrorLogfile, file + "was skipped. Does NOT follow general naming convention.");
                return null;
            }

            //Open document with RTF Parser
            //Todo load doc into parser and parse each item seperately
            AP_Document newApDoc = parser.FindData(file, docType);
            if (newApDoc == null) {
                Logger.AddError(Settings.ErrorLogfile, file + "was skipped by the parser.");
                return null;
            }

            //Derived case specific options
            //DPA
            //Add the type of DPA.
            if (docType == DocumentType.DPA && newApDoc is DPA) {
                ((DPA)newApDoc).KindOfDPA = subsection.Name;
            }

            //Determine the delivery method by analyzing the "SendTo" field's contents.
            //TODO: Use fax number from file name?
            Tuple<DeliveryMethodType, string> result = DetermineDeliveryMethod(newApDoc.SendTo);
            if (result == null)
                return null;

            //Change the delivery method and update where it's going to.
            newApDoc.ChangeDeliveryType(result.Item1);
            newApDoc.SendTo = result.Item2;
            //Return the new AP_Document
            return newApDoc;
        }

        /// <summary>
        ///     Find the delivery method for the document
        /// </summary>
        /// <param name="sendToField"></param>
        /// <returns></returns>
        private static Tuple<DeliveryMethodType, string> DetermineDeliveryMethod(string sendToField) {
            //Check for email address.
            const String rgxEmailPattern =
                @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?";
            var rgx = new Regex(rgxEmailPattern, RegexOptions.IgnoreCase);
            MatchCollection emailMatches = rgx.Matches(sendToField);
            if (emailMatches.Count == 1) {
                return Tuple.Create(DeliveryMethodType.Email, emailMatches[0].Value);
            }

            //Check for a mailing address.
            //Look for a space, two letters, space and then 5 digits
            //ex. ' NY 13219'
            const String rgxAddressPattern = @"\s[a-z]{2}\s\d{5}";
            rgx = new Regex(rgxAddressPattern, RegexOptions.IgnoreCase);
            MatchCollection mailMatches = rgx.Matches(sendToField);
            if (mailMatches.Count == 1) {
                return Tuple.Create(DeliveryMethodType.Mail, sendToField);
            }

            //Check for fax number
            // Return # found plus US-Code (1)
            const String rgxFaxPattern = @"\d{3}-\d{3}-\d{4}";
            rgx = new Regex(rgxFaxPattern, RegexOptions.IgnoreCase);
            Match faxMatch = rgx.Match(sendToField);
            if (faxMatch.Success && ValidateFaxNumber(faxMatch.Value)) {
                return Tuple.Create(DeliveryMethodType.Fax, "1-" + faxMatch.Value);
            }

            return null;
        }

        /// <summary>
        /// Determine the document type based on naming convention.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static DocumentType DetermineDocumentType(string file) {
            //Check for Medical CME
            //Looks for 'Acct-99999-99999'
            const String rgxCME_Convention = @"Acct-\d{5}-\d{5}";
            var rgx = new Regex(rgxCME_Convention, RegexOptions.IgnoreCase);
            Match cmeMatch = rgx.Match(file);
            if (cmeMatch.Success) {
                return DocumentType.CME;
            }

            //Check for DPA
            //Looks for 'DPA-99999-99999-'
            const String rgxDPA_Convention = @"DPA-\d{5}-\d{5}";
            rgx = new Regex(rgxDPA_Convention, RegexOptions.IgnoreCase);
            Match dpaMatch = rgx.Match(file);
            if (dpaMatch.Success) {
                return DocumentType.DPA;
            }

            return DocumentType.ERROR;
        }

        /// <summary>
        ///     Validate the fax number.
        /// </summary>
        /// <remarks>
        ///     999-999-9999 and 888-888-8888 are common numbers used when incorrect form is used.
        /// </remarks>
        /// <returns>
        /// True: Valid fax number
        /// False: Invalid fax number</returns>
        private static Boolean ValidateFaxNumber(string faxNumber) {
            if ((faxNumber == "999-999-9999") || (faxNumber == "888-888-8888") || (faxNumber == "000-000-0000")) {
                return false;
            }
            return true;
        }

    }
}