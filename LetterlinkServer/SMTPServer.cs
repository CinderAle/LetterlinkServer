using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LetterlinkServer
{
    public class SMTPServer : Server
    {
        private const int commandLength = 4;
        private string sender;
        private string recipient;
        private bool isAuthenticated;
        private HashSet<string> unauthorizedActions;
        TcpListener listener;

        public SMTPServer(TcpListener listener)
        {
            initActions();
            initUnauthorizedActions();
            this.listener= listener;
        }

        private void clearContext()
        {
            sender = string.Empty;
            recipient = string.Empty;
            isAuthenticated = false;
        }

        protected override async void writeClient(string message)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(message + "\r\n");
            await clientStream.WriteAsync(bytes, 0, bytes.Length);
            Console.WriteLine($"[SMTP] Server: '{message}'");
        }

        protected override string readClient()
        {
            StreamReader reader = new StreamReader(clientStream);
            string message = reader.ReadLine();
            return message;
        }

        public override async void startServer(object? ctsObject)
        {
            CancellationToken cts = (CancellationToken) ctsObject;
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
            string command = (message != null) && message.Length >= commandLength ? message.Substring(0, commandLength) : string.Empty;
            if (supportedActions.ContainsKey(command) && message != null) {
                //if (unauthorizedActions.Contains(command) || isAuthenticated)
                //{
                    supportedActions[command].Invoke(message);
                    return !command.Equals("QUIT");
                //}
                /*else
                {
                    writeClient("535 Authentication required");
                    return true;
                }*/
            }
            else
            {
                writeClient("502 Command not supported");
                return true;
            }
        }

        protected override async void handleMessages()
        {
            Console.WriteLine("[SMTP] Client connected");
            writeClient("220 localhost");

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
                if(message != null)
                    Console.WriteLine("[SMTP] Client: " + message.Trim());
                if (!chooseAction(message))
                    break;
            }

            if(client != null)
                client.Close();
            Console.WriteLine("[SMTP] Client disconnected");
        }

        protected override void initActions()
        {
            supportedActions = new Dictionary<string, Action<string>>();
            supportedActions.Add("HELO", HELO);
            supportedActions.Add("EHLO", EHLO);
            supportedActions.Add("VRFY", VRFY);
            supportedActions.Add("EXPN", EXPN);
            supportedActions.Add("HELP", HELP);
            supportedActions.Add("NOOP", NOOP);
            supportedActions.Add("QUIT", QUIT);
            supportedActions.Add("MAIL", MAIL);
            supportedActions.Add("RCPT", RCPT);
            supportedActions.Add("DATA", DATA);
            supportedActions.Add("RSET", RSET);
            supportedActions.Add("AUTH", AUTH);
        }

        private void initUnauthorizedActions()
        {
            unauthorizedActions = new HashSet<string>();
            unauthorizedActions.Add("HELO");
            unauthorizedActions.Add("EHLO");
            unauthorizedActions.Add("AUTH");
            unauthorizedActions.Add("QUIT");
            unauthorizedActions.Add("NOOP");
        }

        //SMTP command
        private void HELO(string message)
        {
            writeClient("250 OK");
        }

        //SMTP command
        private void EHLO(string message)
        {
            writeClient("250-AUTH LOGIN PLAIN");
            writeClient("250 OK");
        }

        private string getVerifyingUser(string message)
        {
            int from = message.IndexOf("VRFY:");
            int senderStart = message.IndexOf('<', from);
            int senderEnd = message.IndexOf('@', senderStart);
            return message.Substring(senderStart + 1, senderEnd - senderStart - 1);
        }

        //SMTP command
        private void VRFY(string message)
        {
            MySQLAccess database = new MySQLAccess();
            try
            {
                if (database.CheckLogin(getVerifyingUser(message)))
                    writeClient("250 OK");
                else
                    throw new Exception();
            }
            catch(Exception)
            {
                writeClient("550 No such user here");
            }
            finally
            {
                database.Close();
            }
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

        private void tryAuthenticate(string login, string password)
        {
            try
            {
                isAuthenticated = checkAuth(login, password);
                if (isAuthenticated)
                {
                    this.sender = login;
                    writeClient("235 Client authenticated");
                }
                else
                    throw new Exception();
            }
            catch (Exception)
            {
                writeClient("535 Credentials not valid");
            }
        }

        private void authPlain(string message)
        {
            int plainStart = message.IndexOf("PLAIN");
            int userStart = message.IndexOf(' ', plainStart) + 1;
            string userPassword = message.Substring(userStart).Trim();
            string credentials = Encoding.ASCII.GetString(Convert.FromBase64String(userPassword)).Substring(1);
            string[] logs = credentials.Split('\0');
            if (logs.Length == 2)
                tryAuthenticate(logs[0], logs[1]);
            else
                writeClient("535 Credentials not valid");
        }

        private void authLogin(string message)
        {
            writeClient("334 VXNlcm5hbWU6");
            string username = readClient();
            writeClient("334 UGFzc3dvcmQ6");
            string password = readClient();
            tryAuthenticate(Encoding.ASCII.GetString(Convert.FromBase64String(username)), Encoding.ASCII.GetString(Convert.FromBase64String(password)));
        }

        //SMTP command
        private void AUTH(string message)
        {
            if (message.Contains("PLAIN"))
                authPlain(message);
            else if (message.Contains("LOGIN"))
                authLogin(message);
            else
                writeClient("504 Authentication method not supported");
        }

        //SMTP command
        private void EXPN(string message)
        {
            writeClient("502 Command not supported");
        }

        //SMTP command
        private void HELP(string message)
        {
            writeClient("211-HELO <hostname>");
            writeClient("211-EHLO <hostname>");
            writeClient("211-MAIL FROM:<address>");
            writeClient("211-RCPT TO:<address>");
            writeClient("211-AUTH");
            writeClient("211-DATA");
            writeClient("211-QUIT");
            writeClient("211-HELP");
        }

        //SMTP command
        private void NOOP(string message)
        {
            writeClient("250 OK");
        }

        //SMTP command
        private void QUIT(string message)
        {
            writeClient("221 Bye");
        }

        private string getSender(string message)
        {
            int from = message.IndexOf("FROM:");
            int senderStart = message.IndexOf('<', from);
            int nameAt = message.IndexOf('@');
            int senderEnd = message.IndexOf('>', senderStart);

            if (message.Substring(senderStart + 1, senderEnd - senderStart - 1).EndsWith("@letterlink.com"))
                return message.Substring(senderStart + 1, nameAt - senderStart - 1);
            else
                return string.Empty;
        }

        //SMTP command
        private void MAIL(string message)
        {
            this.sender = getSender(message);
            if (!this.sender.Equals(string.Empty))
                writeClient("250 OK");
            else
                writeClient("550 Invalid sender address");
        }

        private string getRecipient(string message)
        {
            int from = message.IndexOf("TO:");
            int senderStart = message.IndexOf('<', from);
            int nameAt = message.IndexOf('@');
            int senderEnd = message.IndexOf('>', senderStart);

            if (message.Substring(senderStart + 1, senderEnd - senderStart - 1).EndsWith("@letterlink.com"))
                return message.Substring(senderStart + 1, nameAt - senderStart - 1);
            else
                return string.Empty;
        }

        //SMTP command
        private void RCPT(string message)
        {
            this.recipient = getRecipient(message);
            if (!this.recipient.Equals(string.Empty))
                writeClient("250 OK");
            else
                writeClient("550 Invaild recipient address");
        }

        private string getMessageContents()
        {
            StringBuilder mailContents = new StringBuilder();
            byte[] buffer = new byte[8192];
            int bytesRead;
            string block;
            do
            {
                bytesRead = clientStream.Read(buffer, 0, buffer.Length);
                block = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                mailContents.Append(block);
            } while (!block.Contains("\r\n.\r\n"));
            return mailContents.ToString();
        }

        private int[]? listMessage()
        {
            MySQLAccess database = new MySQLAccess();
            int[]? uids = database.AddMessage(this.sender, this.recipient);
            database.Close();
            return uids;
        }

        private bool saveMessage(string message)
        {
            int[]? uids = listMessage();
            try
            {
                if (uids != null && uids.Length == 2)
                {
                    File.WriteAllText($"sent/{uids[0]}.txt", message);
                    File.WriteAllText($"inbox/{uids[1]}.txt", message);
                    return true;
                }
                else
                    throw new Exception();
            }
            catch (Exception)
            {
                return false;
            }
        }

        //SMTP command
        private void DATA(string message)
        {
            writeClient("354 Enter message");
            string contents = getMessageContents();
            if (saveMessage(contents))
                writeClient("250 Message sent");
            else
                writeClient("550 Failed to save the message");
        }

        //SMTP sommand
        private void RSET(string message)
        {
            writeClient("502 OK");
        }
    }
}
