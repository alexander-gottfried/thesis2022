using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Reliable
{
    class TaskUdpSocket : ISocket
    {
        private readonly UdpClient Client;
        private Action<byte[], IPEndPoint> ReceiveCallback;
        private readonly int NrThreads = 1;

        public TaskUdpSocket(int port)
        {
            Client = new UdpClient(port);
        }

        public TaskUdpSocket(int port, int nrThreads)
            : this(port)
        {
            NrThreads = nrThreads;
        }

        public void Send(byte[] message, IPEndPoint destination)
        {
            Client.Send(message, message.Length, destination);
        }

        private void ReceiveTask()
        {
            IPEndPoint source = new IPEndPoint(0, 0);
            while (true)
            {
                byte[] msg = Client.Receive(ref source);
                ReceiveCallback(msg, source);
            }
        }

        public void StartReceive(Action<byte[], IPEndPoint> receiveCallback)
        {
            ReceiveCallback = receiveCallback;
            for (int i = 0; i < NrThreads; i++)
                Task.Factory.StartNew(ReceiveTask, TaskCreationOptions.LongRunning);
        }
    }
}