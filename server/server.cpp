#include "server.hpp"
#include "tasks.hpp"
#include "socket_wrapper.hpp"
#include "buffer.hpp"
#include "reliable_detail.hpp"

#include <iostream>
#include <mutex>
#include <future>

using namespace boost::asio;
using boost::asio::ip::udp;

namespace reliable
{
    struct Server::Impl
    {
        SocketWrapper socket;

        std::mutex recvMutex;

        buf recvBuf;
        buf sendBuf;
        udp::endpoint source;
        udp::endpoint client;
        udp::endpoint syncServer;

        Task* task_;

        int lastSeq = -1;

        int lastAck = -1;

        int delay = 0;

        std::function<void()> onReceive;

        Impl(short port, udp::endpoint& syncPartner, int delayMs)
            : socket(port), syncServer(syncPartner), delay(delayMs)
        {
#ifdef CLIENT_SYNC
            std::cout << "sync w/ client" << std::endl;
#else
            std::cout << "sync w/ server" << std::endl;
#endif
        }
        ~Impl() {}

        void acknowledge(int seqNr)
        {
            if (seqNr > lastAck)
                lastAck = seqNr;
        }
    };

    Server::Server(short port, udp::endpoint& syncPartner)
    {
        simpl = std::make_unique<Impl>(port, syncPartner, 0);
    }

    Server::Server(short port, udp::endpoint& syncPartner, int resendDelayMs)
    {
        simpl = std::make_unique<Impl>(port, syncPartner, resendDelayMs);
    }

    void Server::startReply(Task* task)
    {
        simpl->task_ = task;
        simpl->onReceive = [this]() -> void {
            handlePacket(simpl->source, simpl->recvBuf);
            simpl->socket.receiveFrom(simpl->recvBuf, simpl->source, simpl->onReceive);
        };
        simpl->socket.receiveFrom(simpl->recvBuf, simpl->source, simpl->onReceive);
    }

    void Server::handleData(int seqNr, boost::asio::ip::udp::endpoint& source, buf& buffer)
    {
        if (simpl->client.address().is_unspecified())
            simpl->client = source;

        if (seqNr == simpl->lastSeq + 1)
            acceptData(seqNr, source, buffer);
        else if (seqNr > simpl->lastSeq && simpl->syncServer.port() > 0)
        {
#ifdef CLIENT_SYNC
            requestSync(seqNr, simpl->client);
#else
            requestSync(seqNr, simpl->syncServer);
#endif
        }
    }

    void Server::acceptData(int seqNr, boost::asio::ip::udp::endpoint& source, buf& buffer)
    {
#ifdef CLIENT_SYNC
        simpl->sendBuf[0] = MsgType::dataState;
#else
        simpl->sendBuf[0] = MsgType::data;
#endif
        std::memcpy(&simpl->sendBuf[1], &seqNr, sizeof(int));
        int written = 5 + simpl->task_->compute(buffer, 5, simpl->sendBuf, 5);
#ifdef CLIENT_SYNC
        written += simpl->task_->getSyncData(simpl->sendBuf, written);
#endif

        simpl->lastSeq = seqNr;

        sendBuffer(source, written);

        simpl->acknowledge(seqNr - 1);

        if (simpl->delay > 0)
        {
            auto f = [&]() {
                std::this_thread::sleep_for(std::chrono::milliseconds(simpl->delay));

                if (simpl->lastSeq == seqNr && simpl->lastAck < simpl->lastSeq)
                    sendBuffer(source, 5 + written);
            };

            auto a = std::async(std::launch::async, f);
        }
    }

    void Server::requestSync(int seqNr, boost::asio::ip::udp::endpoint& partner)
    {
        simpl->sendBuf[0] = MsgType::syncRequest;
        std::memcpy(&simpl->sendBuf[1], &seqNr, sizeof(int));

        std::cout << "last: " << simpl->lastSeq << " recv: " << seqNr << std::endl;

        sendBuffer(partner, 5);
    }

    void Server::handleSyncRequest(int seqNr, boost::asio::ip::udp::endpoint& source)
    {
        simpl->sendBuf[0] = MsgType::syncData;
        std::memcpy(&simpl->sendBuf[1], &seqNr, sizeof(int));
        int written = simpl->task_->getSyncData(simpl->sendBuf, 5);

        sendBuffer(source, 5 + written);

        std::cout << "syncRequest received @ " << seqNr << std::endl;
    }

    void Server::handleSyncData(int seqNr, boost::asio::ip::udp::endpoint& source, buf& buffer)
    {
        simpl->task_->synchronize(buffer, 5);
        simpl->lastSeq = seqNr;

        std::cout << "syncData received w/: " << seqNr << std::endl;
    }

    void Server::handlePacket(udp::endpoint& source, buf& buffer)
    {
        const std::scoped_lock guard(simpl->recvMutex);

        MsgType type = MsgType(buffer[0]);
        int recvSeq = 0;
        std::memcpy(&recvSeq, &buffer[1], sizeof(int));

        switch (type)
        {
        case MsgType::ack:
            simpl->acknowledge(recvSeq);
            break;
        case MsgType::data:
            handleData(recvSeq, source, buffer);
            break;
        case MsgType::syncRequest:
            handleSyncRequest(recvSeq, source);
            break;
        case MsgType::syncData:
            handleSyncData(recvSeq, source, buffer);
            break;
        }
    }

    void Server::sendBuffer(udp::endpoint& dest, int bytesToSend)
    {
        simpl->socket.sendTo(simpl->sendBuf, dest, bytesToSend);
    }

    Server::~Server() {}
}