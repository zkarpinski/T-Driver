using System;
using System.IO;
using System.Runtime.Remoting.Messaging;
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

        public string CustomerName { get; protected set; }
        public string Document { get; protected set; }
        public string Account { get; protected set; }
        public string FileName { get; set; }
        public DeliveryMethodTypes DeliveryMethod { get; protected set; }
        public Boolean IsValid { get; set; }
        public Boolean Sent { get; set; }
        public DateTime TimeSent { get;private set; }
        public DateTime FileCreationTime { get; protected set; }
        public Boolean Rejected { get; set; }
        public string SendTo;

        protected string RegexFileName(string pattern) {
            var rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = rgx.Matches(FileName);
            if (matches.Count > 0) {
                return matches[0].Value;
            }
            IsValid = false;
            return "NOT_FOUND";
        }

        public void AddSentTime() {
            TimeSent = RemoveMilliseconds(DateTime.Now);
        }

        /// <summary>
        /// Removes milliseconds from DateTime
        /// </summary>
        /// <remarks>http://stackoverflow.com/questions/1004698/how-to-truncate-milliseconds-off-of-a-net-datetime/1004708#1004708</remarks>
        protected DateTime RemoveMilliseconds(DateTime date) { 
            date = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
            return date;

        }

    }
}