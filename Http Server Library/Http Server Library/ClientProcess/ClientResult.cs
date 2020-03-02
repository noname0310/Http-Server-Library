using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace HttpServerLibrary
{
    struct ClientResult
    {
        public Socket Socket;
        public ClientResultType ClientResultType;
        public int ContentSizeValue;
        public int CurrentReceivedByte;

        public ClientResult(Socket socket)
        {
            Socket = socket;
            ClientResultType = ClientResultType.KeepReceive;
            ContentSizeValue = -1;
            CurrentReceivedByte = 0;
        }
    }

    enum ClientResultType
    {
        KeepReceive,
        Continue
    }
}
