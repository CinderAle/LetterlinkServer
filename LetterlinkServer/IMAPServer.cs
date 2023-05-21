using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Cms;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Xml.Linq;

namespace LetterlinkServer
{
    public class IMAPServer : Server
    {
        private string login;
        private string folder;
        private bool isAuthenticated;
        private bool isRunning;
        private List<string> unflaggedIDs;
        private HashSet<string> unauthorizedActions;
        private readonly string[] supportedFlags = { "\\Seen", "\\Answered", "\\Flagged", "\\Deleted", "\\Draft", "\\Recent"};
        TcpListener listener;

        public IMAPServer(TcpListener listener)
        {
            initActions();
            initUnauthorizedActions();
            unflaggedIDs = new List<string>();
            this.listener = listener;
        }

        protected override async void writeClient(string message)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(message + "\r\n");
            await clientStream.WriteAsync(bytes, 0, bytes.Length);
            Console.WriteLine($"[IMAP] Server: '{message}'");
        }

        protected override string readClient()
        {
            StreamReader reader = new StreamReader(clientStream);
            string message = reader.ReadLine();
            return message;
        }

        private void clearContext()
        {
            folder = string.Empty;
            isAuthenticated = false;
            unflaggedIDs.Clear();
        }

        public override async void startServer(object? ctsObject)
        {
            CancellationToken cts = (CancellationToken)ctsObject;
            while (true)
            {
                clearContext();
                try
                {
                    TcpClient client = await listener.AcceptTcpClientAsync(cts);
                    this.client = client;
                    clientStream = client.GetStream();
                    handleMessages();
                }
                catch (OperationCanceledException)
                {
                    if (client != null)
                        client.Close();
                    break;
                }
                catch (Exception)
                {
                    if (client != null)
                        client.Close();
                }
            }
        }

        protected override bool chooseAction(string? message)
        {
            string command = string.Empty;

            foreach(string method in supportedActions.Keys)
                if (message != null && (message.Substring(10).StartsWith(method) || message.StartsWith(method)))
                {
                    command = method; 
                    break;
                }
            if (!command.Equals(string.Empty) && message != null && (unauthorizedActions.Contains(command) || isAuthenticated))
            {
                supportedActions[command].Invoke(message);
                return !command.Equals("LOGOUT");
            }
            else
                return true; 
        }

        protected override async void handleMessages()
        {
            Console.WriteLine("Client connected");
            writeClient("* OK IMAP4 server ready.");
            

            while (true)
            {
                string message;
                try
                {
                    message = readClient();
                }
                catch (Exception)
                {
                    break;
                }
                Console.WriteLine("[IMAP] Client: " + message.Trim());
                if (!chooseAction(message))
                    break;
            }

            if (client != null)
                client.Close();
            Console.WriteLine("[IMAP] Client disconnected");
        }
        
        protected override void initActions()
        {
            supportedActions = new Dictionary<string, Action<string>>();
            supportedActions.Add("CAPABILITY", CAPABILITY);
            supportedActions.Add("NOOP", NOOP);
            supportedActions.Add("LOGOUT", LOGOUT);
            supportedActions.Add("AUTHENTICATE", AUTHENTICATE);
            supportedActions.Add("LOGIN", LOGIN);
            supportedActions.Add("SELECT", SELECT);
            supportedActions.Add("CREATE", CREATE);
            supportedActions.Add("DELETE", DELETE);
            supportedActions.Add("SUBSCRIBE", SUBSCRIBE);
            supportedActions.Add("UNSUBSCRIBE", UNSUBSCRIBE);
            supportedActions.Add("LIST", LIST);
            supportedActions.Add("EXPUNGE", EXPUNGE);
            supportedActions.Add("SEARCH", SEARCH);
            supportedActions.Add("FETCH", FETCH);
            supportedActions.Add("STORE", STORE);
            supportedActions.Add("UID", UID);
        }

        private void initUnauthorizedActions()
        {
            unauthorizedActions = new HashSet<string>();
            unauthorizedActions.Add("NOOP");
            unauthorizedActions.Add("CAPABILITY");
            unauthorizedActions.Add("LOGOUT");
            unauthorizedActions.Add("AUTHENTICATE");
            unauthorizedActions.Add("LOGIN");

        }

        private void CAPABILITY(string message)
        {
            writeClient("* CAPABILITY IMAP4rev1 AUTH=PLAIN AUTH=LOGIN");
            writeClient(message.Substring(0, 10) + "OK completed");
        }

        private void NOOP(string message)
        {
            writeClient("* OK");
        }

        private void LOGOUT(string message)
        {
            writeClient(message.Substring(0, 10) + "BYE letterlink");
        }

        private bool checkAuth(string login, string password)
        {
            TcpClient logger = new TcpClient();
            logger.Connect("localhost", 85);
            StreamReader reader = new StreamReader(logger.GetStream());
            StreamWriter writer = new StreamWriter(logger.GetStream());
            try
            {
                if (!reader.ReadLine().StartsWith("220"))
                    throw new Exception();
                writer.WriteLine($"LOG {login} {password}");
                writer.Flush();
                string answer = reader.ReadLine();
                logger.Close();
                return answer.StartsWith("250");
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void authPlain(string code)
        {
            writeClient("+");
            string userPassword = readClient();
            string[] logs = Encoding.ASCII.GetString(Convert.FromBase64String(userPassword)).Substring(1).Split('\0');
            Console.WriteLine($"User:{logs[0]} Password:{logs[1]}");
            if (logs.Length == 2)
                tryAuthenticate(logs[0], logs[1], code);
            else
                writeClient(code + "NO failed to authenticate");
        }

        private void authLogin(string code)
        {
            writeClient("+");
            string username = Encoding.ASCII.GetString(Convert.FromBase64String(readClient()));
            writeClient("+");
            string password = Encoding.ASCII.GetString(Convert.FromBase64String(readClient()));
            Console.WriteLine($"User: {username} Password: {password}");
            tryAuthenticate(username, password, code);
        }

        private void tryAuthenticate(string login, string password, string code)
        {
            try
            {
                isAuthenticated = checkAuth(login, password);
                if (isAuthenticated)
                {
                    writeClient(code + "OK authentication complete");
                    this.login = login;
                }
                else
                    throw new Exception();
            }
            catch (Exception)
            {
                writeClient(code + "No failed to authenticate");
            }
        }

        private void AUTHENTICATE(string message)
        {
            if (message.Contains("PLAIN"))
                authPlain(message.Substring(0, 10));      
            else if (message.Contains("LOGIN"))
                authLogin(message.Substring(0, 10));
            else
                writeClient(message.Substring(0, 10) + "NO the AUTH method is not available");
        }

        private void LOGIN(string message)
        {
            string[] logs = message.Split(' ');
            if (logs.Length == 4)
                tryAuthenticate(logs[2], logs[3], message.Substring(0, 10));
            else
                writeClient(message.Substring(0, 10) + "NO failed to authenticate");
        }

        private void SELECT(string message)
        {
            try
            {
                int startFolder = message.IndexOf('\"') + 1;
                string folder = message.Substring(startFolder);
                List<string> folders = getInboxFolders();
                if (folders.Contains(folder) || folder.Equals("INBOX") || folder.Equals("SENT"))
                {
                    writeClient(message.Substring(0, 10) + "OK [READ-WRITE] SELECT completed");
                    this.folder = folder;
                }
                else
                    throw new Exception();
            }
            catch (Exception)
            {
                writeClient(message.Substring(0, 10) + "NO unable to select the folder");
            }
        }

        private void CREATE(string message)
        {
            try
            {
                int startFolder = message.IndexOf('\"') + 1;
                string folder = message.Substring(startFolder);
                List<string> folders = getInboxFolders();
                if (folders.Contains(folder) || folder.Equals("INBOX") || folder.Equals("SENT"))
                {
                    throw new Exception();
                }
                else
                {
                    MySQLAccess database = new MySQLAccess();
                    if (database.CreateFolder(this.login, folder))
                        writeClient(message.Substring(0, 10) + "OK folder created");
                    else
                        throw new Exception();
                    database.Close();
                }

            }
            catch (Exception)
            {
                writeClient(message.Substring(0, 10) + "NO unable to create the folder");
            }
        }

        private void DELETE(string message)
        {
            MySQLAccess database = new MySQLAccess();
            try
            {
                string folder = message.Substring(message.IndexOf('\"') + 1);
                folder = folder.Substring(0, folder.Length - 1);
                if (database.DeleteFolder(this.login, folder))
                    writeClient("A00000001 OK DELETE completed.");
                else
                    throw new Exception();
            }
            catch (Exception)
            {
                writeClient("A00000001 NO failed to DELETE the folder.");
            }
            finally
            {
                database.Close();
            }
        }

        private void SUBSCRIBE(string message)
        {

        }

        private void UNSUBSCRIBE(string message)
        {

        }

        private string[]? getFolderInfo(string message)
        {
            string[] info = new string[2];
            int commaStart = message.IndexOf('\"');
            int commaEnd = message.IndexOf('\"', commaStart + 1);
            if(commaStart > 0 && commaEnd > 0)
            {
                info[0] = message.Substring(commaStart + 1, commaEnd - commaStart - 1);
                commaStart = message.IndexOf('\"', commaEnd + 1);
                commaEnd = message.IndexOf('\"', commaStart + 1);
                if (commaStart > 0 && commaEnd > 0)
                {
                    info[1] = message.Substring(commaStart + 1, commaEnd - commaStart - 1);
                    return info;
                }
                else
                    return null;
            }
            else
                return null;
        }

        private List<string>? getInboxFolders()
        {
            MySQLAccess database = new MySQLAccess();
            List<string>? folders = database.GetInboxFolders(this.login);
            database.Close();
            return folders;
        }

        private void writeFolderListToClient(List<string>? list)
        {
            if (list != null)
                foreach (string item in list)
                    writeClient($"* LIST (\\HasNoChildren) \"/\" \"{item}\"");
        }

        private void LIST(string message)
        {
            string[]? info = getFolderInfo(message);
            if (info != null)
            {
                if (info[1].Equals("INBOX") || info[1].Equals("INBOX/*"))
                {
                    writeFolderListToClient(getInboxFolders());
                    writeClient(message.Substring(0, 10) + "OK LIST completed");
                }
                else if (info[1].Equals("*") || info[1].Equals(string.Empty))
                {
                    writeClient("* LIST (\\Noselect) \"/\" \"\"");
                    writeClient("* LIST (\\HasChildren) \"/\" \"INBOX\"");
                    writeClient("* LIST (\\HasNoChildren) \"/\" \"SENT\"");
                    writeClient(message.Substring(0, 10) + "OK LIST completed");
                }
                else
                    writeClient(message.Substring(0, 10) + "OK LIST completed");
            }
            else
                writeClient(message.Substring(0, 10) + "BAD invalid arguments");
        }

        private void EXPUNGE(string message)
        {
            MySQLAccess database = new MySQLAccess();
            try
            {
                List<string>? ids = database.DeleteMarkedLetters(this.login, this.folder);
                if (ids != null)
                {
                    foreach (string id in ids)
                        writeClient($"* {id} EXPUNGE");
                    writeClient(message.Substring(0, 10) + "OK EXPUNGE completed.");
                }
                else
                    throw new Exception();
            }
            catch (Exception)
            {
                writeClient(message.Substring(0, 10) + "NO EXPUNGE failed.");
            }
            finally
            {
                database.Close();
            }
        }

        private void searchUIDs()
        {
            try
            {
                MySQLAccess database = new MySQLAccess();
                if (!this.login.Equals(string.Empty) && !this.folder.Equals(string.Empty))
                {
                    List<string>? letters = database.SearchLetters(this.login, this.folder);
                    if (letters != null)
                    {
                        string uids = string.Empty;
                        foreach (string letter in letters)
                            uids += letter + ' ';
                        uids = uids.Trim();
                        writeClient($"* SEARCH {uids}");
                    }
                    else
                        throw new Exception();
                }
                else
                    throw new Exception();
                database.Close();
            }
            catch (Exception)
            {
                writeClient("* SEARCH");
            }
        }

        private void SEARCH(string message)
        {
            if(message.Contains("SEARCH ALL"))
            {
                searchUIDs();
            }
        }

        private string? fetchLetterFlags(string id, bool isUID)
        {
            MySQLAccess database = new MySQLAccess();
            string? flag = database.GetLetterFlags(id, this.login, this.folder, isUID);
            database.Close();
            return flag; 
        }

        private string? fetchLetterBody(string id, bool isUID)
        {
            if (!isUID)
            {
                MySQLAccess database = new MySQLAccess();
                Dictionary<string, string>? ids = database.GetLettersIDsFromFolder(this.login, this.folder);
                string? contents = ids != null && ids.ContainsKey(id) ? fetchLetterBody(ids[id], true) : null;
                database.Close();
                return contents;
            }
            else
            {
                string path;
                if (this.folder.Equals("SENT"))
                    path = $"sent/{id}.txt";
                else
                    path = $"inbox/{id}.txt";
                return File.ReadAllText(path);
            }
        }

        private void FETCH(string message)
        {
            try
            {
                string[] splits = message.Split(' ');
                string id = splits[1];
                string modifiersLine = message.Substring(message.IndexOf('(') + 1);
                string[] modifiers = modifiersLine.Substring(0, modifiersLine.Length - 1).Split(' ');
                string answer = string.Empty;
                if (modifiers.Contains("FLAGS"))
                {
                    string? flags = fetchLetterFlags(id, false);
                    if (flags != null)
                        answer += $"FLAGS ({flags}) ";
                    else
                        throw new Exception();
                }
                if (modifiers.Contains("BODY[]")) {
                    string? contents = fetchLetterBody(id, false);
                    if (contents != null)
                        answer += "BODY[] {" + contents.Length + "}\r\n" + contents;
                    else
                        throw new Exception();
                }
                writeClient($"* {id} FETCH ({answer})");   
            }
            catch (Exception)
            {
                writeClient("* NO could not fetch the letter");
            }
        }

        private void resetUIDFlags(string uid, string toFlag)
        {
            MySQLAccess database = new MySQLAccess();
            try
            {
                if (!database.ChangeLetterFlags(uid, toFlag, this.login, this.folder, true))
                    throw new Exception();
            }
            catch (Exception)
            {
                writeClient("A00000001 NO colud not change flags");
            }
            finally
            {
                database.Close();
            }
        }

        private void resetFlags(int from, int to, string toFlag)
        {
            bool areUnflagged = true;
            for (int i = from; i <= to && areUnflagged; i++)
                unflaggedIDs.Contains(Convert.ToString(i));
            if (areUnflagged)
            {
                MySQLAccess database = new MySQLAccess();
                bool isOK = true;
                for (int i = from; i <= to && isOK; i++)
                {
                    if (!database.ChangeLetterFlags(i.ToString(), toFlag, this.login, this.folder, false))
                    {
                        if (i > from)
                            writeClient($"A00000001 OK flags changed for {from}:{i - 1}");
                        else
                            writeClient($"A00000001 NO could not change flags.");
                        isOK = false;
                    }  
                }
                if (isOK)
                    writeClient($"A00000001 OK flags changed for {from}:{to}");
                database.Close();
            }
            else
                writeClient("A00000001 NO not all flags unset.");
        }

        private void unflagLetters(int idStart, int idEnd)
        {
            MySQLAccess database = new MySQLAccess();
            Dictionary<string, string>? IDs = database.GetLettersIDsFromFolder(this.login, this.folder);
            database.Close();

            string start = Convert.ToString(idStart);
            bool hasID = IDs.ContainsKey(idStart.ToString());
            while (idStart <= idEnd && database != null && hasID)
            {
                hasID = IDs.ContainsKey(idStart.ToString());
                if(hasID)
                    unflaggedIDs.Add(IDs[idStart.ToString()]);
                idStart++;
            }
            if (hasID)
                writeClient($"A00000001 OK Flags unset for messages {start}:{idEnd}");
            else
                writeClient($"A00000001 OK Flags unset for messages {start}:{idStart - 1}");
        }

        private void unflagUIDLetters(string uid)
        {
            MySQLAccess database = new MySQLAccess();
            Dictionary<string, string>? IDs = database.GetLettersIDsFromFolder(this.login, this.folder);
            database.Close();
            if (IDs.ContainsValue(uid))
            {
                unflaggedIDs.Add(uid);
                writeClient($"A00000001 OK Flags set for message {uid}");
            }
            else
                writeClient("A00000001 NO colud not set the flags.");
        }

        private void STORE(string message)
        {
            string[] words = message.Split(' ');
            try
            {
                int idStart, idEnd;
                if (words[1].Contains(':'))
                {
                    int separator = words[1].IndexOf(':');
                    idStart = Convert.ToInt32(words[1].Substring(0, separator));
                    idEnd = Convert.ToInt32(words[1].Substring(separator + 1));
                }
                else
                {
                    idStart = Convert.ToInt32(words[1]);
                    idEnd = idStart;
                }
                if (words[2].Equals("+FLAGS"))
                {
                    resetFlags(idStart, idEnd, words[3].Substring(1, words[3].Length - 2));
                }
                else if (words[2].Equals("-FLAGS"))
                {
                    unflagLetters(idStart, idEnd);
                }
                else
                    throw new Exception();
            }
            catch (Exception)
            {
                writeClient("A00000001 NO wrong syntax.");
            }
        }

        private string? getLocalID(string uid)
        {
            MySQLAccess database = new MySQLAccess();
            Dictionary<string, string> ids = database.GetLettersIDsFromFolder(this.login, this.folder);
            database.Close();
            string? local = null;
            foreach(string key in ids.Keys)
                if (ids[key].Equals(uid))
                {
                    local = key;
                    break;
                }
            return local;
        }

        private void uidFetch(string message)
        {
            try
            {
                string[] splits = message.Split(' ');
                string id = splits[2];
                string localID = getLocalID(id);
                if (localID == null)
                    throw new Exception();
                string modifiersLine = message.Substring(message.IndexOf('(') + 1);
                string[] modifiers = modifiersLine.Substring(0, modifiersLine.Length - 1).Split(' ');
                string answer = string.Empty;
                if (modifiers.Contains("FLAGS"))
                {
                    string? flags = fetchLetterFlags(id, true);
                    if (flags != null)
                        answer += $"FLAGS ({flags}) ";
                    else
                        throw new Exception();
                }
                if (modifiers.Contains("BODY[]"))
                {
                    string? contents = fetchLetterBody(id, true);
                    if (contents != null)
                        answer += "BODY[] {" + contents.Length + "}\r\n" + contents;
                    else
                        throw new Exception();
                }
                writeClient($"* {localID} FETCH (UID {id} {answer})");
            }
            catch (Exception)
            {
                writeClient("* NO could not fetch the letter");
            }
        }

        private void UID(string message)
        {
            string[] words = message.Split(' ');
            try
            {
                if (words[1].Equals("STORE"))
                {
                    if (words[3].Equals("+FLAGS"))
                    {
                        resetUIDFlags(words[2], words[4].Substring(1, words[4].Length - 2));
                    }
                    else if (words[3].Equals("-FLAGS"))
                    {
                        unflagUIDLetters(words[2]);
                    }
                    else
                        throw new Exception();
                }
                else if (words[1].Equals("FETCH"))
                {
                    uidFetch(message);
                }
                else
                    throw new Exception();
            }
            catch (Exception)
            {
                writeClient("A00000001 BAD wrong syntax.");
            }
        }
    }
}