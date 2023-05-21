using System.Net;
using System.Net.Sockets;

namespace LetterlinkServer
{
    public class POP3Server : Server
    {
        private const int messageLength = 4;

        public POP3Server()
        {
            port = 110;
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

        public override async void startServer(object? ctsObject)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine($"POP3 server started on port {port}");

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                handleMessages();
            }
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

        protected override bool chooseAction(string? message)
        {
            string command = string.Empty;
            foreach (string method in supportedActions.Keys)
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

        protected override void initActions()
        {
            supportedActions.Add("NOOP", NOOP);
            supportedActions.Add("QUIT", QUIT);
            supportedActions.Add("USER", USER);
            supportedActions.Add("PASS", PASS);
            supportedActions.Add("APOP", APOP);
            supportedActions.Add("STAT", STAT);
            supportedActions.Add("LIST", LIST);
            supportedActions.Add("RETR", RETR);
            supportedActions.Add("TOP", TOP);
            supportedActions.Add("DELE", DELE);
            supportedActions.Add("RSET", RSET);
        }

        private void NOOP(string message)
        {

        }

        private void QUIT(string message)
        {

        }

        private void USER(string message)
        {

        }

        private void PASS(string message)
        {

        }

        private void APOP(string message)
        {

        }

        private void STAT(string message)
        {

        }

        private void LIST(string message)
        {

        }

        private void RETR(string message)
        {

        }

        private void TOP(string message)
        {

        }

        private void DELE(string message)
        {

        }

        private void RSET(string message)
        {

        }
    }
}
