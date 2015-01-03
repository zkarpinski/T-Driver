namespace TDriver {
    //TODO Design mail work.
    public class MailWork : Work {
        private readonly AP_Document _mail;

        public MailWork(string moveLocation, string origDocument, AP_Document mail) : base(moveLocation, origDocument) {
            _mail = mail;
        }

        public override AP_Document DocObject => _mail;

        public string FileToPrint {
            get { return _mail.FileToSend; }
        }

        public override bool Process() {
#if DEBUG //Allow simulating
    //Debug result :: Mail Success.
            _mail.AddSentTime();
            
            Debug.WriteLine(String.Format("Mailed {0} to {1} for {2} with account {3} using printer: {4}", FileToPrint, _mail.SendTo, _mail.CustomerName, _mail.Account, "TODO Add Printer"));
            return true;

#else
            //TODO Add mail processing
            return false;
#endif
        }
    }
}