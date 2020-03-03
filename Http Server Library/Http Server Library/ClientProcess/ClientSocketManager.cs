using System;
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
            clientSocket.StartProcess();
        }

        private string ClientSocket_OnClientEndReceive(byte[] ReceivedData, ParseResult parseResult)
        {
            string content;
            string parameter;

            if (parseResult.ContentLength == 0)
                content = "";
            else
            {
                byte[] contentByte = new byte[parseResult.ContentLength];
                Buffer.BlockCopy(ReceivedData, parseResult.ContentRange.StartIndex, contentByte, 0, contentByte.Length);
                content = Encoding.UTF8.GetString(contentByte);
            }

            if (parseResult.ParameterRange.EndIndex - parseResult.ParameterRange.StartIndex <= 0)
                parameter = "";
            else
            {
                byte[] parameterByte = new byte[parseResult.ParameterRange.EndIndex - parseResult.ParameterRange.StartIndex + 1];
                Buffer.BlockCopy(ReceivedData, parseResult.ParameterRange.StartIndex, parameterByte, 0, parameterByte.Length);
                parameter = Encoding.UTF8.GetString(parameterByte);
            }

            return OnClientRequest?.Invoke(parseResult.RequestType, parameter, content);
        }
    }
}
