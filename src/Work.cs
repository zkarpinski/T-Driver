using System;
using System.IO;

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
        protected string MoveLocation { private get; set; }
        public abstract DPA DPAObject { get; }

        public bool Move() {
            string dpaFile = DPAObject.Document;
            string fileName = Path.GetFileName(dpaFile);
            if (fileName == null) return false;
            string saveAs = Path.Combine(MoveLocation, fileName);

            //Delete the file in the destination if it exists already.
            // since File.Move does not overwrite.
            if (File.Exists(saveAs)) {
                File.Delete(saveAs);
            }
            try {
                File.Move(dpaFile, saveAs);
                return true;
            }
            catch (Exception) {
                //Todo add notifcation or error log.
                return false;
            }
        }

        public abstract Boolean Process();
    }
}