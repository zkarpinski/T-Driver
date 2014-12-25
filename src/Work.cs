using System;
using System.IO;

namespace TDriver {
    public abstract class Work {
        public Boolean Completed;

        protected Work() {
            Completed = false;
        }

        protected Work(String moveLocation, String origDocument) {
            this.MoveLocation = moveLocation;
            this.DocumentToMove = origDocument;
        }

        protected string MoveLocation { private get; set; }
        protected string DocumentToMove { private get; set; }
        public abstract AP_Document DocObject { get; }

        public bool Move() {
            string docFile = DocObject.Document;
            string fileName = Path.GetFileName(docFile);
            if (fileName == null) return false;
            string saveAs = Path.Combine(MoveLocation, fileName);

            //Delete the file in the destination if it exists already.
            // since File.Move does not overwrite.
            if (File.Exists(saveAs)) {
                File.Delete(saveAs);
            }
            try {
                File.Move(docFile, saveAs);
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