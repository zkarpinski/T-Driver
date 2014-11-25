namespace TDriver {
    //TODO Design mail work.
    public class MailWork : Work {
        private readonly Mail _mail;

        public MailWork(Mail mail, ref DPAType typeOfDPA) {
            _mail = mail;
            MoveLocation = typeOfDPA.MoveFolder;
            KindOfDPA = typeOfDPA.Name;
        }

        public override DPA DPAObject {
            get { return _mail; }
        }

        public override bool Process() {
#if DEBUG //Allow simulating
    //Debug result :: Mail Success.
    //_mail.AddSentTime();
            return false;

#else
            //TODO Add mail processing
            return false;
#endif
        }
    }
}