using Google.Protobuf.WellKnownTypes;
using LetterlinkServer;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Cryptography;
using System.Net;
using System.Net.Sockets;

internal class Program
{

    static readonly int[] smtpPorts = { 25, 587 };
    static readonly int[] imapPorts = { 143, 993 };
    static readonly int[] loginPorts = { 85 };
    const string startCommand = "START";
    const string stopCommand = "STOP";
    const string smtpServer = "SMTP";
    const string imapServer = "IMAP";
    const string loginServer = "LOGIN";
    const string wrongCommandError = "The specified command does not exist";
    const string wrongOptionError = "The specified option is not correct!";

    static Thread[]? smtp = null;
    static CancellationTokenSource[]? smtpCancel = null;
    static Thread[]? imap = null;
    static CancellationTokenSource[]? imapCancel = null;
    static Thread[]? login = null;
    static CancellationTokenSource[]? loginCancel = null;

    private static int[]? showOptions(int[] options)
    {
        if (options != null && options.Length > 1)
        {
            for (int i = 0; i < options.Length; i++)
                Console.WriteLine($"{i + 1}. {options[i]}");
            return new int[] { 1, options.Length };
        }
        else
            return options != null && options.Length == 1 ? new int[] { 1 } : null;
    }

    private static int getOption(int[] diapason)
    {
        Console.Write("Choose option: ");
        if(diapason != null)
        {
            if (diapason.Length == 1)
                return 1;
            else
            {
                try
                {
                    int temp = Convert.ToInt32(Console.ReadLine());
                    if (temp < diapason[0] || temp > diapason[1])
                        throw new Exception();
                    else
                        return temp - 1;
                }
                catch (Exception)
                {
                    return -1;
                }
            }
        }
            return -1;
    }

    private static int processPortsChooser(int[] ports)
    {
        int[] diapason = showOptions(ports);
        int option = getOption(diapason);
        if (option >= 0)
            return ports[option];
        else
        {
            Console.WriteLine(wrongOptionError);
            return -1;
        }
    }

    private static void startSMTPServer(int port)
    {
        if (smtp == null)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            smtp = new Thread[1000];
            smtpCancel = new CancellationTokenSource[1000];
            for(int i = 0;i < smtp.Length; i++)
            {
                smtpCancel[i] = new CancellationTokenSource();
                smtp[i] = new Thread(new ParameterizedThreadStart(new SMTPServer(listener).startServer));
                smtp[i].Start(smtpCancel[i].Token);
            }
            Console.WriteLine($"SMTP server started on port {port}");
        }
        else
            Console.WriteLine("SMTP server is already started");
    }

    private static void startIMAPServer(int port)
    {
        if (imap == null)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            imap = new Thread[1000];
            imapCancel = new CancellationTokenSource[1000];
            for (int i = 0; i < imap.Length; i++)
            {
                imapCancel[i] = new CancellationTokenSource();
                imap[i] = new Thread(new ParameterizedThreadStart(new IMAPServer(listener).startServer));
                imap[i].Start(imapCancel[i].Token);
            }
            Console.WriteLine($"IMAP server started on port {port}");
        }
        else
            Console.WriteLine("IMAP server is already started");
    }

    private static void startLoginServer(int port)
    {
        if (login == null)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            login = new Thread[1000];
            loginCancel = new CancellationTokenSource[1000];
            for (int i = 0; i < login.Length; i++)
            {
                loginCancel[i] = new CancellationTokenSource();
                login[i] = new Thread(new ParameterizedThreadStart(new LoginHandler(listener).startServer));
                login[i].Start(loginCancel[i].Token);
            }
            Console.WriteLine($"SMTP server started on port {port}");
        }
        else
            Console.WriteLine("SMTP server is already started");
    }

    private static void stopSMTPServer()
    {
        if(smtp != null && smtpCancel != null)
        {
            for(int i = 0;i < smtp.Length; i++)
            {
                smtpCancel[i].Cancel();
                smtp[i].Join();
            }
            smtp = null;
            smtpCancel = null;
            Console.WriteLine("SMTP server stopped");
        }
        else
            Console.WriteLine("SMTP server is not yet started");
    }

    private static void stopIMAPServer()
    {
        if (imap != null && imapCancel != null)
        {
            for (int i = 0; i < imap.Length; i++)
            {
                imapCancel[i].Cancel();
                imap[i].Join();
            }
            imap = null;
            imapCancel = null;
            Console.WriteLine("IMAP server stopped");
        }
        else
            Console.WriteLine("IMAP server is not yet started");
    }

    private static void stopLoginServer()
    {
        if (login != null && loginCancel != null)
        {
            for (int i = 0; i < login.Length; i++)
            {
                loginCancel[i].Cancel();
                login[i].Join();
            }
            login = null;
            loginCancel = null;
            Console.WriteLine("Login server stopped");
        }
        else
            Console.WriteLine("Login server is not yet started");
    }

    private static void parseCommand(string command)
    {
        command = command.ToUpper().Trim();
        if (command.StartsWith(startCommand + ' '))
        {
            int port;
            command = command.Substring(startCommand.Length + 1);
            switch (command)
            {
                case smtpServer:
                    port = processPortsChooser(smtpPorts);
                    if(port > 0)
                        startSMTPServer(port);
                    break;
                case imapServer:
                    port = processPortsChooser(imapPorts);
                    if (port > 0)
                        startIMAPServer(port);
                    break;
                case loginServer:
                    port = processPortsChooser(loginPorts);
                    if (port > 0)
                        startLoginServer(port);
                    break;
                default:
                    Console.WriteLine(wrongCommandError);
                    break;
            }
        }
        else if (command.StartsWith(stopCommand + ' '))
        {
            command = command.Substring(stopCommand.Length + 1);
            switch (command)
            {
                case smtpServer:
                    stopSMTPServer();
                    break;
                case imapServer:
                    stopIMAPServer();
                    break;
                case loginServer:
                    stopLoginServer();
                    break;
                default:
                    Console.WriteLine(wrongCommandError);
                    break;
            }
        }
        else
            Console.WriteLine(wrongCommandError);
    }

    private static void Main(string[] args)
    {
        while (true) 
        {
            Console.WriteLine("Enter command:");
            parseCommand(Console.ReadLine());
        };
    }
}