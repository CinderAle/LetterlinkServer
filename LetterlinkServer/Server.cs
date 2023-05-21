using System.Net.Sockets;

namespace LetterlinkServer
{
    public abstract class Server
    {
        protected int port;
        protected NetworkStream? clientStream;
        protected TcpClient? client;
        protected Dictionary<string, Action<string>>? supportedActions;

        public abstract void startServer(object? ctsObject);
        protected abstract void handleMessages();
        protected abstract bool chooseAction(string? message);
        protected abstract void initActions();
        protected abstract string readClient();
        protected abstract void writeClient(string message);
    }
}