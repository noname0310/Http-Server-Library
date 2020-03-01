using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace HttpServerLibrary
{
    class ClientSocket
    {
        public delegate string ClientEndReceive(string ReceivedData);
        public event ClientEndReceive OnClientEndReceive;

        private int PacketSize;
        private byte[] Buffer;
        private Socket Client;
        private byte[] Header;

        public ClientSocket(int packetSize, byte[] header, Socket client)
        {
            PacketSize = packetSize;
            Header = header;
            Client = client;
        }

        public void Process()
        {
            Buffer = new byte[PacketSize];
            Client.BeginReceive(Buffer, 0, PacketSize, SocketFlags.None, Receivecallback, Client);
        }

        private void Receivecallback(IAsyncResult ar)
        {
            int receivedByte = Client.EndReceive(ar);

            string content = OnClientEndReceive?.Invoke(Encoding.UTF8.GetString(Buffer));

            byte[] contentBuffer = Encoding.UTF8.GetBytes(content);
            byte[] SendBuffer = new byte[Header.Length + contentBuffer.Length];
            System.Buffer.BlockCopy(Header, 0, SendBuffer, 0, Header.Length);
            System.Buffer.BlockCopy(contentBuffer, 0, SendBuffer, Header.Length, contentBuffer.Length);

            Client.Send(SendBuffer);

            Client.Close();
            Client.Dispose();
        }
    }
}
