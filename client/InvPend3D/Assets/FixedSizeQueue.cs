using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testudp.src
{
    class FixedSizeQueue<T> where T : IComparable
    {
        private Queue<T> q = new Queue<T>();
        public readonly int Limit;
        public FixedSizeQueue(int limit)
        {
            Limit = limit;
        }
        public void Enqueue(T obj)
        {
            q.Enqueue(obj);
            while (q.Count > Limit) q.Dequeue();
        }
        public List<T> ToList()
        {
            List<T> r = q.ToList();
            r.Reverse();
            return r;
        }
        public int Count() { return q.Count; }
    }
}
