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

        public bool Add(ref Work work) {
            try {
                _con.Open();
                //Todo Verify connection state is Open
                using (var cmdInsert = new OleDbCommand(strCommand, _con)) {
                    //Todo Fix KindOfDPA
                    cmdInsert.Parameters.AddWithValue("@dPAType", "Active");
                    cmdInsert.Parameters.AddWithValue("@delivery", work.DocObject.DeliveryMethod.ToString());
                    cmdInsert.Parameters.AddWithValue("@account", work.DocObject.Account);
                    cmdInsert.Parameters.AddWithValue("@sendTo", work.DocObject.SendTo);
                    cmdInsert.Parameters.AddWithValue("@customerName", work.DocObject.CustomerName);
                    cmdInsert.Parameters.AddWithValue("@timeSent", work.DocObject.TimeSent);
                    cmdInsert.Parameters.AddWithValue("@fileCreationTime", work.DocObject.FileCreationTime);
                    cmdInsert.Parameters.AddWithValue("@document", work.DocObject.Document);
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
    }
}