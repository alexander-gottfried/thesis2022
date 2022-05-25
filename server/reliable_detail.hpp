#ifndef RELIABLE_DETAIL__
#define RELIABLE_DETAIL__

#include <iostream>
#include <sstream>

#include "buffer.hpp"

namespace reliable {
    enum MsgType : unsigned char
    {
        data, syncRequest, syncData, ack, dataState
    };

    // DEBUG
    static void printBuf(const buf& buf)
    {
        std::stringstream str;
        str << std::hex;
        for (int i = 0; i < buf.size(); i++)
        {
            str << (int)buf[i] << " ";
        }
        str << "\n";
        std::cout << str.str();
    }
}

#endif