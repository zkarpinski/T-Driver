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
        NR_DPA = 3,
    }

    public class AP_Document {
        /// <summary>
        /// Default constructor
        /// </summary>
        protected AP_Document() {}

        /// <summary>
        /// Constructor used by the derived classes as base.
        /// </summary>
        /// <param name="document"></param>
        protected AP_Document(string document) {
            IsValid = true;
            Sent = false;
            this.Document = document;
            this.FileName = Path.GetFileNameWithoutExtension(Document);
            this.FileCreationTime = RemoveMilliseconds(File.GetCreationTime(Document));
        }

        //Data properties
        public string SendTo { get; set; }
        public string CustomerName { get; protected set; }
        public string Document { get; protected set; }
        public string FileToSend { get; protected set; }
        public string Account { get; protected set; }
        public string ServiceAddress { get; protected set; }
        public string FileName { get; protected set; }
        public DeliveryMethodType DeliveryMethod { get; protected set; }
        public DateTime TimeSent { get; private set; }
        public DateTime FileCreationTime { get; protected set; }
        public virtual DocumentType DocumentType {
            get { return DocumentType.ERROR; }
        }

        //Internal use properties
        public Boolean IsValid { get; set; }
        public Boolean Sent { get; set; }
        public String InvalidReason { get; set; }

        /// <summary>
        /// Acquire the account number, if one exists, using Regular Expressions
        /// </summary>
        /// <param name="strAccount">String to check for account number.</param>
        /// <returns>String "#####-#####" or null</returns>
        protected string RegexAccount(string strAccount) {
            const string rgxAccountPattern = @"\d{5}-\d{5}";
            var rgx = new Regex(rgxAccountPattern, RegexOptions.IgnoreCase);
            Match match = rgx.Match(strAccount);
            if (match.Success) {
                return match.Value;
            }
            IsValid = false;
            return null;
        }

        /// <summary>
        /// Stores the time when the method is called, as the time sent.
        /// </summary>
        public void AddSentTime() {
            TimeSent = RemoveMilliseconds(DateTime.Now);
        }

        /// <summary>
        ///     Define the delivery method and change what file is sent.
        /// </summary>
        /// <remarks>
        ///     Mail = Original Document
        ///     Email = .PDF
        ///     Fax = Original Document
        /// </remarks>
        /// <param name="delivery"></param>
        public void ChangeDeliveryType(DeliveryMethodType delivery) {
            this.DeliveryMethod = delivery;
            if (delivery == DeliveryMethodType.Email)
                this.FileToSend = String.Format(@"{0}\{1}.pdf", Settings.PdfsPath, this.FileName);
            else if (delivery == DeliveryMethodType.Mail)
                this.FileToSend = this.Document;
            else if (delivery == DeliveryMethodType.Fax)
                this.FileToSend = this.Document;
            else
                this.FileToSend = this.Document;
        }

        /// <summary>
        ///     Removes milliseconds from DateTime to match Access 2003 Date/Time field.
        /// </summary>
        /// <remarks>http://stackoverflow.com/questions/1004698/how-to-truncate-milliseconds-off-of-a-net-datetime/1004708#1004708</remarks>
        protected DateTime RemoveMilliseconds(DateTime date) {
            date = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
            return date;
        }
    }
}