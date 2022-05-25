#include "socket_wrapper.hpp"

#include <iostream>
#include <boost/bind/bind.hpp>

using std::cout; using std::endl; using std::cerr;
using std::vector;
using std::function;

using namespace boost::asio;
using boost::asio::ip::udp;
using boost::bind;

namespace reliable
{
    class SocketWrapperImpl
    {
    private:
        thread_pool ioThreads;
        executor_work_guard<thread_pool::executor_type>* work_guard;

        udp::socket socket;

        const int packetLength;
    public:
        SocketWrapperImpl(int packetLength_, short port)
            : socket(ioThreads, udp::endpoint(udp::v4(), port))
            , packetLength(packetLength_)
        {
            work_guard = new executor_work_guard<thread_pool::executor_type>(ioThreads.get_executor());
        }

        ~SocketWrapperImpl()
        {
            delete work_guard;

            ioThreads.join();
            ioThreads.stop();
        }

        void sendTo(buf& data, udp::endpoint& destination, int sentBytes)
        {
            socket.async_send_to(
                buffer(data, sentBytes), destination,
                [&](std::error_code const& err, std::size_t len)
                {
                    if (err)
                        std::cerr << err.message() << endl;
                }
            );
        }

        void sendTo(std::shared_ptr<buf> data_ptr, udp::endpoint& destination)
        {
            socket.async_send_to(
                buffer(*data_ptr), destination,
                [&](std::error_code const& err, std::size_t len)
                {
                    if (err)
                        std::cerr << err.message() << endl;
                }
            );
        }

        void receiveFrom(buf& data, udp::endpoint& sender, function<void()>& handler)
        {
            socket.async_receive_from(
                buffer(data), sender,
                bind(handleRecv, handler,
                    std::placeholders::_1,
                    std::placeholders::_2)
            );
        }

        void receiveFrom(std::shared_ptr<buf> data_ptr, udp::endpoint& sender, function<void()>& handler)
        {
            socket.async_receive_from(
                buffer(*data_ptr), sender,
                bind(handleRecv, handler,
                    std::placeholders::_1,
                    std::placeholders::_2)
            );
        }

    private:
        function<void(function<void()>&, std::error_code const&, std::size_t)>
            // ---
            handleRecv =
            [](function<void()>& func, std::error_code const& error, std::size_t length) -> void
        {
            if (error)
            {
                std::cerr << error.message() << endl;
                return;
            }
            func();
        };
    };

    SocketWrapper::SocketWrapper(short port)
        : SocketWrapper(64, port)
    {}
    SocketWrapper::SocketWrapper(int packetLength, short port)
    {
        swimpl = new SocketWrapperImpl(packetLength, port);
    }
    SocketWrapper::~SocketWrapper()
    {
        delete swimpl;
    }
    void SocketWrapper::sendTo(buf& data, udp::endpoint& destination)
    {
        swimpl->sendTo(data, destination, data.size());
    }
    void SocketWrapper::sendTo(buf& data, udp::endpoint& destination, int sentBytes)
    {
        swimpl->sendTo(data, destination, sentBytes);
    }
    void SocketWrapper::sendTo(std::shared_ptr<buf> data_ptr, udp::endpoint& destination)
    {
        swimpl->sendTo(data_ptr, destination);
    }
    void SocketWrapper::receiveFrom(buf& data, udp::endpoint& sender, function<void()>& handler)
    {
        swimpl->receiveFrom(data, sender, handler);
    }
}