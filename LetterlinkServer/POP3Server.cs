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

        public override async void startServer()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine($"POP3 server started on port {port}");

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                handleMessages(client);
            }
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

        protected override bool chooseAction(string? message, StreamReader reader, StreamWriter writer)
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
                supportedActions[command].Invoke(message, reader, writer);
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

        private void NOOP(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void QUIT(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void USER(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void PASS(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void APOP(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void STAT(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void LIST(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void RETR(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void TOP(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void DELE(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void RSET(string message, StreamReader reader, StreamWriter writer)
        {

        }
    }
}
