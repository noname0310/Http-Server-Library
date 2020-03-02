using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace HttpServerLibrary
{
    public class HttpServer
    {
        public delegate string ClientRequestProcess(RequestType requestType, string parameter, string content);
        public event ClientRequestProcess OnClientRequest;

        public Socket Socket { get; private set; }

        private CountdownEvent CountdownEvent;

        private const int PACKET_SIZE = 1024 * 10;
        private IPEndPoint IPEndPoint;
        private SocketListener SocketListener;
        private ClientSocketManager ClientSocketManager;

        public HttpServer(IPEndPoint endPoint)
        {
            Socket = null;

            IPEndPoint = endPoint;
            SocketListener = new SocketListener();
            ClientSocketManager = new ClientSocketManager(PACKET_SIZE);

            SocketListener.OnClientConnected += SocketListener_OnClientConnected;
            ClientSocketManager.OnClientRequest += ClientSocketManager_OnClientRequest;
        }

        public HttpServer(IPEndPoint endPoint, CountdownEvent countdownEvent) : this(endPoint)
        {
            CountdownEvent = countdownEvent;
        }

        ~HttpServer()
        {
            SocketListener.OnClientConnected -= SocketListener_OnClientConnected;
            ClientSocketManager.OnClientRequest -= ClientSocketManager_OnClientRequest;
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
            CountdownEvent.Signal();
        }

        private void SocketListener_OnClientConnected(Socket ClientSocket) => ClientSocketManager.ClientProcess(ClientSocket);

        private string ClientSocketManager_OnClientRequest(RequestType requestType, string parameter, string content)
        {
            return OnClientRequest?.Invoke(requestType, parameter, content);
        }
    }

    public enum RequestType
    {
        GET,
        POST,
        PUT,
        DELETE
    }
}
