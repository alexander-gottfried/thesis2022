#ifndef RELIABLE__
#define RELIABLE__

#include "tasks.hpp"
#include "buffer.hpp"
#include <string>
#include <memory>
#include <boost/asio.hpp>

namespace reliable
{
    class Server
    {
    public:
        Server(short port, boost::asio::ip::udp::endpoint& syncPartner);
        Server(short port, boost::asio::ip::udp::endpoint& syncPartner, int resendDelayMs);
        ~Server();
        void startReply(Task* task);
    protected:
        void sendBuffer(boost::asio::ip::udp::endpoint& dest, int bytesToSend);
        void handlePacket(boost::asio::ip::udp::endpoint& source, buf& buffer);
        void handleData(int seqNr, boost::asio::ip::udp::endpoint& source, buf& buffer);
        void acceptData(int seqNr, boost::asio::ip::udp::endpoint& source, buf& buffer);
        void requestSync(int seqNr, boost::asio::ip::udp::endpoint& partner);
        void handleSyncRequest(int seqNr, boost::asio::ip::udp::endpoint& source);
        void handleSyncData(int seqNr, boost::asio::ip::udp::endpoint& source, buf& buffer);
    private:
        struct Impl;
        std::unique_ptr<Impl> simpl;
    };
}

#endif