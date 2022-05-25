namespace Reliable
{
    class TrafficCounter
    {
        readonly object inLock = new object();
        public int InBytes { get; private set; } = 0;

        readonly object outLock = new object();
        public int OutBytes { get; private set; } = 0;

        public int In(int bytes)
        {
            lock (inLock)
            {
                InBytes += bytes;
                return InBytes;
            }
        }

        public int Out(int bytes)
        {
            lock (outLock)
            {
                OutBytes += bytes;
                return OutBytes;
            }
        }
    }
}