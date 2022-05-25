#include <array>
#include "tasks.hpp"
using std::array;

class Pid : public reliable::TaskWrapper<float, float, array<float, 2>>::T
{
private:
    const float Kp; const float Ki; const float Kd;
    const float dt; const float max; const float min;

    array<float, 2> var_vals = { 0.0f, 0.0f };
    float* pre_err = &(var_vals[0]);
    float* integral = &(var_vals[1]);

public:
    Pid(float Kp_, float Ki_, float Kd_, float dt_, float min_, float max_)
        : Kp(Kp_), Ki(Ki_), Kd(Kd_), dt(dt_), max(max_), min(min_)
    { }

    float compute(float pv) override
    {
        float sp = 0.0f;

        float error = sp - pv;

        float Pout = Kp * error;

        *integral += error * dt;
        float Iout = Ki * *integral;

        float deriv = (error - *pre_err) / dt;
        float Dout = Kd * deriv;

        float output = Pout + Iout + Dout;

        *pre_err = error;

        if (output > max)
            output = max;
        else if (output < min)
            output = min;

        return output;
    }

    void synchronize(array<float, 2>& syncData) override
    {
        var_vals = syncData;
    }

    array<float, 2>& getSyncData() override
    {
        return var_vals;
    }
};