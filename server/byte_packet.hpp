#ifndef BYTE_PACKET__
#define BYTE_PACKET__

#include <memory>
#include <vector>
#include <array>
#include <cstring>
#include <exception>

namespace reliable
{

    template<size_t PktSize>
    class BytePacket
    {
    private:
        int offset = 0;
    public:
        const std::shared_ptr<std::array<unsigned char, PktSize>> ptr;

        BytePacket(int size)
            : ptr(std::make_shared<std::array<unsigned char, PktSize>>())
        { }

        template<typename T>
        BytePacket& put(T& value)
        {
            int insertedSize = copy(value);

            offset += insertedSize;

            return *this;
        }

        template<typename T>
        BytePacket& put(T&& value)
        {
            int insertedSize = copy(value);

            offset += insertedSize;

            return *this;
        }
    private:
        template<typename T>
        size_t copy(const T& src)
        {
            int size = sizeof(T);
            testBoundsCopy(&(*ptr)[offset], &src, size, offset);
            return size;
        }

        template<typename T>
        size_t copy(const std::vector<T>& src)
        {
            int size = sizeof(T) * src.size();
            testBoundsCopy(&(*ptr)[offset], &src[0], size, offset);
            return size;
        }

        template<typename T, size_t N>
        size_t copy(const std::array<T, N>& src)
        {
            int size = sizeof(T) * N;
            testBoundsCopy(&(*ptr)[offset], &src[0], size, offset);
            return size;
        }

        void testBoundsCopy(void* dst, const void* src, size_t size, int offset)
        {
            if (offset + size > PktSize)
                throw std::out_of_range("out of range");
            std::memcpy(dst, src, size);
        }
    };

}

#endif