using System;
using System.IO;
using System.Text.RegularExpressions;

namespace TDriver {
    public class DPA: AP_Document {

        public DPA() {}

        public string KindOfDPA { get; set; }

        /// <summary>
        ///     Constructer used for Parser
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <param name="sendTo"></param>
        /// <param name="customer"></param>
        /// <param name="premiseAddress"></param>
        /// <param name="dateOffered"></param>
        /// <param name="document"></param>
        public DPA(string accountNumber, string sendTo, string customer, string serviceAddress, string dateOffered,
            string document) {
            this.Account = RegexAccount(accountNumber);
            this.SendTo = CleanSendToField(sendTo);
            this.CustomerName = customer.Replace("Customer Name ", String.Empty).ToUpper();
            this.ServiceAddress = CleanServiceAddressField(serviceAddress);
            this.Document = document;
            this.FileName = Path.GetFileNameWithoutExtension(Document);
            this.FileCreationTime = RemoveMilliseconds(File.GetCreationTime(Document));
        }


        private string CleanSendToField(String sendToField) {
            string[] junkToRemove = { "Email to:", "Mail to:", "Fax to:" };
            foreach (string s in junkToRemove) {
                sendToField = sendToField.Replace(s, String.Empty);
            }
            return (sendToField.Trim());
        }

        private string CleanServiceAddressField(String addressField) {
            string[] junkToRemove = { "Service Address " };
            foreach (string s in junkToRemove) {
                addressField = addressField.Replace(s, String.Empty);
            }
            return (addressField.Trim());
        }

    }
}