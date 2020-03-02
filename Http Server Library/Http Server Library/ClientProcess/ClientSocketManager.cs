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

        private string ClientSocket_OnClientEndReceive(byte[] ReceivedData)
        {
            RequestType requestType;
            string parameter = null;
            string content = null;
            Console.WriteLine(Encoding.UTF8.GetString(ReceivedData).Replace('\r', 'r').Replace('\n', 'n'));

            int startindex;
            switch (ReceivedData[0])
            {
                case (byte)'G':
                    requestType = RequestType.GET;
                    startindex = 5;
                    break;

                case (byte)'P':

                    if (ReceivedData[1] == (byte)'O')
                    {
                        requestType = RequestType.POST;
                        startindex = 6;
                    }
                    else
                    {
                        requestType = RequestType.PUT;
                        startindex = 5;
                    }
                    break;

                case (byte)'D':
                    requestType = RequestType.DELETE;
                    startindex = 7;
                    break;

                default:
                    goto case (byte)'G';
            }

            int index = startindex;
            string ReceivedString = Encoding.UTF8.GetString(ReceivedData);
            while (ReceivedString[index] != ' ')
                index++;

            parameter = ReceivedString.Substring(startindex, index - startindex);

            return OnClientRequest?.Invoke(requestType, parameter, content);
        }
    }
}
