using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Reliable
{

    abstract class Client
    {
        protected enum MsgType : byte
        {
            data, syncRequest, syncData, ack, dataState
        }

        protected readonly ISocket Socket;

        public Client(ISocket socket)
        {
            Socket = socket;
            Socket.StartReceive(OnReceive);
        }

        private void OnReceive(byte[] message, IPEndPoint source)
        {
            MsgType msgType = (MsgType)message[0];
            int recvSeq = BitConverter.ToInt32(message, 1);

            switch (msgType)
            {
                case MsgType.data:
                    //Acknowledge(recvSeq, source);
                    HandleData(recvSeq, message, source);
                    break;
                case MsgType.dataState:
                    if (HandleData(recvSeq, message, source))
                        HandleState(recvSeq, message, source);
                    break;
                case MsgType.syncRequest:
                    HandleSyncRequest(recvSeq, source);
                    break;
            }
        }

        public void Connect(string address, int port)
        {
            Connect(new IPEndPoint(IPAddress.Parse(address), port));
        }

        public abstract void Send(float message);
        public abstract void Connect(IPEndPoint remoteEndPoint);

        protected abstract void Acknowledge(int recvSeq, IPEndPoint source);
        protected abstract bool HandleData(int seqNr, byte[] message, IPEndPoint source);
        protected abstract void AcceptData(int seqNr, byte[] message, IPEndPoint source);
        protected abstract void RejectData(int seqNr, byte[] message, IPEndPoint source);
        protected abstract void HandleState(int seqNr, byte[] message, IPEndPoint source);
        protected abstract void HandleSyncRequest(int seqNr, IPEndPoint source);
    }

    class CallbackClient : Client
    {
        protected readonly List<IPEndPoint> ServerEndPoints;

        private readonly Action<float> Callback;

        protected int LastSeq = -1, NextSeq = 0;

        private float[] State = new float[2];

        private readonly ITimer Timer;
        public readonly FromWho SrcCounter = new FromWho();

        public CallbackClient(ISocket socket, Action<float> callback)
            : base(socket)
        {
            Callback = callback;

            ServerEndPoints = new List<IPEndPoint>();

            Timer = new NullTimer();
        }

        public CallbackClient(ISocket socket, Action<float> callback, ITimer timer)
            : this(socket, callback)
        {
            Timer = timer;
        }

        public override void Connect(IPEndPoint remoteEndPoint)
        {
            ServerEndPoints.Add(remoteEndPoint);
        }

        public override void Send(float message)
        {
            byte[] packet = new byte[9];
            int sendSeq = NextSeq++;

            packet[0] = (byte)MsgType.data;
            Buffer.BlockCopy(BitConverter.GetBytes(sendSeq), 0, packet, 1, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(message), 0, packet, 5, 4);

            SendToAll(sendSeq, message, packet);
        }

        protected virtual void SendToAll(int seqNr, float content, byte[] packet)
        {
            Timer.Start(seqNr);

            foreach (var destination in ServerEndPoints)
            {
                Socket.Send(packet, destination);
            }
        }

        protected override void Acknowledge(int recvSeq, IPEndPoint source)
        {
            /*
            Timer.Stop(recvSeq);

            byte[] ack = new byte[5];
            ack[0] = (byte)MsgType.ack;
            Buffer.BlockCopy(BitConverter.GetBytes(recvSeq), 0, ack, 1, 4);

            Socket.Send(ack, source);
            */
        }

        protected override bool HandleData(int seqNr, byte[] message, IPEndPoint source)
        {
            bool valid = seqNr == LastSeq + 1;
            if (valid)
                AcceptData(seqNr, message, source);
            else
                RejectData(seqNr, message, source);
            return valid;
        }

        protected override void AcceptData(int seqNr, byte[] message, IPEndPoint source)
        {
            Timer.Stop(seqNr);
            SrcCounter.Received(source.Address.ToString());
            LastSeq++;

            float recvContent = BitConverter.ToSingle(message, 5);

            Callback(recvContent);
        }

        protected override void RejectData(int seqNr, byte[] message, IPEndPoint source) { }

        protected override void HandleState(int seqNr, byte[] message, IPEndPoint source)
        {
            Buffer.BlockCopy(message, 9, State, 0, 8);

            UnityEngine.Debug.Log($"{message.Str()}\n{seqNr}: {State.Str()}");
        }

        protected override void HandleSyncRequest(int seqNr, IPEndPoint source)
        {
            byte[] sync = new byte[13];
            sync[0] = (byte)MsgType.syncData;
            Buffer.BlockCopy(BitConverter.GetBytes(LastSeq), 0, sync, 1, 4);
            Buffer.BlockCopy(State, 0, sync, 5, 8);

            UnityEngine.Debug.Log($"{State.Str()} : {sync.StrHex()}");

            Socket.Send(sync, source);
        }
    }
}