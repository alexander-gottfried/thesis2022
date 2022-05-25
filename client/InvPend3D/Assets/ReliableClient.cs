using System;
using System.Net;
using UnityEngine;

namespace Reliable
{
    /*
    enum MsgType : byte
    {
        data, syncRequest, syncData
    }

    class ReliableClient
        : IPublisher<float>
    {

        private readonly AsyncSocket Socket;

        private Action<byte[]> HandleData
            = (byte[] m) => { };
        private Action<AsyncSocket, byte[], IPEndPoint> HandleSyncRequest
            = (AsyncSocket sc, byte[] m, IPEndPoint sr) => { };

        private int LastSeq = -1, NextSeq = 0;

        private ITimer Timer = new NullTimer();

        public ReliableClient(AsyncSocket socket)
        {
            Socket = socket;
            Socket.StartReceive(OnReceive);
        }

        public ReliableClient(AsyncSocket socket, PerPacketTimer timer)
            : this(socket)
        {
            Timer = timer;
        }

        public void Send(float msg)
        {
            byte[] message = new byte[9];
            int sendSeq = NextSeq++;

            Util.PackData(ref message, sendSeq, msg);

            Timer.Start(sendSeq);

            Socket.Send(message);
        }

        private void OnReceive(byte[] message, IPEndPoint source)
        {
            MsgType msgType = (MsgType)message[0];

            switch (msgType)
            {
                case MsgType.data:
                    {
                        int recvSeqNr = 0;
                        float content = 0;
                        Util.UnpackData(ref recvSeqNr, ref content, message);

                        Timer.Stop(recvSeqNr);

                        if (recvSeqNr == LastSeq + 1)
                        {
                            Util.PrintUsed(message, recvSeqNr, content, source);

                            LastSeq++;

                            Publisher.Publish(content);
                        }
                        else
                            Util.PrintDisc(message, recvSeqNr, content, source);
                    } break;
                case MsgType.syncRequest:
                    HandleSyncRequest(Socket, message, source);
                    break;
            }
        }
        
        private readonly Publisher<float> Publisher = new Publisher<float>();
        // IPublisher functionality
        public uint Subscribe(Action<float> callback) => Publisher.Subscribe(callback);
        public void Unsubscribe(uint id) => Publisher.Unsubscribe(id);
    }
    */
}
