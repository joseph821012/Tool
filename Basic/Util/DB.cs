using Basic.Conn;
using MySqlX.XDevAPI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Basic.Util
{
    public class DB
    {
        public string ErrorMsg { get; set; }
        public MySqlx conn = null;

        public DB()
        {
            if (conn == null)
                conn = new MySqlx(conn.SqlString());
        }

        public bool DBTrans_Start()
        {
            try
            {
                if (!conn.IsConnected)
                    throw new Exception(conn.ErrorMessage);

                string sql = "start transaction";
                int ret = conn.SQL_Exec(sql);

                return ret >= 0;
            }
            catch (Exception ex)
            {
                ErrorMsg = "交易模式啟動失敗:" + ex.Message;
                return false;
            }
        }

        public bool DBTrans_Rollback()
        {
            try
            {
                if (!conn.IsConnected)
                    throw new Exception(conn.ErrorMessage);

                string sql = "rollback";
                int ret = conn.SQL_Exec(sql);

                return ret >= 0;
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message;
                return false;
            }
        }

        public bool Get<T>(string TableName, string Condition, out T Model) where T : new()
        {
            try
            {
                Model = new T();
                DataTable dt = Get_DataTable(TableName, Condition, 0, 1);
                if (dt.Rows.Count <= 0)
                    throw new Exception(TableName + "資料庫找不到結果");
                Model = RowToModel<T>(dt.Rows[0]);
                return true;
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message;
                Model = new T();
                return false;
            }
        }

        public DataTable Get_DataTable(string table_name, string condition, int str = 0, int count = 0)
        {
            ErrorMsg = "";
            string limit = (str != 0 && count != 0) ? $" limit {str} ,{count} " : "";
            string sqlcmd = "select * from " + table_name;
            sqlcmd = (condition.Trim() != "") ? sqlcmd += " where " + condition : sqlcmd;
            return Get_DataTable(sqlcmd + limit);
        }

        public DataTable Get_DataTable(string sqlcmd)
        {
            if (!conn.IsConnected)
            {
                ErrorMsg = conn.ErrorMessage;
                return new DataTable();
            }
            return conn.Query(sqlcmd);
        }

        public int Get_RowCount(string table, string Where, string GroupNM = "")
        {
            if (!conn.IsConnected)
            {
                ErrorMsg = conn.ErrorMessage;
                return 0;
            }

            int count = 0;
            DataTable tmp;
            table = table.ToLower();
            Where = (string.IsNullOrEmpty(Where) == false) ? $" where {Where}" : "";
            if (table.Contains("left join"))
                count = conn.RowCount("Select count(1) from " + table + " " + Where + " GROUP BY " + GroupNM);            
            else
            {
                tmp = Get_DataTable("Select  count(1) from " + table + " " + Where);
                if (tmp.Rows.Count != 0) count = Converto.toInt(tmp.Rows[0][0].ToString());
            }
            return count;
        }
       
        public bool Insert<T>(T model)
        {
            ErrorMsg = "";
            try
            {
                return Insert_DataRow(ModelToRow(model));
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message;
                return false;
            }
        }
        
        public bool Insert_DataRow( DataRow row)
        {
            DataTable dt = new DataTable();
            dt.ImportRow(row);
            
            string sql_fields = "", sql_values = "";
            string name, value;
            for (int index = 0; index < row.ItemArray.Length; index++)
            {
                //INSERT INTO 表格名 (欄位1, 欄位2, ...)  VALUES('值1', '值2', ...) 
                name = row.Table.Columns[index].ColumnName;
                value = Converto.toString(row[name]);
                if (name != "" && value != "")
                {
                    if (sql_fields == "")
                    {
                        sql_fields = "(";
                        sql_values = "Values(";
                    }
                    else { sql_fields += ","; sql_values += ","; }
                    sql_fields += name;
                    sql_values += "'" + value + "'";
                }
            }
            if (sql_fields != "")
            {
                string SQL指令 = "Insert into " + dt.TableName + " " +
                           sql_fields + ") " + sql_values + ")";

                if (!conn.IsConnected)
                {
                    ErrorMsg = conn.ErrorMessage;
                    return false;
                }

                if (conn.SQL_Exec(SQL指令) > 0) return true;
                ErrorMsg = conn.ErrorMessage;
            }
            return false;
        }

        public bool Update_DataRow(string SQL指令搜尋條件, DataRow row)
        {
            DataTable dt = new DataTable();
            dt.ImportRow(row);

            string SQL指令 = "select * from " + dt.TableName;
            if (SQL指令搜尋條件.Trim() != "") SQL指令 += " where " + SQL指令搜尋條件;
            DataTable rec_tab = Get_DataTable(SQL指令);
            if (rec_tab.Rows.Count <= 0) return false;
            string name, value, old_val;
            //UPDATE "table_name" SET column_1 = [value1], column_2 = [value2] WHERE {SQL指令搜尋條件} 
            SQL指令 = "";
            DataColumnCollection columns = row.Table.Columns;
            for (int index = 0; index < columns.Count; index++)
            {
                name = columns[index].ColumnName;
                try
                {
                    object obj = row[name];
                    value = Converto.toString(obj);
                    old_val = Converto.toString(rec_tab.Rows[0][name]);
                }
                catch { continue; }
                if (name != "" && old_val != value)
                {
                    if (SQL指令 == "") SQL指令 = "SET ";
                    else SQL指令 += ", ";
                    SQL指令 += name + "='" + value + "'";
                }
            }
            if (SQL指令 != "")
            {
                SQL指令 = "UPDATE " + dt.TableName + " " + SQL指令;
                if (SQL指令搜尋條件.Trim() != "") SQL指令 += " where " + SQL指令搜尋條件;

                if (!conn.IsConnected)
                {
                    ErrorMsg = conn.ErrorMessage;
                    return false;
                }

                if (conn.SQL_Exec(SQL指令) <= 0) return false;
            }
            return true;
        }

        public bool Delete_Record(string table_name, string SQL指令搜尋條件)
        {
            string SQL指令 = "delete from " + table_name;
            if (SQL指令搜尋條件.Trim() != "") SQL指令 += " where " + SQL指令搜尋條件;

            if (!conn.IsConnected)
            {
                ErrorMsg = conn.ErrorMessage;
                return false;
            }

            bool result = conn.SQL_Exec(SQL指令) > 0;
            return result;
        }

        public int SQL_Exec(string sqlCmd)
        {
            return conn.SQL_Exec(sqlCmd);
        }

        public  T RowToModel<T>(DataRow row) where T : new()
        {
            // create a new object
            T Model = new T();
            // set the item
            foreach (DataColumn c in row.Table.Columns)
            {
                // find the property for the column
                PropertyInfo p = Model.GetType().GetProperty(c.ColumnName);

                // if exists, set the value
                if (p != null && row[c] != DBNull.Value)
                {
                    p.SetValue(Model, Convert.ChangeType(row[c], p.PropertyType), null);
                }
            }
            return Model;
        }

        public DataRow ModelToRow<T>(T model)
        {
            var tb = new DataTable(typeof(T).Name);

            PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in props)
            {
                Type t = GetCoreType(prop.PropertyType);
                tb.Columns.Add(prop.Name, t);
            }


            var values = new object[props.Length];

            for (int i = 0; i < props.Length; i++)
            {
                values[i] = props[i].GetValue(model, null);
            }

            tb.Rows.Add(values);


            return tb.Rows[0];
        }

        public static bool IsNullable(Type t)
        {
            return !t.IsValueType || (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        public static Type GetCoreType(Type t)
        {
            if (t != null && IsNullable(t))
            {
                if (!t.IsValueType)
                {
                    return t;
                }
                else
                {
                    return Nullable.GetUnderlyingType(t);
                }
            }
            else
            {
                return t;
            }
        }
    }
}
