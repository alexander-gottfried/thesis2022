using System;
using System.Net;

namespace Reliable
{
    interface ISocket
    {
        void Send(byte[] message, IPEndPoint destination);
        void StartReceive(Action<byte[], IPEndPoint> receiveCallback);
    }
}
