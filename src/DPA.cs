using System;

namespace TDriver {
    public class DPA : AP_Document {
        public DPA() {}

        /// <summary>
        ///     Constructer used for Parser
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <param name="sendTo"></param>
        /// <param name="customer"></param>
        /// <param name="serviceAddress"></param>
        /// <param name="dateOffered"></param>
        /// <param name="document"></param>
        public DPA(string accountNumber, string sendTo, string customer, string serviceAddress, string dateOffered,
            string document) : base(document) {
            Account = RegexAccount(accountNumber);
            SendTo = CleanSendToField(sendTo);
            CustomerName = customer.Replace("Customer Name ", String.Empty).ToUpper();
            ServiceAddress = serviceAddress.Replace("Service Address ", String.Empty).Trim();
        }

        public string KindOfDPA { get; set; }

        public override DocumentType DocumentType => DocumentType.DPA;

        private string CleanSendToField(String sendToField) {
            string[] junkToRemove = {"Email to:", "Mail to:", "Fax to:"};
            foreach (var s in junkToRemove) {
                sendToField = sendToField.Replace(s, String.Empty);
            }
            return (sendToField.Trim());
        }
    }
}