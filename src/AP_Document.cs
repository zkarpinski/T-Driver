using System;
using System.IO;
using System.Text.RegularExpressions;

namespace TDriver {
    [Flags]
    public enum DeliveryMethodType {
        Err = 0,
        Fax = 1,
        Email = 2,
        Mail = 3,
    }

    [Flags]
    public enum DocumentType {
        ERROR = 0,
        DPA = 1,
        CME = 2,
    }

    public class AP_Document {

        //Data properties
        public string SendTo { get; set; }
        public string CustomerName { get; protected set; }
        public string Document { get; protected set; }
        public string FileToSend { get; protected set; }
        public string Account { get; protected set; }
        public string ServiceAddress { get; protected set; }
        public string FileName { get; protected set; }
        public DeliveryMethodType DeliveryMethod { get; protected set; }
        public virtual DocumentType DocumentType { get { return DocumentType.ERROR; } }
        public DateTime TimeSent { get; private set; }
        public DateTime FileCreationTime { get; protected set; }

        //Internal use properties
        public Boolean IsValid { get; set; }
        public Boolean Sent { get; set; }
        public Boolean Rejected { get; set; }

        protected AP_Document() {}

        protected AP_Document(string document) {
            IsValid = true;
            Sent = false;
            Rejected = false;
            this.Document = document;
            this.FileName = Path.GetFileNameWithoutExtension(Document);
            this.FileCreationTime = RemoveMilliseconds(File.GetCreationTime(Document));
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
        /// Define the delivery method and change what file is sent.
        /// </summary>
        /// <remarks>
        /// Mail = Original Document
        /// Email = .PDF
        /// Fax = Original Document
        /// </remarks>
        /// <param name="delivery"></param>
        public void ChangeDeliveryType(DeliveryMethodType delivery) {
            this.DeliveryMethod = delivery;
            if (delivery == DeliveryMethodType.Email)
                this.FileToSend = String.Format(@"{0}\{1}.pdf", Settings.PdfsPath, this.FileName);
            else if (delivery == DeliveryMethodType.Mail)
                //Todo decide if mail items should be made to pdfs first.
                this.FileToSend = this.Document;
            else if (delivery == DeliveryMethodType.Fax)
                this.FileToSend = this.Document;
            else
                this.FileToSend = this.Document;
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
