﻿using System;
using System.IO;
using System.Text.RegularExpressions;

namespace TDriver {
    public class Medical_CME : AP_Document {

        //Properties unique to Medcial CMEs
        // Some are not used, and are strictly for future reference purposes.
        public override DocumentType DocumentType { get { return DocumentType.CME; } }
        public string DrFaxNumber { get { return this.SendTo; } }
        public string DrCompany { get; private set; }
        public string DrName { get; private set; }
        public string DrPhoneNumber { get; private set; }
        public string LastVisit { get; private set; }
        public string PatientName { get; private set; }

        //Constructor used by the parser.
        public Medical_CME(string drFaxNumber, string drName, string drPhoneNumber, string drCompany, string accountNumber, string accountHolder, string patientName, string serviceAddress, string lastVisit, string file) : base(file) {
            this.SendTo = drFaxNumber;
            this.DrName = drName;
            this.DrPhoneNumber = drPhoneNumber;
            this.DrCompany = drCompany;
            this.Account = accountNumber;
            this.CustomerName = accountHolder;
            this.PatientName = patientName;
            this.ServiceAddress = serviceAddress;
            this.LastVisit = lastVisit;

            //ChangeDeliveryType(DeliveryMethodType.Fax);
        }
    }
}
