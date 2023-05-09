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

        public override async void startServer()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine($"FTP server started on port {port}");

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                handleMessages(client);
            }
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

        private void USER(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void PASS(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void ACCT(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void CWD(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void CDUP(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void SMNT(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void REIN(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void QUIT(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void PORT(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void PASV(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void TYPE(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void STRU(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void MODE(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void RETR(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void STOR(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void STOU(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void APPE(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void ALLO(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void REST(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void RNFR(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void RNTO(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void ABOR(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void DELE(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void RMD(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void MKD(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void PWD(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void LIST(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void NLST(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void SITE(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void SYST(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void STAT(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void HELP(string message, StreamReader reader, StreamWriter writer)
        {

        }

        private void NOOP(string message, StreamReader reader, StreamWriter writer)
        {

        }
    }
}
