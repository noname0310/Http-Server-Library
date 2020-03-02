using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace HttpServerLibrary
{
    class ClientSocketManager
    {
        public delegate string ClientRequestProcess(RequestType requestType, string parameter, string content);
        public event ClientRequestProcess OnClientRequest;

        private byte[] Header;
        private readonly int PacketSize;

        public ClientSocketManager(int packetsize)
        {
            Header = Encoding.UTF8.GetBytes("HTTP/1.0 200 OK\r\n" +
                "Connection: close\r\n" +
                //"Content-Type: text/html; charset=utf-8\r\n" +
                "\n");

            PacketSize = packetsize;
        }

        public void ClientProcess(Socket Client)
        {
            ClientSocket clientSocket = new ClientSocket(PacketSize, Header, Client);
            clientSocket.OnClientEndReceive += ClientSocket_OnClientEndReceive;
            clientSocket.Process();
        }

        private string ClientSocket_OnClientEndReceive(string ReceivedData)
        {
            RequestType requestType;
            string parameter = null;
            string content = null;
            Console.WriteLine(ReceivedData);

            int startindex;
            switch (ReceivedData.Substring(0, 2))
            {
                case "GE":
                    requestType = RequestType.GET;
                    startindex = 5;
                    break;

                case "PO":
                    requestType = RequestType.POST;
                    startindex = 6;
                    break;

                case "DE":
                    requestType = RequestType.DELETE;
                    startindex = 7;
                    break;

                case "PU":
                    requestType = RequestType.PUT;
                    startindex = 5;
                    break;

                default:
                    goto case "GE";
            }

            int index = startindex;
            while (ReceivedData[index] != ' ')
                index++;

            parameter = ReceivedData.Substring(startindex, index - startindex);

            return OnClientRequest?.Invoke(requestType, parameter, content);
        }
    }
}
