using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Reliable
{
    class PrintedCallbackClient : CallbackClient
    {
        private Action<string> Print;

        public PrintedCallbackClient(ISocket socket, Action<float> callback, ITimer timer, Action<string> print)
            : base(socket, callback, timer)
        {
            Print = print;
        }

        protected override void SendToAll(int seqNr, float content, byte[] packet)
        {
            base.SendToAll(seqNr, content, packet);

            Print("Sent: " + packet.StrHex());
        }

        protected override void AcceptData(int seqNr, byte[] message, IPEndPoint source)
        {
            base.AcceptData(seqNr, message, source);

            Print(string.Format("Accepted: {0}\nby: {1}:{2}",
                seqNr, source.Address, source.Port));
        }

        protected override void RejectData(int seqNr, byte[] message, IPEndPoint source)
        {
            base.RejectData(seqNr, message, source);

            Print(string.Format("Rejected: {0}\nby: {1}:{2}",
                seqNr, source.Address, source.Port));
        }

        protected override void HandleState(int seqNr, byte[] message, IPEndPoint source)
        {
            base.HandleState(seqNr, message, source);

            Print("state");
        }

        protected override void HandleSyncRequest(int seqNr, IPEndPoint source)
        {
            base.HandleSyncRequest(seqNr, source);

            Print(string.Format("SyncRequest @ {0:X}\nby {1}:{2}",
                seqNr, source.Address, source.Port));
        }
    }

    class PrintedResendClient : ResendClient
    {
        private Action<string> Print;

        public PrintedResendClient(UdpSocket socket, Action<float> callback, ITimer timer, int delayMilliseconds, Action<string> print)
            : base(socket, callback, delayMilliseconds, timer)
        {
            Print = print;
        }

        protected override void SendToAll(int seqNr, float content, byte[] packet)
        {
            base.SendToAll(seqNr, content, packet);

            string pktstr = "";
            foreach (byte b in packet)
                pktstr += string.Format("{0:X} ", b);

            Print("Sent: " + pktstr);
        }

        protected override void Resend(int seqNr, float content, byte[] packet)
        {
            base.Resend(seqNr, content, packet);

            Print(string.Format("Resent @ {0}", seqNr));
        }

        protected override bool HandleData(int seqNr, byte[] message, IPEndPoint source)
        {
            bool r = base.HandleData(seqNr, message, source);

            string s = "";
            for (int i = 0; i < message.Length; i++)
                s += string.Format("{0:X} ", message[i]);
            Print("got: " + s);

            return r;
        }

        protected override void AcceptData(int seqNr, byte[] message, IPEndPoint source)
        {
            base.AcceptData(seqNr, message, source);

            Print(string.Format("Accepted: {0}\nby: {1}:{2}",
                seqNr, source.Address, source.Port));
        }

        protected override void RejectData(int seqNr, byte[] message, IPEndPoint source)
        {
            base.RejectData(seqNr, message, source);

            Print(string.Format("Rejected: {0}\nby: {1}:{2}",
                seqNr, source.Address, source.Port));
        }
    }
}