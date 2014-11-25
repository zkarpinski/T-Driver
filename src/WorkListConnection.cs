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
                using (var cmdInsert = new OleDbCommand(strCommand, _con)) {
                    cmdInsert.Parameters.AddWithValue("@dPAType", work.KindOfDPA);
                    cmdInsert.Parameters.AddWithValue("@delivery", work.DPAObject.DeliveryMethod.ToString());
                    cmdInsert.Parameters.AddWithValue("@account", work.DPAObject.Account);
                    cmdInsert.Parameters.AddWithValue("@sendTo", work.DPAObject.SendTo);
                    cmdInsert.Parameters.AddWithValue("@customerName", work.DPAObject.CustomerName);
                    cmdInsert.Parameters.AddWithValue("@timeSent", work.DPAObject.TimeSent);
                    cmdInsert.Parameters.AddWithValue("@fileCreationTime", work.DPAObject.FileCreationTime);
                    cmdInsert.Parameters.AddWithValue("@document", work.DPAObject.Document);
                    cmdInsert.ExecuteNonQuery();
                }
                _con.Close();
                return true;
            }
            catch (Exception ex) {
                Logger.AddError(Settings.ErrorLogfile, ex.Message);
                return false;
            }
        }
    }
}