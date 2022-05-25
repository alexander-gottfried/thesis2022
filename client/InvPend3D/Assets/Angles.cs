using System.Collections.Generic;
using System.Diagnostics;

namespace Reliable
{
    class AngleRecorder
    {
        private Stopwatch Watch;
        private Dictionary<string, List<double>> Values;

        public AngleRecorder()
        {
            Watch = Stopwatch.StartNew();
            Values = new Dictionary<string, List<double>>();
            Values.Add("times", new List<double>());
            Values.Add("angles", new List<double>());
        }

        ~AngleRecorder()
        {
            Watch.Stop();
        }

        public void RecordNew(float angle)
        {
            Values["times"].Add(Watch.Elapsed.TotalMilliseconds);
            Values["angles"].Add(angle);
        }

        public Dictionary<string, List<double>> GetValues()
            => Values;
    }
}