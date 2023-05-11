using System.Net;
using System.Net.Sockets;

namespace LetterlinkServer
{
    public class LoginHandler : Server
    {
        
        private const int commandLength = 3;

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
                handleMessages();
            }
        }

        protected override bool chooseAction(string? message)
        {
            string command = message != null ? message.Substring(0, commandLength) : string.Empty;
            if (supportedActions.ContainsKey(command) && message != null)
            {
                supportedActions[command].Invoke(message);
                return true;
            }
            else
                return false;
        }

        protected override async void handleMessages()
        {
            Console.WriteLine("Client connected");
            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream);
            StreamWriter writer = new StreamWriter(stream);


            string? message = await reader.ReadLineAsync();
            Console.WriteLine("Client: " + message);
            chooseAction(message);
            

            client.Close();
            Console.WriteLine("Client disconnected");
        }

        protected override void initActions()
        {
            supportedActions.Add("LOG", LOG);
            supportedActions.Add("REG", REG);
        }

        //Usage: LOG username hash
        private void LOG(string message)
        {
            message = message.Trim();
            string[] logs = message.Split(' ');
            MySQLAccess database = new MySQLAccess();
            if (database.CheckPassword(logs[0], logs[1]))
                writeClient("250 login successful");
            else
                writeClient("550 invalid login data");
            database.Close();
        }

        //Usage: REG username hash
        private void REG(string message)
        {
            message = message.Trim();
            string[] logs = message.Split(' ');
            MySQLAccess database = new MySQLAccess();
            if (database.InsertUser(logs[0], logs[1]))
                writeClient("250 registration successful");
            else
                writeClient("550 registration failed");
            database.Close();
        }
    }
}
