using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reliable
{
    class FromWho
    {
        public string[] Addresses { get; } = { "10.2.1.149", "10.2.1.150" };
        public int[] Counts { get; } = { 0, 0 };

        public void Received(string ip)
        {
            for (int i = 0; i < Addresses.Length; i++)
            {
                if (Addresses[i] == ip)
                    Counts[i]++;
            }
        }
    }
}
