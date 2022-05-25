using System.Collections.Generic;
using System.Diagnostics;

namespace Reliable
{
    interface ITimer
    {
        void Start(int nr);
        double Stop(int nr);
    }

    class NullTimer : ITimer
    {
        public void Start(int nr)
        { }
        public double Stop(int nr)
        { return 0; }
    }

    class PerPacketTimer : ITimer
    {
        private readonly Stopwatch Watch = Stopwatch.StartNew();

        public List<double> Times { get; } = new List<double>(1000);

        public void Start(int sequenceNr)
        {
            if (sequenceNr == Times.Count)
            {
                Times.Add(-Watch.Elapsed.TotalMilliseconds);
            }
        }

        public double Stop(int sequenceNr)
        {
            if (Times[sequenceNr] <= 0)
                Times[sequenceNr] += Watch.Elapsed.TotalMilliseconds;
            return Times[sequenceNr];
        }

        public override string ToString()
        { 
            string s = "";
            foreach (var time in Times)
                s += time + ", ";
            return s;
        }
    }
}
