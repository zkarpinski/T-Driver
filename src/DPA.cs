using System;

namespace TDriver {
    public class DPA : AP_Document {
        public DPA() {}

        /// <summary>
        ///    DPA Constructor
        /// </summary>
        public DPA(string accountNumber, string sendTo, string customer, string serviceAddress, string dateOffered,
            string document) : base(document) {
            Account = RegexAccount(accountNumber);
            SendTo = CleanSendToField(sendTo);
            CustomerName = customer.Replace("Customer Name ", String.Empty).ToUpper();
            ServiceAddress = serviceAddress.Replace("Service Address ", String.Empty).Trim();
        }

        //Properties unique to DPAs
        public string KindOfDPA { get; set; }
        public override DocumentType DocumentType => DocumentType.DPA;

        /// <summary>
        /// Remove known generic words from the SendTo cell in the DPA document.
        /// </summary>
        /// <param name="sendToField"></param>
        /// <returns></returns>
        private string CleanSendToField(String sendToField) {
            string[] junkToRemove = {"Email to:", "Mail to:", "Fax to:"};
            foreach (var s in junkToRemove) {
                sendToField = sendToField.Replace(s, String.Empty);
            }
            return (sendToField.Trim());
        }

    }
}
