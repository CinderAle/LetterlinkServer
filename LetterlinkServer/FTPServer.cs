using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LetterlinkServer
{
    public class FTPServer : Server
    {
        public FTPServer() 
        {
            port = 21;
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
            Console.WriteLine($"FTP server started on port {port}");

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                handleMessages();
            }
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

        protected override async void handleMessages()
        {
            Console.WriteLine("Client connected");

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
                Console.WriteLine("Client: " + message);
                /*if (!chooseAction(message, reader, writer))
                    break;*/
            }

            client.Close();
            Console.WriteLine("Client disconnected");
        }

        protected override void initActions()
        {
            supportedActions.Add("USER", USER);
            supportedActions.Add("PASS", PASS);
            supportedActions.Add("ACCT", ACCT);
            supportedActions.Add("CWD", CWD);
            supportedActions.Add("CDUP", CDUP);
            supportedActions.Add("SMNT", SMNT);
            supportedActions.Add("REIN", REIN);
            supportedActions.Add("QUIT", QUIT);
            supportedActions.Add("PORT", PORT);
            supportedActions.Add("PASV", PASV);
            supportedActions.Add("TYPE", TYPE);
            supportedActions.Add("STRU", STRU);
            supportedActions.Add("MODE", MODE);
            supportedActions.Add("RETR", RETR);
            supportedActions.Add("STOR", STOR);
            supportedActions.Add("STOU", STOU);
            supportedActions.Add("APPE", APPE);
            supportedActions.Add("ALLO", ALLO);
            supportedActions.Add("REST", REST);
            supportedActions.Add("RNFR", RNFR);
            supportedActions.Add("RNTO", RNTO);
            supportedActions.Add("ABOR", ABOR);
            supportedActions.Add("DELE", DELE);
            supportedActions.Add("RMD", RMD);
            supportedActions.Add("MKD", MKD);
            supportedActions.Add("PWD", PWD);
            supportedActions.Add("LIST", LIST);
            supportedActions.Add("NLST", NLST);
            supportedActions.Add("SITE", SITE);
            supportedActions.Add("SYST", SYST);
            supportedActions.Add("STAT", STAT);
            supportedActions.Add("HELP", HELP);
            supportedActions.Add("NOOP", NOOP);
        }

        private void USER(string message)
        {

        }

        private void PASS(string message)
        {

        }

        private void ACCT(string message)
        {

        }

        private void CWD(string message)
        {

        }

        private void CDUP(string message)
        {

        }

        private void SMNT(string message)
        {

        }

        private void REIN(string message)
        {

        }

        private void QUIT(string message)
        {

        }

        private void PORT(string message)
        {

        }

        private void PASV(string message)
        {

        }

        private void TYPE(string message)
        {

        }

        private void STRU(string message)
        {

        }

        private void MODE(string message)
        {

        }

        private void RETR(string message)
        {

        }

        private void STOR(string message)
        {

        }

        private void STOU(string message)
        {

        }

        private void APPE(string message)
        {

        }

        private void ALLO(string message)
        {

        }

        private void REST(string message)
        {

        }

        private void RNFR(string message)
        {

        }

        private void RNTO(string message)
        {

        }

        private void ABOR(string message)
        {

        }

        private void DELE(string message)
        {

        }

        private void RMD(string message)
        {

        }

        private void MKD(string message)
        {

        }

        private void PWD(string message)
        {

        }

        private void LIST(string message)
        {

        }

        private void NLST(string message)
        {

        }

        private void SITE(string message)
        {

        }

        private void SYST(string message)
        {

        }

        private void STAT(string message)
        {

        }

        private void HELP(string message)
        {

        }

        private void NOOP(string message)
        {

        }
    }
}
