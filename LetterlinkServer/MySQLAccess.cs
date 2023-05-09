using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1;

namespace LetterlinkServer
{
    public class MySQLAccess
    {
        private MySqlConnection? connection;

        private const string host = "localhost";
        private const string username = "root";
        private const string password = "880461";
        private const string database = "letterlink_db";
        private const string logs_table = "users_logs";

        public MySQLAccess()
        {
            try
            {
                connection = new MySqlConnection($"Server={host}; database={database}; UID={username}; password={password}");
                connection.Open();
            }
            catch (Exception)
            {
                connection = null;
            }
        }

        public bool CheckLogin(string login)
        {
            if (connection != null)
            {
                string query = "SELECT login FROM " + logs_table + " WHERE(login='" + login + "');";
                MySqlDataReader reader = new MySqlCommand(query, connection).ExecuteReader();
                return reader.Read();
            }
            else
                return false;
        }

        public bool CheckPassword(string login, string password)
        {
            if (connection != null && CheckLogin(login))
            {
                string query = "SELECT password FROM " + logs_table + " WHERE(login='" + login + "');";
                MySqlDataReader reader = new MySqlCommand(query, connection).ExecuteReader();
                return password.Equals(reader.GetString(0));
            }
            else
                return false;
        }

        public bool InsertUser(string login, string password)
        {
            if (connection != null && !CheckLogin(login))
            {
                string query = "INSERT INTO " + logs_table + "(login,password) VALUES ('" + login + "','" + password + "');";
                MySqlDataReader reader = new MySqlCommand(query, connection).ExecuteReader();
                return reader.GetString(0).Length > 0;
            }
            else
                return false;
        }

        public void Close()
        {
            if (connection != null)
                connection.Close();
            connection = null;
        }
    }
}
