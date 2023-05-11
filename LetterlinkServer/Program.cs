using LetterlinkServer;
using MailKit.Net.Smtp;
using MimeKit;
using System.Net;
using System.Net.Sockets;

internal class Program
{
    private static void Main(string[] args)
    {
        SMTPServer smtp = new SMTPServer();
        Thread smtpThread = new Thread(new ThreadStart(smtp.startServer));
        smtpThread.Start();

        SmtpClient smtpClient = new SmtpClient();
        smtpClient.Connect("localhost", 25, false);
        MimeMessage message = new MimeMessage();
        message.From.Add(new MailboxAddress("biba", "biba@letterlink.com"));
        message.To.Add(new MailboxAddress("boba", "boba@letterlink.com"));
        message.Subject = "Test subject";
        BodyBuilder body = new BodyBuilder();
        body.TextBody = "Test message text";
        body.Attachments.Add("sendImageTest.jpg", File.ReadAllBytes("sendImageTest.jpg"));
        body.Attachments.Add("sendImageTest2.png", File.ReadAllBytes("sendImageTest2.png"));
        message.Body = body.ToMessageBody();
        smtpClient.Send(message);
        smtpClient.Disconnect(true);
            
        while (true) ;
    }
}