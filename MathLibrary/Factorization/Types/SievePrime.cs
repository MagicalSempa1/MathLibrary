using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Factorization.Types
{
    public readonly struct SievePrime(int p, ushort logpScaled, bool hasP2, int p2, int s1, int s2, int s1p2, int s2p2)
    {
        public readonly int p = p;
        public readonly ushort logpScaled = logpScaled;
        public readonly bool hasP2 = hasP2;
        public readonly int p2 = p2;


        public readonly int s1 = s1;
        public readonly int s2 = s2;
        public readonly int s1p2 = s1p2;
        public readonly int s2p2 = s2p2;
    }
}
