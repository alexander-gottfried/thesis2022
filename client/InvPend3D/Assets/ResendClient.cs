using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Reliable
{

    class ResendClient : CallbackClient
    {
        private int Delay;
        private Dictionary<IPEndPoint, int> SequenceNrs = new Dictionary<IPEndPoint, int>();

        public ResendClient(UdpSocket socket, Action<float> callback, int delayMilliseconds)
            : base(socket, callback)
        {
            Delay = delayMilliseconds;
        }

        public ResendClient(UdpSocket socket, Action<float> callback, int delayMilliseconds, ITimer timer)
            : base(socket, callback, timer)
        {
            Delay = delayMilliseconds;
        }

        public override void Connect(IPEndPoint remoteEndPoint)
        {
            base.Connect(remoteEndPoint);

            SequenceNrs.Add(remoteEndPoint, -1);
        }

        protected virtual void Resend(int seqNr, float content, byte[] packet)
        {
            foreach (var server in SequenceNrs)
            {
                if (server.Value < LastSeq)
                    Socket.Send(packet, server.Key);
            }
        }

        protected override void SendToAll(int seqNr, float content, byte[] packet)
        {
            base.SendToAll(seqNr, content, packet);

            Task.Delay(Delay).ContinueWith(_ =>
                {
                    Resend(seqNr, content, packet);
                }
            );
        }
    }
}