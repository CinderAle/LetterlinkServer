using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;

namespace LetterlinkServer
{
    public class SMTPServer : Server
    {
        private const int commandLength = 4;
        private string sender;
        private string recipient;

        public SMTPServer()
        {
            port = 25;
            initActions();
        }

        protected override async void writeClient(string message)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(message + "\r\n");
            await clientStream.WriteAsync(bytes, 0, bytes.Length);
            Console.WriteLine($"Server: '{message}'");
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
                TcpClient client = await listener.AcceptTcpClientAsync();
                clientStream = client.GetStream();
                handleMessages(); 
            }
        }

        protected override bool chooseAction(string? message)
        {
            string command = (message != null) && message.Length >= commandLength ? message.Substring(0, commandLength) : string.Empty;
            if (supportedActions.ContainsKey(command) && message != null)
            {
                supportedActions[command].Invoke(message);
                return !command.Equals("QUIT");
            }
            else
            {
                writeClient("250 Command not supported");
                return true;
            }
        }

        protected override async void handleMessages()
        {
            Console.WriteLine("Client connected");
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
                Console.WriteLine("Client: " + message.Trim());
                if (!chooseAction(message))
                    break;
            }

            if(client != null)
                client.Close();
            Console.WriteLine("Client disconnected");
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
        }

        //SMTP command
        private void HELO(string message)
        {
            writeClient("250 OK");
        }

        //SMTP command
        private void EHLO(string message)
        {
            writeClient("250 OK");
        }

        //SMTP command
        private void VRFY(string message)
        {
            writeClient("250 OK");
        }

        //SMTP command
        private void EXPN(string message)
        {
            writeClient("250 OK");
        }

        //SMTP command
        private void HELP(string message)
        {
            writeClient("211-HELO <hostname>");
            writeClient("211-EHLO <hostname>");
            writeClient("211-MAIL FROM:<address>");
            writeClient("211-RCPT TO:<address>");
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
            //Console.WriteLine("From: " + this.sender + "\r\nTo: " + this.recipient);
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
