using System;

namespace TDriver {
    public class EmailWork : Work {
        public readonly string SendAs;
        public readonly Email email;

        public EmailWork(Email eEmail, DPAType typeOfDPA)
        {
            email = eEmail;
            SendAs = typeOfDPA.SendEmailFrom;
            MoveLocation = typeOfDPA.MoveFolder;
            KindOfDPA = typeOfDPA.Name;
        }

        public override bool Process() {
            throw new NotImplementedException();
        }
    }
}