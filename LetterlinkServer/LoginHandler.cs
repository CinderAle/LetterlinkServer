using System.Net;
using System.Net.Sockets;

namespace LetterlinkServer
{
    public class LoginHandler : Server
    {
        private const int commandLength = 3;

        public LoginHandler(int port)
        {
            this.port = port;
            initActions();
        }

        public override async void startServer()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine($"Logs server started on port {port}");

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                handleMessages(client);
            }
        }

        protected override bool chooseAction(string? message, StreamReader reader, StreamWriter writer)
        {
            string command = message != null ? message.Substring(0, commandLength) : string.Empty;
            if (supportedActions.ContainsKey(command) && message != null)
            {
                supportedActions[command].Invoke(message, reader, writer);
                return true;
            }
            else
                return false;
        }

        protected override async void handleMessages(TcpClient client)
        {
            Console.WriteLine("Client connected");
            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream);
            StreamWriter writer = new StreamWriter(stream);


            string? message = await reader.ReadLineAsync();
            Console.WriteLine("Client: " + message);
            chooseAction(message, reader, writer);
            

            client.Close();
            Console.WriteLine("Client disconnected");
        }

        protected override void initActions()
        {
            supportedActions.Add("LOG", LOG);
            supportedActions.Add("REG", REG);
        }

        //Usage: LOG username hash
        private void LOG(string message, StreamReader reader, StreamWriter writer)
        {
            message = message.Trim();
            string[] logs = message.Split(' ');
            MySQLAccess database = new MySQLAccess();
            if (database.CheckPassword(logs[0], logs[1]))
                writer.WriteAsync("250 login successful");
            else
                writer.WriteAsync("401 invalid login data");
            database.Close();
        }

        //Usage: REG username hash
        private void REG(string message, StreamReader reader, StreamWriter writer)
        {
            message = message.Trim();
            string[] logs = message.Split(' ');
            MySQLAccess database = new MySQLAccess();
            if (database.InsertUser(logs[0], logs[1]))
                writer.Write("250 registration successful");
            else
                writer.Write("401 registration failed");
            database.Close();
        }
    }
}
