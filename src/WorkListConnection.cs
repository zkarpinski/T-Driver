using System;
using System.Data;
using System.Data.OleDb;

namespace TDriver {
    internal class WorkListConnection {
        //Constant SQl Queries
        private const string strDPACommand =
            @"INSERT INTO DeferredPaymentAgreements (DPAType,DeliveryMethod,AccountNumber,SentTo,CustomerName,SentTime,FileCreated,File)" +
            "VALUES (@dPAType,@delivery,@account,@sendTo,@customerName,@timeSent,@fileCreationTime,@document)";
        private const string strCMECommand =
            @"INSERT INTO CertifiedMedicalEmergency (DeliveryMethod,AccountNumber,DrName,DrFaxNumber,DrPhoneNumber,DrCompany,accountHolder,PatientName,ServiceAddress,SentTime,FileCreated,File)" +
            "VALUES (@delivery,@account,@drName,@drFaxNumber,@drPhoneNumber,@drCompany,@accountHolder,@patientName,@serviceAddress,@timeSent,@fileCreationTime,@document)";

        private static OleDbConnection _con;

        public WorkListConnection(string databaseFile) {
            _con = new OleDbConnection(
                @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + databaseFile);
        }

        private bool AddDPARecord(DPA dpaDoc) {
            try {
                _con.Open();
                //Todo Verify connection state is Open
                using (var cmdInsert = new OleDbCommand(strDPACommand, _con)) {
                    cmdInsert.Parameters.AddWithValue("@dPAType", dpaDoc.KindOfDPA);
                    cmdInsert.Parameters.AddWithValue("@delivery", dpaDoc.DeliveryMethod.ToString());
                    cmdInsert.Parameters.AddWithValue("@account", dpaDoc.Account);
                    cmdInsert.Parameters.AddWithValue("@sendTo", dpaDoc.SendTo);
                    cmdInsert.Parameters.AddWithValue("@customerName", dpaDoc.CustomerName);
                    cmdInsert.Parameters.AddWithValue("@timeSent", dpaDoc.TimeSent);
                    cmdInsert.Parameters.AddWithValue("@fileCreationTime", dpaDoc.FileCreationTime);
                    cmdInsert.Parameters.AddWithValue("@document", dpaDoc.Document);
                    cmdInsert.ExecuteNonQuery();
                }
                _con.Close();
                //Todo verify connection closed.
                return true;
            }
            catch (Exception ex) {
                Logger.AddError(Settings.ErrorLogfile, ex.Message);
                //Close the connection if it's not already closed.
                if (_con.State != ConnectionState.Closed)
                    _con.Close();
                return false;
            }
        }

        private bool AddMedicalRecord(MedicalCME cmeDoc) {
            try {
                _con.Open();
                //Todo Verify connection state is Open
                using (var cmdInsert = new OleDbCommand(strCMECommand, _con)) {
                    cmdInsert.Parameters.AddWithValue("@delivery", cmeDoc.DeliveryMethod.ToString());
                    cmdInsert.Parameters.AddWithValue("@account", cmeDoc.Account);
                    cmdInsert.Parameters.AddWithValue("@drName", cmeDoc.DrName);
                    cmdInsert.Parameters.AddWithValue("@drFaxNumber", cmeDoc.DrFaxNumber);
                    cmdInsert.Parameters.AddWithValue("@drPhoneNumber", cmeDoc.DrPhoneNumber);
                    cmdInsert.Parameters.AddWithValue("@drCompany", cmeDoc.DrCompany);
                    cmdInsert.Parameters.AddWithValue("@accountHolder", cmeDoc.CustomerName);
                    cmdInsert.Parameters.AddWithValue("@patientName", cmeDoc.PatientName);
                    cmdInsert.Parameters.AddWithValue("@serviceAddress", cmeDoc.ServiceAddress);
                    cmdInsert.Parameters.AddWithValue("@timeSent", cmeDoc.TimeSent);
                    cmdInsert.Parameters.AddWithValue("@fileCreationTime", cmeDoc.FileCreationTime);
                    cmdInsert.Parameters.AddWithValue("@document", cmeDoc.Document);
                    cmdInsert.ExecuteNonQuery();
                }
                _con.Close();
                //Todo verify connection closed.
                return true;
            }
            catch (Exception ex) {
                Logger.AddError(Settings.ErrorLogfile, ex.Message);
                //Close the connection if it's not already closed.
                if (_con.State != ConnectionState.Closed)
                    _con.Close();
                return false;
            }
        }

        public bool Add(AP_Document doc) {
            switch (doc.DocumentType) {
                case DocumentType.DPA:
                    return AddDPARecord((DPA) doc);
                case DocumentType.CME:
                    return AddMedicalRecord((MedicalCME) doc);
            }

            return false;
        }
    }
}