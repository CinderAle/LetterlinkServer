using System.Net;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Runtime.ConstrainedExecution;

namespace LetterlinkServer
{
    public class IMAPServer : Server
    {
        public IMAPServer()
        {
            port = 143;
            initActions();
        }

        protected override async void writeClient(string message)
        {
            StreamWriter writer = new StreamWriter(clientStream);
            await writer.WriteAsync(message);
            await writer.FlushAsync();
        }

        protected override string readClient()
        {
            StreamReader reader = new StreamReader(clientStream);
            char[] buffer = new char[8192];
            int charsRead = reader.Read(buffer, 0, 8192);
            return new string(buffer).Substring(0, charsRead).Replace("\0", "");
        }

        public override async void startServer()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine($"IMAP server started on port {port}");

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                handleMessages();
            }
        }

        protected override bool chooseAction(string? message)
        {
            string command = string.Empty;
            foreach(string method in supportedActions.Keys)
                if (message != null && message.StartsWith(method))
                {
                    command = method; 
                    break;
                }
            if (!command.Equals(string.Empty) && message != null)
            {
                supportedActions[command].Invoke(message);
                return command.Equals("LOGOUT");
            }
            else
                return true; 
        }

        protected override async void handleMessages()
        {
            Console.WriteLine("Client connected");
            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream);
            StreamWriter writer = new StreamWriter(stream);

            while (true)
            {
                string? message = await reader.ReadLineAsync();
                Console.WriteLine("Client: " + message);
                if (!chooseAction(message))
                    break;
            }

            client.Close();
            Console.WriteLine("Client disconnected");
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
        }

        private void CAPABILITY(string message)
        {

        }

        private void NOOP(string message)
        {

        }

        private void LOGOUT(string message)
        {

        }

        private void AUTHENTICATE(string message)
        {

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
    }
}
