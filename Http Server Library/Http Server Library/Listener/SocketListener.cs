﻿using System;
using System.Net;
using System.Net.Sockets;

namespace HttpServerLibrary
{
    class SocketListener
    {
        public delegate void ClientConnected(Socket ClientSocket);
        public event ClientConnected OnClientConnected;

        public Socket Socket { get; private set; }
        public bool Listening;

        public SocketListener()
        {
        }

        public void Start(IPEndPoint endPoint)
        {
            if (Listening)
                return;

            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            Socket.Bind(endPoint);
            Socket.Listen(100);
            Socket.BeginAccept(AcceptCallback, null);
            Listening = false;
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            Socket clientSocket;
            try
            {
                clientSocket = Socket.EndAccept(ar);
                Socket.BeginAccept(AcceptCallback, null);
            }
            catch (SocketException)
            {
                return;
            }

            OnClientConnected?.Invoke(clientSocket);

        }

        public void Stop()
        {
            if (!Listening)
                return;

            Socket.Close();
            Socket.Dispose();
            Listening = false;
        }
    }
}
