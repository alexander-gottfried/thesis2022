using System;
using System.Collections.Generic;

namespace Reliable
{

    interface IPublisher<PublishT>
    {
        uint Subscribe(Action<PublishT> callback);
        void Unsubscribe(uint id);
    }

    class Publisher<PublishT> : IPublisher<PublishT>
    {
        private Dictionary<uint, Action<PublishT>> Subscribers = new Dictionary<uint, Action<PublishT>>();
        private uint NextEntry = 0;

        public uint Subscribe(Action<PublishT> callback)
        {
            Subscribers.Add(NextEntry, callback);
            return NextEntry++;
        }

        public void Unsubscribe(uint id)
        {
            Subscribers.Remove(id);
        }

        public void Publish(PublishT data)
        {
            foreach (var s in Subscribers.Values)
            {
                s(data);
            }
        }
    }
}
