#include "server.hpp"
#include "tasks.hpp"
#include "pid.hpp"

#include <iostream>
#include <string>
#include <fstream>
#include <sstream>

using std::cout; using std::endl; using std::cerr;

using Server = reliable::Server;

int main(int argc, char** argv)
{
    std::ifstream infile("defaults.cfg");
    if (infile.fail()) {
        cout << "Can't open defaults.cfg\n";
        return 1;
    }

    std::string sync_ip = "0.0.0.0";
    short port = 8010;
    short sync_port = 8010;
    int delay = 0;
    float Kp, Ki, Kd, dt, min, max;

    if (!(infile >> Kp >> Ki >> Kd >> dt >> min >> max))
    {
        cout << "Can't read defaults.cfg\n";
        return 1;
    }

    int idx = 1;
    while (idx < argc)
    {
        if (strcmp(argv[idx], "port") == 0 && idx + 1 < argc)
        {
            port = atoi(argv[idx + 1]);
            idx += 2;
        }
        else if (strcmp(argv[idx], "sync") == 0 && idx + 1 < argc)
        {
            sync_ip = argv[idx + 1];
            idx += 2;
        }
        else if (strcmp(argv[idx], "sport") == 0 && idx + 1 < argc)
        {
            sync_port = atoi(argv[idx + 1]);
            idx += 2;
        }
        else if (strcmp(argv[idx], "delay") == 0 && idx + 1 < argc)
        {
            delay = atoi(argv[idx + 1]);
            idx += 2;
        }
        else
            idx++;
    }

    using namespace std::placeholders;
    using namespace boost::asio::ip;

    cout << "1" << std::endl;

    Pid p(Kp, Ki, Kd, dt, min, max);
    reliable::TaskWrapper<float, float, array<float, 2>> task(&p);

    cout << "2" << std::endl;

    udp::endpoint sync_endp(address::from_string(sync_ip), sync_port);
    Server s(port, sync_endp);

    cout << "3" << std::endl;

    s.startReply(&task);

    cout << "4" << std::endl;

    return 0;
}