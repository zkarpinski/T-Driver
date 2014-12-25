namespace TDriver {
    internal static class WorkFactory {
        /// <summary>
        ///     Creates the corresponding work object depending on the DeliveryMethod.
        /// </summary>
        /// <param name="dpa"></param>
        /// <param name="Subsection"></param>
        /// <returns>Work</returns>
        public static Work Create(AP_Document doc, AP_Subsection Subsection) {
            if (!doc.IsValid) return null;
            switch (doc.DeliveryMethod) {
                case DeliveryMethodType.Fax: //Fax
                    if (doc.GetType() == typeof(Medical_CME)) {
                        Medical_CME medDoc = (Medical_CME)doc;
                        return new FaxWork(Subsection.MoveFolder, medDoc.Document, medDoc.DrFaxNumber, medDoc.DrName, medDoc.FileToSend, doc, Subsection);
                    }
                    else
                        return new FaxWork(Subsection.MoveFolder, doc.Document, doc.SendTo, doc.CustomerName, doc.FileToSend, doc, Subsection);
                case DeliveryMethodType.Email: //Email
                    return new EmailWork(Subsection.MoveFolder, doc.Document, Subsection.SendEmailFrom, Settings.EmailMsg, doc);
                case DeliveryMethodType.Mail: //Mail
                    return new MailWork(Subsection.MoveFolder, doc.Document, doc);
                default:
                    //TODO Add error log, unexpected deliveryMethod
                    return null;
            }
        }
    }
}