using System;

namespace TDriver {
    public abstract class Work {
        public Boolean Completed;

        protected Work() {
            Completed = false;
        }

        protected Work(DPA dpa, DPAType dpaType) {
            Completed = false;
        }

        public string KindOfDPA { get; protected set; }
        public string MoveLocation { get; protected set; }

        public abstract Boolean Process();
    }
}