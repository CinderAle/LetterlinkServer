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
        private const string inbox_table = "inbox";
        private const string sent_table = "sent";

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
                bool isSuccess = reader.Read();
                reader.Close();
                return isSuccess;
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
                string actualPassword = reader.GetString(0);
                reader.Close();
                return password.Equals(actualPassword);
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
                reader.Close();
                return true;
            }
            else
                return false;
        }

        public int[]? AddMessage(string sender, string recipient)
        {
            if(connection != null)
            {
                string query = "INSERT INTO " + sent_table + "(login) VALUES ('" + sender + "');";
                MySqlDataReader reader = new MySqlCommand(query, connection).ExecuteReader();
                reader.Close();

                query = "INSERT INTO " + inbox_table + "(login) VALUES ('" + recipient + "');";
                reader = new MySqlCommand(query, connection).ExecuteReader();
                reader.Close();

                query = "SELECT uid FROM " + sent_table + " WHERE(login='" + sender + "')";
                reader = new MySqlCommand(query, connection).ExecuteReader();
                int senderID = 0;
                while(reader.Read())
                    senderID = reader.GetInt32(0);
                reader.Close();

                query = "SELECT uid FROM " + inbox_table + " WHERE(login='" + recipient + "')";
                reader = new MySqlCommand(query, connection).ExecuteReader();
                int recipientID = 0;
                while(reader.Read())
                    recipientID = reader.GetInt32(0);
                reader.Close();

                return new int[] { senderID , recipientID };
            }
            else
                return null;
        }

        public void Close()
        {
            if (connection != null)
                connection.Close();
            connection = null;
        }
    }
}
