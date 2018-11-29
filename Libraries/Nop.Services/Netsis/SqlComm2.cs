// Atilla KayaNopCommerceNop.ServicesSqlComm2.cs

using System.Data;
using System.Data.SqlClient;

namespace Nop.Services.Netsis
{
    public partial class SqlComm2
    {
        // this is a shortcut for your connection string
        //static string DatabaseConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["nopConnectionString"].ConnectionString;
        static string DatabaseConnectionString = "Data Source=85.99.244.100,1433;Initial Catalog=ATAK2018;UID=sa;password=PROJE;;Connection Timeout=600";
        //static string DatabaseConnectionString =  @"Persist Security info=true; integrated Security=false; Initial Catalog=nop; Server=LENOVO-LENOVO\MSSQLSERVER; UID=sa; PWD=sa2012;";
        // this is for just executing sql command with no value to return
        /// <summary>
        /// SqlExecute
        /// </summary>
        /// <param name="sql"></param>
        public static void SqlExecute(string sql)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseConnectionString))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // with this you will be able to return a value
        /// <summary>
        /// SqlReturn
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static object SqlReturn(string sql)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                object result = (object)cmd.ExecuteScalar();
                return result;
            }
        }

        // with this you can retrieve an entire table or part of it
        /// <summary>
        /// SqlDataTable
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static DataTable SqlDataTable(string sql)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseConnectionString))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Connection.Open();
                DataTable tempTable = new DataTable();
                tempTable.Columns.Add("Stock");
                tempTable.Columns.Add("ProductName");
                tempTable.Columns.Add("Price");
                tempTable.Columns.Add("StockCode");
                SqlDataReader dr = cmd.ExecuteReader();
                //tempTable.Load(dr);
                while (dr.Read())
                {
                    tempTable.Rows.Add(dr[0], dr[1],dr[2],dr[3]);
                }
                return tempTable;
            }
        }

        // sooner or later you will probably use stored procedures. 
        // you can use this in order to execute a stored procedure with 1 parameter
        // it will work for returning a value or just executing with no returns
        /// <summary>
        /// SqlStoredProcedure1Param
        /// </summary>
        /// <param name="StoredProcedure"></param>
        /// <param name="PrmName1"></param>
        /// <param name="Param1"></param>
        /// <returns></returns>
        public static object SqlStoredProcedure1Param(string StoredProcedure, string PrmName1, object Param1)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseConnectionString))
            {
                SqlCommand cmd = new SqlCommand(StoredProcedure, conn) {CommandType = CommandType.StoredProcedure};
                cmd.Parameters.Add(new SqlParameter(PrmName1, Param1.ToString()));
                cmd.Connection.Open();
                object obj;
                obj = cmd.ExecuteScalar();
                return obj;
            }
        }


    }
}