using System;

namespace TDriver {
    public class EmailWork : Work {
        public readonly string SendAs;
        public readonly Email email;

        public EmailWork(Email eEmail, DPAType emailWorkDPAType) {
            email = eEmail;
            SendAs = emailWorkDPAType.SendEmailFrom;
            MoveLocation = emailWorkDPAType.MoveFolder;
            KindOfDPA = emailWorkDPAType.Name;
        }

        public override bool Process() {
            throw new NotImplementedException();
        }
    }
}