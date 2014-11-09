using System.Data;
using System.Data.OleDb;

namespace TDriver {
    internal class WorkListConnection {

        private static OleDbConnection _con;

        private const string strCommand =
            @"INSERT INTO DeferredPaymentAgreements (DPAType,DeliveryMethod,AccountNumber,SentTo,CustomerName,SentTime,FileCreated,File)" +
            "VALUES (@dPAType,@delivery,@account,@sendTo,@customerName,@timeSent,@fileCreationTime,@document)";
        public WorkListConnection(string databaseFile) {
            _con = new OleDbConnection(
                @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + databaseFile);
        }

        public bool Add(ref Work work) {
            _con.Open();
                using (OleDbCommand cmdInsert = new OleDbCommand(strCommand,_con)) {
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

            
            /*
            //Todo Make Insert cleaner using .Add
            // http://stackoverflow.com/questions/19956533/sql-insert-query-using-c-sharp
            cmdInsert.CommandText += " VALUES (";
            cmdInsert.CommandText += "'" + work.KindOfDPA + "',";
            cmdInsert.CommandText += "'" + work.DPAObject.DeliveryMethod + "',";
            cmdInsert.CommandText += "'" + work.DPAObject.Account + "',";
            cmdInsert.CommandText += "'" + work.DPAObject.SendTo + "',";
            cmdInsert.CommandText += "'" + work.DPAObject.CustomerName + "',";
            cmdInsert.CommandText += "'" + work.DPAObject.TimeSent + "',";
            cmdInsert.CommandText += "'" + work.DPAObject.FileCreationTime + "',";
            cmdInsert.CommandText += "'" + work.DPAObject.Document + "')";
            _con.Open();

            cmdInsert.CommandType = CommandType.Text;

            cmdInsert.Connection = _con;
            
            cmdInsert.ExecuteNonQuery();
            _con.Close();
             * */
            return true;
        }
    }
}