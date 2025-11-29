using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Factorization.Types
{
    public readonly struct SpqsSievePlan(
        BigInteger baseLeft0,
        int blockLen,
        int logScale,
        int logSlack,
        int[] p,
        ushort[] logPScaled,
        bool[] hasP2,
        int[] p2,
        int[] r1,
        int[] r2,
        int[] r1p2,
        int[] r2p2,
        int[] baseLeftModP,
        int[] baseLeftModP2,
        int[] stepLmodP,
        int[] stepLmodP2)
    {
        public readonly BigInteger BaseLeft0 = baseLeft0;
        public readonly int BlockLen = blockLen;
        public readonly int LogScale = logScale;
        public readonly int LogSlack = logSlack;

        public readonly int[] P = p;
        public readonly ushort[] LogPScaled = logPScaled;
        public readonly bool[] HasP2 = hasP2;
        public readonly int[] P2 = p2;

        public readonly int[] R1 = r1;
        public readonly int[] R2 = r2;
        public readonly int[] R1P2 = r1p2;
        public readonly int[] R2P2 = r2p2;
        public readonly int[] BaseLeftModP = baseLeftModP;
        public readonly int[] BaseLeftModP2 = baseLeftModP2;
        public readonly int[] StepLmodP = stepLmodP;
        public readonly int[] StepLmodP2 = stepLmodP2;
    }
}
