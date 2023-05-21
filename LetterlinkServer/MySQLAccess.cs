using MySql.Data.MySqlClient;

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
        private const string inbox_folders = "inbox_folders";
        private readonly string[] defaultFolders = { "INBOX", "SENT", "Received", "Drafts", "Trash" };

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
                reader.Read();
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
                query = $"INSERT INTO {inbox_folders} (login,folder) VALUES ('{login}', 'Received'),('{login}','Trash'),('{login}','Drafts');";
                reader = new MySqlCommand(query, connection).ExecuteReader();
                reader.Close();

                return true;
            }
            else
                return false;
        }

        public bool CreateFolder(string login, string folder)
        {
            if (connection != null)
            {
                List<string> folders = GetInboxFolders(login);
                if (!folders.Contains(folder) && !folder.Equals("*"))
                {
                    string query = $"INSERT INTO {inbox_folders}(login,folder) VALUES ('{login}',''{folder}')";
                    MySqlDataReader reader = new MySqlCommand(query, connection).ExecuteReader();
                    reader.Close();
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }

        public List<string>? FetchSentLetters(string login)
        {
            if (connection != null)
            {
                List<string> letters = new List<string>();
                string query = $"SELECT uid FROM {sent_table} WHERE(login='{login}')";
                MySqlDataReader reader = new MySqlCommand(query, connection).ExecuteReader();
                while(reader.Read())
                    letters.Add(reader.GetString(0));
                reader.Close();
                return letters;
            }
            else
                return null;
        }

        public List<string>? FetchReceivedLetters(string login, string folder)
        {
            if(connection != null)
            {
                List<string> letters = new List<string>();
                string query;
                if (folder.Equals("*"))
                    query = $"SELECT uid FROM {inbox_table} WHERE(login='{login}')";
                else
                    query = $"SELECT uid FROM {inbox_table} WHERE(login='{login}',folder='{folder}')";
                MySqlDataReader reader = new MySqlCommand(query, connection).ExecuteReader();
                while(reader.Read())
                    letters.Add(reader.GetString(0));
                reader.Close();
                return letters;
            }
            else
                return null;
        }

        public List<string>? GetInboxFolders(string login)
        {
            if (connection != null)
            {
                List<string> folders = new List<string>();
                string query = "SELECT folder FROM " + inbox_folders + " WHERE(login='" + login + "');";
                MySqlDataReader reader = new MySqlCommand(query, connection).ExecuteReader();
                while (reader.Read())
                {
                    string folder = reader.GetString(0);
                    if(!folders.Contains(folder))
                        folders.Add(folder);
                }
                reader.Close();
                return folders;
            }
            else
                return null;
        }

        public List<string>? SearchLetters(string login, string folder)
        {
            if (connection != null)
            {
                List<string> letters = new List<string>();
                string query;
                if (folder.Equals("SENT"))
                    query = $"SELECT uid FROM {sent_table} WHERE(login='{login}');";
                else if (folder.Equals("INBOX"))
                    query = $"SELECT uid FROM {inbox_table} WHERE(login='{login}');";
                else
                    query = $"SELECT uid FROM {inbox_table} WHERE(login='{login}',folder='{folder}');";
                MySqlDataReader reader = new MySqlCommand(query, connection).ExecuteReader();
                while(reader.Read())
                    letters.Add(reader.GetString(0));
                reader.Close();
                return letters;
            }
            else
                return null;
        }

        public Dictionary<string, string>? GetLettersIDsFromFolder(string login, string folder)
        {
            if (connection != null)
            {
                Dictionary<string, string> ids = new Dictionary<string, string>();
                string query;
                if (folder.Equals("INBOX"))
                    query = $"SELECT uid FROM {inbox_table} WHERE(login='{login}');";
                else if (folder.Equals("SENT"))
                    query = $"SELECT uid FROM {sent_table} WHERE(login='{login}');";
                else
                    query = $"SELECT uid FROM {inbox_table} WHERE(login='{login}',folder='{folder}');";
                MySqlDataReader reader = new MySqlCommand(query, connection).ExecuteReader();
                int counter = 0;
                while (reader.Read())
                    ids.Add(Convert.ToString(++counter), reader.GetString(0));
                reader.Close();
                return ids;
            }
            else
                return null;
        } 

        public int[]? AddMessage(string sender, string recipient)
        {
            if(connection != null)
            {
                string query = "INSERT INTO " + sent_table + "(login, flags) VALUES ('" + sender + "','\\Recent');";
                MySqlDataReader reader = new MySqlCommand(query, connection).ExecuteReader();
                reader.Close();

                query = "INSERT INTO " + inbox_table + "(login, flags, folder) VALUES ('" + recipient + "','\\Recent','Received');";
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

        public bool DeleteFolder(string login, string folder)
        {
            if (connection != null && !defaultFolders.Contains(folder))
            {
                string query = $"DELETE FROM {inbox_folders} WHERE(login={login} AND folder='{folder}')";
                MySqlDataReader reader = new MySqlCommand(query, connection).ExecuteReader();
                reader.Close();
                return true;
            }
            else
                return false;
        }

        public List<string>? DeleteMarkedLetters(string login, string folder)
        {
            if (connection != null)
            {
                string query;
                List<string> deletedIDs = new List<string>();
                Dictionary<string, string>? allLetters = GetLettersIDsFromFolder(login, folder);
                if(allLetters != null)
                {
                    foreach(string localID in allLetters.Keys)
                        if (GetLetterFlags(allLetters[localID], login, folder, true).Contains("\\Deleted"))
                            deletedIDs.Add(localID);
                }

                if (folder.Equals("SENT"))
                    query = $"DELETE FROM {sent_table} WHERE(login='{login}' AND flags='\\Deleted')";
                else
                    query = $"DELETE FROM {inbox_table} WHERE(login='{login}' AND flags='\\Deleted')";
                MySqlDataReader reader = new MySqlCommand(query, connection).ExecuteReader();
                reader.Close();
                return deletedIDs;
            }
            else
                return null;
        }

        public bool ChangeLetterFlags(string id, string flag, string login, string folder, bool isUID)
        {
            Dictionary<string, string>? ids = GetLettersIDsFromFolder(login, folder);
            if (connection != null && ids != null)
            {
                string uid = isUID ? id : ids[id];
                string query;
                if (folder.Equals("SENT"))
                    query = $"UPDATE {sent_table} SET flags = '{flag} WHERE(login='{login}' AND uid='{uid}')'";
                else
                    query = $"UPDATE {inbox_table} SET flags = '{flag} WHERE(login='{login}' AND uid='{uid}')'";
                MySqlDataReader reader = new MySqlCommand(query, connection).ExecuteReader();
                reader.Close();
                return true;
            }
            else
                return false;
        }

        public string? GetLetterFlags(string id, string login, string folder, bool isUID)
        {
            Dictionary<string, string>? ids = GetLettersIDsFromFolder(login, folder);
            if (connection != null && ids != null)
            {
                string query;
                string uid = isUID ? id : ids[id];
                if (folder.Equals("SENT"))
                    query = $"SELECT flags FROM {sent_table} WHERE(uid='{uid}')";
                else
                    query = $"SELECT flags FROM {inbox_table} WHERE(uid='{uid}')";
                MySqlDataReader reader = new MySqlCommand(query, connection).ExecuteReader();
                reader.Read();
                string flag = reader.GetString(0);
                reader.Close();
                return flag;
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