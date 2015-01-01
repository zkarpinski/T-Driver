namespace TDriver {
    public class MedicalCME : AP_Document {
        //Constructor used by the parser.
        public MedicalCME(string drFaxNumber, string drName, string drPhoneNumber, string drCompany,
            string accountNumber, string accountHolder, string patientName, string serviceAddress, string lastVisit,
            string file) : base(file) {
            SendTo = drFaxNumber;
            DrName = drName;
            DrPhoneNumber = drPhoneNumber;
            DrCompany = drCompany;
            Account = accountNumber;
            CustomerName = accountHolder;
            PatientName = patientName;
            ServiceAddress = serviceAddress;

            //ChangeDeliveryType(DeliveryMethodType.Fax);
        }

        //Properties unique to Medcial CMEs
        // Some are not used, and are strictly for future reference purposes.
        public override DocumentType DocumentType => DocumentType.CME;
        public string DrFaxNumber => SendTo;
        public string DrCompany { get; private set; }
        public string DrName { get; private set; }
        public string DrPhoneNumber { get; private set; }
        public string PatientName { get; private set; }
    }
}