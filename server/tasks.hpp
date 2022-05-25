#ifndef REL_TASKS__
#define REL_TASKS__

#include <functional>
#include <vector>
#include <cstring>
#include "buffer.hpp"

namespace reliable
{
    class Task
    {
    public:
        virtual ~Task() {}
        virtual int compute(buf& in, int in_offset, buf& out, int out_offset) = 0;
        virtual void synchronize(buf& syncData, int offset) = 0;
        virtual int getSyncData(buf& destination, int offset) = 0;
    };

    template<typename In, typename Out, typename Sync>
    class TaskWrapper : public Task
    {
    public:
        class T
        {
        public:
            virtual Out compute(In x) = 0;
            virtual void synchronize(Sync& x) = 0;
            virtual Sync& getSyncData() = 0;
        };

        TaskWrapper(T* task)
            : task_(task)
        { }

        int compute(buf& in, int in_offset, buf& out, int out_offset) override
        {
            In value;
            std::memcpy(&value, &in[in_offset], sizeof(In));

            Out output = task_->compute(value);
            std::memcpy(&out[out_offset], &output, sizeof(Out));

            return sizeof(Out);
        }

        void synchronize(buf& syncData, int offset) override
        {
            Sync sValue;
            std::memcpy(&sValue, &syncData[offset], sizeof(Sync));

            task_->synchronize(sValue);
        }

        int getSyncData(buf& output, int offset) override
        {
            Sync sData = task_->getSyncData();
            std::memcpy(&output[offset], &sData, sizeof(Sync));

            return sizeof(Sync);
        }
    private:
        T* task_;
    };
}

#endif