using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic.Conn
{
    public class MySqlx
    {
        MySqlConnection db_connection = null;
        public string ErrorMessage { get; set; }
        public bool IsConnected { get; set; }

        public MySqlx()
        {
            ErrorMessage = "";
            IsConnected = dbOpen(SqlString());
        }

        public MySqlx(string ConnecString)
        {
            ErrorMessage = "";
            IsConnected = dbOpen(ConnecString);
        }

        public bool dbOpen(string ConnecString)
        {
            try
            {
                ErrorMessage = "";
                IsConnected = false;

                string connectStr = ConnecString;
                db_connection = new MySqlConnection(connectStr);
                if (db_connection.State == ConnectionState.Closed)
                {
                    db_connection.Open();
                    IsConnected = true;
                }
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                dbClose();
            }
            return db_connection != null;
        }
        public void dbClose()
        {
            if (db_connection != null)
                db_connection.Close();
            db_connection = null;
            IsConnected = false;
        }

        public DataTable Query(string SqlCmd)
        {
            DataTable dt = new DataTable();
            MySqlCommand cmd = SQLCommand(SqlCmd);
            try
            {
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    dt = new DataTable();
                    dt.Load(reader);
                }
                else
                    dt = TableNoRow(reader);

                reader.Close();
                cmd.Dispose();
            }
            catch (Exception e)
            {
                ErrorMessage = e.ToString();
            }

            return dt;

        }
        public int Insert(string SqlCmd)
        {
            return SQL_Exec(SqlCmd);
        }
        public int Update(string SqlCmd)
        {
            return SQL_Exec(SqlCmd);
        }
        public int Delete(string SqlCmd)
        {
            return SQL_Exec(SqlCmd);
        }
        public int RowCount(string SqlCmd)
        {
            int rec_count = 0;
            try
            {
                MySqlCommand cmd = SQLCommand(SqlCmd);
                MySqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    rec_count++;
                }
                dr.Close();
                cmd.Dispose();
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
            }
            return rec_count;
        }
        public int SQL_Exec(string SqlCmd)
        {
            try
            {
                MySqlCommand cmd = SQLCommand(SqlCmd);
                int record_count = cmd.ExecuteNonQuery();
                cmd.Dispose();
                return record_count;
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
            }
            return 0;
        }

        public MySqlCommand SQLCommand(string SqlCmd)
        {
            if (db_connection == null) return null;
            MySqlCommand cmd = new MySqlCommand(SqlCmd, db_connection);
            return cmd;
        }

        public DataTable TableNoRow(MySqlDataReader dr)
        {
            DataTable table = new DataTable();
            for (int index = 0; index < dr.FieldCount; index++)
            {
                table.Columns.Add(dr.GetName(index));
            }
            return table;
        }

        public  string strConnection { get; set; }

        public  string _account = "dev01", _password = "Goflow2022", _dbName = "erp_release", _ip = "59.125.31.88", _port = "1653";

        public string SqlString(string account ="" , string password ="", string dbName ="", string ip ="", string port = "")
        {
            account = (string.IsNullOrEmpty(account)) ? _account : account;
            password = (string.IsNullOrEmpty(password)) ? _password : password;
            dbName = (string.IsNullOrEmpty(dbName)) ? _dbName : dbName;
            ip = (string.IsNullOrEmpty(ip)) ? _ip : ip;
            port = (string.IsNullOrEmpty(port)) ? _port : port;
            strConnection = $"server={ip};port={port};database={dbName};user={account};password={password}";
            return strConnection;
        }
    }
}
