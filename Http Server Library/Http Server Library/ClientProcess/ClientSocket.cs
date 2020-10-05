using System;
using System.Text;
using System.Net.Sockets;

namespace HttpServerLibrary
{
    class ClientSocket
    {
        public delegate string ClientEndReceive(byte[] ReceivedData, ParseResult parseResult);
        public event ClientEndReceive OnClientEndReceive;

        private int PacketSize;
        private byte[] Buffer;

        private Socket Client;
        private byte[] Header;

        private bool FirstTime;
        private byte[] ContentFullBuffer;
        private int LeftContentByte;
        private int ContentOffSet;
        private ParseResult parseResult;

        public ClientSocket(int packetSize, byte[] header, Socket client)
        {
            PacketSize = packetSize;
            Header = header;
            Client = client;
            FirstTime = true;
        }

        public void StartProcess()
        {
            Buffer = new byte[PacketSize];
            Process(Buffer.Length);
        }

        private void Process(int ReceiveLength)
        {
            for (int i = 0; i < Buffer.Length; i++)
                Buffer[i] = 0;

            Client.BeginReceive(Buffer, 0, (Buffer.Length < ReceiveLength) ? Buffer.Length : ReceiveLength, SocketFlags.None, Receivecallback, Client);
        }

        private void Receivecallback(IAsyncResult ar)
        {
            int receivedByte = Client.EndReceive(ar);
            if (receivedByte == 0)
            {
                try
                {
                    if (Client.Connected)
                        Client.Shutdown(SocketShutdown.Both);
                    Client.Close();
                    Client.Dispose();
                }
                catch
                {
                }
            }

            if (FirstTime)
            {
                parseResult = PacketParser.HeaderParse(Buffer);

                if (parseResult.RequestType == RequestType.GET || parseResult.ContentLength == parseResult.ReceivedContentLength)
                {
                    ReturnDataAndDispose(OnClientEndReceive?.Invoke(Buffer, parseResult));
                }
                else
                {
                    LeftContentByte = parseResult.ContentLength - parseResult.ReceivedContentLength;
                    ContentOffSet = parseResult.ContentRange.EndIndex + 1;

                    ContentFullBuffer = new byte[parseResult.ContentRange.StartIndex + parseResult.ContentLength];
                    System.Buffer.BlockCopy(Buffer, 0, ContentFullBuffer, 0, receivedByte);

                    FirstTime = false;
                    Process(LeftContentByte);
                }
            }
            else
            {
                System.Buffer.BlockCopy(Buffer, 0, ContentFullBuffer, ContentOffSet, receivedByte);

                LeftContentByte -= receivedByte;
                ContentOffSet += receivedByte;

                if (LeftContentByte <= 0)
                    ReturnDataAndDispose(OnClientEndReceive?.Invoke(ContentFullBuffer, parseResult));
                else
                    Process(LeftContentByte);
            }
        }

        private void ReturnDataAndDispose(string content)
        {
            byte[] ContentBuffer = Encoding.UTF8.GetBytes(content);
            byte[] SendBuffer = new byte[Header.Length + ContentBuffer.Length];
            System.Buffer.BlockCopy(Header, 0, SendBuffer, 0, Header.Length);
            System.Buffer.BlockCopy(ContentBuffer, 0, SendBuffer, Header.Length, ContentBuffer.Length);

            try
            {
                Client.Send(SendBuffer);
                if (Client.Connected)
                    Client.Shutdown(SocketShutdown.Both);
                Client.Close();
                Client.Dispose();
            }
            catch 
            {
            }
        }
    }
}
