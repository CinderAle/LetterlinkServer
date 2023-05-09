using System.Net.Sockets;

namespace LetterlinkServer
{
    public abstract class Server
    {
        protected int port;
        protected Dictionary<string, Action<string, StreamReader, StreamWriter>> supportedActions;
        public abstract void startServer();
        protected abstract void handleMessages(TcpClient client);
        protected abstract bool chooseAction(string? message, StreamReader reader, StreamWriter writer);
        protected abstract void initActions();
    }
}
