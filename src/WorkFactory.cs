namespace TDriver {
    internal static class WorkFactory {
        /// <summary>
        ///     Creates the corresponding work object depending on the DeliveryMethod.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="subsection"></param>
        /// <returns>Work</returns>
        public static Work Create(AP_Document doc, AP_Subsection subsection) {
            if (!doc.IsValid) return null;
            switch (doc.DeliveryMethod) {
                case DeliveryMethodType.Fax: //Fax
                    if (doc.GetType() == typeof (MedicalCME)) {
                        MedicalCME medDoc = (MedicalCME) doc;
                        string moveFolder =  subsection.MoveFolder;
                        //Move CMEs faxed to the doctor to a different folder.
                        if (medDoc.IsToDoctor) { moveFolder = subsection.MoveFolder2; }

                        return new FaxWork(moveFolder, medDoc.Document, medDoc.DrFaxNumber, medDoc.DrName,
                            medDoc.FileToSend, doc, subsection);
                    }
                    return new FaxWork(subsection.MoveFolder, doc.Document, doc.SendTo, doc.CustomerName,
                        doc.FileToSend, doc, subsection);
                case DeliveryMethodType.Email: //Email
                    return new EmailWork(subsection.MoveFolder, doc.Document, subsection.SendEmailFrom,
                        Settings.EmailMsg, doc);
                case DeliveryMethodType.Mail: //Mail
                    return new MailWork(subsection.MoveFolder, doc.Document, doc);
                default:
                    //TODO Add error log, unexpected deliveryMethod
                    return null;
            }
        }
    }
}