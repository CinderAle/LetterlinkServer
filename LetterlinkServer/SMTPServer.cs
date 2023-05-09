using System.Net;
using System.Net.Sockets;

namespace LetterlinkServer
{
    public class SMTPServer : Server
    {
        private const int commandLength = 4;
        public SMTPServer()
        {
            port = 25;
            initActions();
        }

        public override async void startServer()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine($"SMTP server started on port {port}");

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
                return !command.Equals("QUIT");
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
        }

        private void HELO(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void EHLO(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void VRFY(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void EXPN(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void HELP(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void NOOP(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void QUIT(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void MAIL(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void RCPT(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void DATA(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void RSET(string message, StreamReader reader, StreamWriter writer)
        {

        }
    }
}
