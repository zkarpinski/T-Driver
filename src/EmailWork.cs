using System;

namespace TDriver {
    //TODO Design email work.
    public class EmailWork : Work {
        public readonly string SendAs;
        private readonly Email _email;

        public override DPA DPAObject { get { return _email; } }

        public EmailWork(Email eEmail, DPAType typeOfDPA) {
            _email = eEmail;
            SendAs = typeOfDPA.SendEmailFrom;
            MoveLocation = typeOfDPA.MoveFolder;
            KindOfDPA = typeOfDPA.Name;
        }

        public override bool Process() {
            #if DEBUG //Allow simulating
            //Debug result :: Email Success.
            //_email.AddSentTime();
            return false;

#else
            //TODO Add email processing
            return false;
#endif
        }
    }
}