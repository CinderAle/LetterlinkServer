using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LetterlinkServer
{
    public class LoginHandler : Server
    {
        
        private const int commandLength = 3;

        protected override async void writeClient(string message)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(message + "\r\n");
            await clientStream.WriteAsync(bytes, 0, bytes.Length);
            Console.WriteLine($"[LOGIN] Server: '{message}'");
        }

        protected override string readClient()
        {
            StreamReader reader = new StreamReader(clientStream);
            string message = reader.ReadLine();
            return message;
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
                clientStream = client.GetStream();
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
            Console.WriteLine("[LOGIN] Client connected");
            writeClient("220 letterlink.login greetings");

            string? message = readClient();
            Console.WriteLine("[LOGIN] Client: " + message);
            chooseAction(message);
            
            if(client != null)
                client.Close();
            Console.WriteLine("[LOGIN] Client disconnected");
        }

        protected override void initActions()
        {
            supportedActions = new Dictionary<string, Action<string>>();
            supportedActions.Add("LOG", LOG);
            supportedActions.Add("REG", REG);
        }

        //Usage: LOG username hash
        private void LOG(string message)
        {
            message = message.Trim();
            string[] logs = message.Substring(4).Split(' ');
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
            string[] logs = message.Substring(4).Split(' ');
            MySQLAccess database = new MySQLAccess();
            if (database.InsertUser(logs[0], logs[1]))
                writeClient("250 registration successful");
            else
                writeClient("550 registration failed");
            database.Close();
        }
    }
}
