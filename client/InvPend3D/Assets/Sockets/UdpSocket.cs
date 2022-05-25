using System;
using System.Net;
using System.Net.Sockets;

namespace Reliable
{
    class UdpSocket : ISocket
    {
        private readonly UdpClient Socket;
        private AsyncCallback Callback;

        public UdpSocket(int port)
        {
            Socket = new UdpClient(port);
        }

        public void Send(byte[] message, IPEndPoint destination)
        {
            Socket.SendAsync(message, message.Length, destination);
        }

        public void StartReceive(Action<byte[], IPEndPoint> receiveCallback)
        {
            Callback = new AsyncCallback(
                (IAsyncResult result) =>
                {
                    IPEndPoint source = new IPEndPoint(0, 0);
                    byte[] message = Socket.EndReceive(result, ref source);

                    try
                    {
                        receiveCallback(message, source);
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.Log(e.Message);
                    }

                    Socket.BeginReceive(Callback, Socket);
                });

            Socket.BeginReceive(Callback, Socket);
        }
    }


}
