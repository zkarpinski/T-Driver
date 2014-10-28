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
        public string MoveLocation { get; protected set; }
        public string DPAFile { get; protected set; }

        public bool Move() {
            string fileName = Path.GetFileName(DPAFile);
            if (fileName == null) return false;
            string saveAs = Path.Combine(MoveLocation, fileName);

            //Delete the file in the destination if it exists already.
            // since File.Move does not overwrite.
            if (File.Exists(saveAs)) {
                File.Delete(saveAs);
            }
            try {
                File.Move(DPAFile, saveAs);
                return true;
            }
            catch (Exception) {
                return false;
            }
        }

        public abstract Boolean Process();
    }
}