using System.Net;

namespace Reliable
{
    interface IRedundantClient
    {
        void Connect(IPEndPoint endpoint);
        void Disconnect(IPEndPoint endpoint);
    }
}
