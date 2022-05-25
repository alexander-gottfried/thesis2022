#ifndef SOCKET_WRAPPER__
#define SOCKET_WRAPPER__

#include <vector>
#include <functional>
#include <boost/asio.hpp>
#include "buffer.hpp"

namespace reliable
{
    class SocketWrapperImpl;
    class SocketWrapper
    {
    public:
        SocketWrapper(short port);
        SocketWrapper(int packetLength, short port);
        ~SocketWrapper();
        void sendTo(buf& data, boost::asio::ip::udp::endpoint& destination);
        void sendTo(buf& data, boost::asio::ip::udp::endpoint& destination, int sentBytes);
        void sendTo(std::shared_ptr<buf> data_ptr, boost::asio::ip::udp::endpoint& destination);
        void receiveFrom(buf& data, boost::asio::ip::udp::endpoint& sender, std::function<void()>& handler);
        void receiveFrom(std::shared_ptr<buf> data_ptr, boost::asio::ip::udp::endpoint& sender, std::function<void()>& handler);
    private:
        SocketWrapperImpl* swimpl;
    };
}

#endif