using System;
using System.Net;
using System.Net.Sockets;

namespace HttpServerLibrary
{
    public class HttpServer
    {
        public Socket Socket { get; private set; }

        private const int PACKET_SIZE = 1024 * 4;
        private SocketListener SocketListener;
        private IPEndPoint IPEndPoint;

        public HttpServer(IPEndPoint endPoint)
        {
            Socket = null;

            SocketListener = new SocketListener();
            IPEndPoint = endPoint;

            SocketListener.OnClientConnected += SocketListener_OnClientConnected;
        }

        ~HttpServer()
        {
            SocketListener.OnClientConnected -= SocketListener_OnClientConnected;
        }

        public void Start()
        {
            SocketListener.Start(IPEndPoint);
            Socket = SocketListener.Socket;
        }

        public void Stop()
        {
            SocketListener.Stop();
            Socket = null;
        }

        private void SocketListener_OnClientConnected(Socket ClientSocket)
        {
            new ClientSocket(PACKET_SIZE).ClientProcess(ClientSocket);
        }
    }
}
