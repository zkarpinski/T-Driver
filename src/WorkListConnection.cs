using System;
using System.Data.OleDb;

namespace TDriver {
    internal class WorkListConnection {
        private const string strCommand =
            @"INSERT INTO DeferredPaymentAgreements (DPAType,DeliveryMethod,AccountNumber,SentTo,CustomerName,SentTime,FileCreated,File)" +
            "VALUES (@dPAType,@delivery,@account,@sendTo,@customerName,@timeSent,@fileCreationTime,@document)";

        private static OleDbConnection _con;

        public WorkListConnection(string databaseFile) {
            _con = new OleDbConnection(
                @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + databaseFile);
        }

        public bool Add(AP_Document doc) {
            if (doc.DocumentType == DocumentType.DPA) {
                DPA dpaDoc = (DPA)doc;
                try {
                    _con.Open();
                    //Todo Verify connection state is Open
                    using (var cmdInsert = new OleDbCommand(strCommand, _con)) {
                        //Todo Fix KindOfDPA
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
                    if (_con.State != System.Data.ConnectionState.Closed)
                        _con.Close();
                    return false;
                }
            }
            else { return false; }
        }
    }
}