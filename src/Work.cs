using System;
using System.IO;

namespace TDriver {
    public abstract class Work {
        public Boolean Completed;

        protected Work() {
            Completed = false;
        }

        protected Work(String moveLocation, String origDocument) {
            MoveLocation = moveLocation;
            DocumentToMove = origDocument;
        }

        public abstract AP_Document DocObject { get; }
        public String SubSection { get; protected set; }
        public DocumentType DocType { get; protected set; }
        protected string MoveLocation { private get; set; }
        protected string DocumentToMove { private get; set; }

        public bool Move() {
            String fileName = Path.GetFileName(DocumentToMove);
            if (fileName == null) return false;
            String saveAs = Path.Combine(MoveLocation, fileName);

            //Delete the file in the destination if it exists already.
            // since File.Move does not overwrite.
            if (File.Exists(saveAs)) {
                File.Delete(saveAs);
            }
            try {
                File.Move(DocumentToMove, saveAs);
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