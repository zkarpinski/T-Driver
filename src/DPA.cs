using System;
using System.IO;
using System.Text.RegularExpressions;

namespace TDriver {
    public class DPA {
        [Flags]
        public enum DeliveryMethodTypes {
            Err = 0,
            Fax = 1,
            Email = 2,
            Mail = 3,
        }

        public string SendTo;

        protected DPA(String document) {
            IsValid = true;
            Sent = false;
            Rejected = false;
            Document = document;
            FileName = Path.GetFileNameWithoutExtension(document);
            Account = RegexFileName(@"\d{5}-\d{5}");
        }

        protected DPA() {
            IsValid = true;
            Sent = false;
            Rejected = false;
        }

        /// <summary>
        ///     Constructer used for Parser to return a base DPA.
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <param name="sendTo"></param>
        /// <param name="customer"></param>
        /// <param name="premiseAddress"></param>
        /// <param name="dateOffered"></param>
        /// <param name="document"></param>
        public DPA(string accountNumber, string sendTo, string customer, string premiseAddress, string dateOffered,
            string document) {
            Account = RegexAccount(accountNumber);
            SendTo = sendTo;
            CustomerName = customer.Replace("Customer Name ", "").ToUpper();
            Document = document;
        }

        public string CustomerName { get; protected set; }
        public string Document { get; protected set; }
        public string Account { get; protected set; }
        public string ServiceAddress { get; protected set; }
        public string FileName { get; protected set; }
        public DeliveryMethodTypes DeliveryMethod { get; protected set; }
        public DateTime TimeSent { get; private set; }
        public DateTime FileCreationTime { get; protected set; }

        public Boolean IsValid { get; set; }
        public Boolean Sent { get; set; }
        public Boolean Rejected { get; set; }

        protected string RegexFileName(string pattern) {
            var rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = rgx.Matches(FileName);
            if (matches.Count > 0) {
                return matches[0].Value;
            }
            IsValid = false;
            return "NOT_FOUND";
        }

        protected string RegexAccount(string strAccount) {
            const string rgxAccountPattern = @"\d{5}-\d{5}";
            var rgx = new Regex(rgxAccountPattern, RegexOptions.IgnoreCase);
            MatchCollection matches = rgx.Matches(strAccount);
            if (matches.Count > 0) {
                return matches[0].Value;
            }
            IsValid = false;
            return null;
        }

        public void AddSentTime() {
            TimeSent = RemoveMilliseconds(DateTime.Now);
        }

        /// <summary>
        ///     Removes milliseconds from DateTime
        /// </summary>
        /// <remarks>http://stackoverflow.com/questions/1004698/how-to-truncate-milliseconds-off-of-a-net-datetime/1004708#1004708</remarks>
        protected DateTime RemoveMilliseconds(DateTime date) {
            date = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
            return date;
        }
    }
}