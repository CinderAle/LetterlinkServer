using LetterlinkServer;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MimeKit;
using Org.BouncyCastle.Asn1;
using System.Net;
using System.Net.Sockets;

internal class Program
{

    private static void Main(string[] args)
    {
        SMTPServer smtp = new SMTPServer();
        IMAPServer imap = new IMAPServer();
        LoginHandler login = new LoginHandler(85);
        Thread smtpThread = new Thread(new ThreadStart(smtp.startServer));
        Thread imapThread = new Thread(new ThreadStart(imap.startServer));
        Thread loginThread = new Thread(new ThreadStart(login.startServer));

        smtpThread.Start();
        imapThread.Start();
        loginThread.Start();

        /*SmtpClient smtpClient = new SmtpClient();
        smtpClient.Connect("localhost", 25, false);
        smtpClient.Authenticate("user", "password");*/

        ImapClient imapClient = new ImapClient();
        imapClient.Connect("localhost", 143, false);
        imapClient.Authenticate("user", "password");

        MimeMessage message = new MimeMessage();
        message.From.Add(new MailboxAddress("biba", "biba@letterlink.com"));
        message.To.Add(new MailboxAddress("boba", "boba@letterlink.com"));
        message.Subject = "Test subject";
        BodyBuilder body = new BodyBuilder();
        body.TextBody = "Test message text";
        body.Attachments.Add("sendImageTest.jpg", File.ReadAllBytes("sendImageTest.jpg"));
        body.Attachments.Add("sendImageTest2.png", File.ReadAllBytes("sendImageTest2.png"));
        message.Body = body.ToMessageBody();
        imapClient.Disconnect(true);
        
        /*smtpClient.Send(message);
        smtpClient.Disconnect(true);*/
            
        while (true) ;
    }
}