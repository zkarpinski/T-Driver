using System;

namespace TDriver {
    //TODO Design email work.
    public class EmailWork : Work {
        public readonly string SendAs;
        public readonly Email email;

        public EmailWork(Email eEmail, DPAType typeOfDPA) {
            email = eEmail;
            SendAs = typeOfDPA.SendEmailFrom;
            MoveLocation = typeOfDPA.MoveFolder;
            KindOfDPA = typeOfDPA.Name;
        }

        public override DPA DPAObject {
            get { throw new NotImplementedException(); }
        }

        public override bool Process() {
            throw new NotImplementedException();
        }
    }
}