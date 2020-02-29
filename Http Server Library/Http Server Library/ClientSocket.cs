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
        int PacketSize;
        byte[] Buffer;
        Socket Client;

        public ClientSocket(int packetSize)
        {
            PacketSize = packetSize;
        }

        public void ClientProcess(Socket client)
        {
            Client = client;

            Buffer = new byte[PacketSize];
            client.BeginReceive(Buffer, 0, PacketSize, SocketFlags.None, Receivecallback, client);
        }
        private void Receivecallback(IAsyncResult ar)
        {
            int receivedByte = Client.EndReceive(ar);
            Console.WriteLine("Received Data.Length = " + receivedByte);
            Console.WriteLine(Encoding.UTF8.GetString(Buffer));

            EndReceive();
            return;
        }

        private void EndReceive()
        {
            Console.WriteLine("EndReceive");

            Client.Close();
            Client.Dispose();
        }
    }
}
