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

        public override async void startServer()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine($"IMAP server started on port {port}");

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                handleMessages(client);
            }
        }

        protected override bool chooseAction(string? message, StreamReader reader, StreamWriter writer)
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
                supportedActions[command].Invoke(message, reader, writer);
                return command.Equals("LOGOUT");
            }
            else
                return true; 
        }

        protected override async void handleMessages(TcpClient client)
        {
            Console.WriteLine("Client connected");
            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream);
            StreamWriter writer = new StreamWriter(stream);

            while (true)
            {
                string? message = await reader.ReadLineAsync();
                Console.WriteLine("Client: " + message);
                if (!chooseAction(message, reader, writer))
                    break;
            }

            client.Close();
            Console.WriteLine("Client disconnected");
        }

        protected override void initActions()
        {
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

        private void CAPABILITY(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void NOOP(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void LOGOUT(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void AUTHENTICATE(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void LOGIN(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void SELECT(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void EXAMINE(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void CREATE(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void DELETE(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void RENAME(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void SUBSCRIBE(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void UNSUBSCRIBE(string message, StreamReader reader, StreamWriter writer)
        {

        }
        private void LIST(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void LSUB(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void STATUS(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void APPEND(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void CHECK(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void CLOSE(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void EXPUNGE(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void SEARCH(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void FETCH(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void STORE(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void COPY(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void UID(string message, StreamReader reader, StreamWriter writer)
        {

        }
    }
}
