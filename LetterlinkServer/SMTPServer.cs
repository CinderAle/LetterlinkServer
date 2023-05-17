using Org.BouncyCastle.Tls.Crypto.Impl.BC;
using System.Collections;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Reflection;
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

        public SMTPServer()
        {
            port = 25;
            initActions();
            initUnauthorizedActions();
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

        public override async void startServer()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine($"SMTP server started on port {port}");

            while (true)
            {
                clearContext();
                try
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    clientStream = client.GetStream();
                    handleMessages();
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
                if (unauthorizedActions.Contains(command) || isAuthenticated)
                {
                    supportedActions[command].Invoke(message);
                    return !command.Equals("QUIT");
                }
                else
                {
                    writeClient("535 Authentication required");
                    return true;
                }
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

        private void authPlain(string message)
        {
            int plainStart = message.IndexOf("PLAIN");
            int userStart = message.IndexOf(' ', plainStart) + 1;
            string userPassword = message.Substring(userStart).Trim();
            string credentials = Encoding.ASCII.GetString(Convert.FromBase64String(userPassword)).Substring(1);
            string[] logs = credentials.Split('\0');
            isAuthenticated = checkAuth(logs[0], logs[1]);
            if (isAuthenticated)
                writeClient("235 Client authenticated");
            else
                writeClient("535 Credentials not valid");
        }

        private void authLogin(string message)
        {
            writeClient("250 OK");
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
            int senderEnd = message.IndexOf('@', senderStart);
            return message.Substring(senderStart + 1, senderEnd - senderStart - 1);
        }

        //SMTP command
        private void MAIL(string message)
        {
            this.sender = getSender(message);
            writeClient("250 OK");
        }

        private string getRecipient(string message)
        {
            int to = message.IndexOf("TO:");
            int recipientStart = message.IndexOf('<', to);
            int recipientEnd = message.IndexOf('@', recipientStart);
            return message.Substring(recipientStart + 1, recipientEnd - recipientStart - 1);
        }

        //SMTP command
        private void RCPT(string message)
        {
            this.recipient = getRecipient(message);
            writeClient("250 OK");
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

        //SMTP command
        private void DATA(string message)
        {
            writeClient("354 Enter message");
            string contents = getMessageContents();
            Console.WriteLine("From: " + this.sender + "\r\nTo: " + this.recipient);
            //Console.WriteLine("Client data:: " + mailContents.ToString());
            writeClient("250 OK");
        }

        //SMTP sommand
        private void RSET(string message)
        {
            writeClient("502 OK");
        }
    }
}
