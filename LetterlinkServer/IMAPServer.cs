using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Cms;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace LetterlinkServer
{
    public class IMAPServer : Server
    {
        private string sender;
        private string recipient;
        private bool isAuthenticated;

        public IMAPServer()
        {
            port = 143;
            initActions();
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
            sender = string.Empty;
            recipient = string.Empty;
            isAuthenticated = false;
        }

        public override async void startServer()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine($"IMAP server started on port {port}");

            while (true)
            {
                clearContext();
                TcpClient client = await listener.AcceptTcpClientAsync();
                clientStream = client.GetStream();
                handleMessages();
            }
        }

        protected override bool chooseAction(string? message)
        {
            string command = string.Empty;
            foreach(string method in supportedActions.Keys)
                if (message != null && message.Substring(10).StartsWith(method))
                {
                    command = method; 
                    break;
                }
            if (!command.Equals(string.Empty) && message != null)
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
            supportedActions.Add("EXAMINE", EXAMINE);
            supportedActions.Add("CREATE", CREATE);
            supportedActions.Add("DELETE", DELETE);
            supportedActions.Add("RENAME", RENAME);
            supportedActions.Add("SUBSCRIBE", SUBSCRIBE);
            supportedActions.Add("UNSUBSCRIBE", UNSUBSCRIBE);
            supportedActions.Add("LIST", LIST);
            supportedActions.Add("LSUB", LSUB);
            supportedActions.Add("STATUS", STATUS);
            supportedActions.Add("APPEND", APPEND);
            supportedActions.Add("CHECK", CHECK);
            supportedActions.Add("CLOSE", CLOSE);
            supportedActions.Add("EXPUNGE", EXPUNGE);
            supportedActions.Add("SEARCH", SEARCH);
            supportedActions.Add("FETCH", FETCH);
            supportedActions.Add("STORE", STORE);
            supportedActions.Add("COPY", COPY);
            supportedActions.Add("UID", UID);
            supportedActions.Add("STARTTLS", STARTTLS);
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
            isAuthenticated = checkAuth(logs[0], logs[1]);
            if(isAuthenticated)
                writeClient(code + "OK AUTH complete");
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
            isAuthenticated = checkAuth(username, password);
            if (isAuthenticated)
                writeClient(code + "OK AUTH complete");
            else
                writeClient(code + "NO failed to authenticate");
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

        }

        private void SELECT(string message)
        {

        }

        private void EXAMINE(string message)
        {

        }

        private void CREATE(string message)
        {

        }

        private void DELETE(string message)
        {

        }

        private void RENAME(string message)
        {

        }

        private void SUBSCRIBE(string message)
        {

        }

        private void UNSUBSCRIBE(string message)
        {

        }
        private void LIST(string message)
        {
            writeClient("* LIST (\\Noselect) \"/\" \"\"");
            writeClient("* LIST (\\HasNoChildren) \"/\" \"INBOX\"");
            writeClient(message.Substring(0, 10) + "OK LIST completed");
        }

        private void LSUB(string message)
        {

        }

        private void STATUS(string message)
        {

        }

        private void APPEND(string message)
        {

        }

        private void CHECK(string message)
        {

        }

        private void CLOSE(string message)
        {

        }

        private void EXPUNGE(string message)
        {

        }

        private void SEARCH(string message)
        {

        }

        private void FETCH(string message)
        {

        }

        private void STORE(string message)
        {

        }

        private void COPY(string message)
        {

        }

        private void UID(string message)
        {

        }

        private void STARTTLS(string message)
        {
            writeClient(message.Substring(0, 10) + "OK Begin TLS negotiation now.");   
        }
    }
}
