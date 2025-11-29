using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Factorization.Types
{
    public sealed class SiqsSievePlan(
        QSPolynomial poly,
        BigInteger baseLeft0,
        int blockLen,
        int logScale,
        int logSlack,
        int[] P,
        ushort[] logPScaled,
        bool[] hasP2,
        int[] p2,
        int[] baseLeftModP,
        int[] baseLeftModP2,
        int[] stepLmodP,
        int[] stepLmodP2,
        int[] r1,
        int[] r2,
        int[] r1p2,
        int[] r2p2,
        bool[] skipPrime,
        int termCount,
        int[][] deltaModP,
        int[][]? deltaModP2)
    {
        public QSPolynomial Poly = poly;

        public readonly BigInteger BaseLeft0 = baseLeft0;
        public readonly int BlockLen = blockLen;
        public readonly int LogScale = logScale;
        public readonly int LogSlack = logSlack;

        public readonly int[] P = P;
        public readonly ushort[] LogPScaled = logPScaled;
        public readonly bool[] HasP2 = hasP2;
        public readonly int[] P2 = p2;

        public readonly int[] BaseLeftModP = baseLeftModP;
        public readonly int[] BaseLeftModP2 = baseLeftModP2;
        public readonly int[] StepLmodP = stepLmodP;
        public readonly int[] StepLmodP2 = stepLmodP2;

        public readonly int[] R1 = r1;
        public readonly int[] R2 = r2;
        public readonly int[] R1P2 = r1p2;
        public readonly int[] R2P2 = r2p2;

        /// <summary>Простые, для которых A ≡ 0 (mod p) или иная деградация – их не используем в решете.</summary>
        public readonly bool[] SkipPrime = skipPrime;

        /// <summary>Число термов B_v в семье.</summary>
        public readonly int TermCount = termCount;

        /// <summary>delta_mod_p[v][i] = (2*B_v*A^{-1}) mod p_i для данного A.</summary>
        public readonly int[][] DeltaModP = deltaModP;

        /// <summary>delta_mod_p2[v][i] = (2*B_v*A^{-1}) mod p_i^2, если используем p^2; может быть null.</summary>
        public readonly int[][]? DeltaModP2 = deltaModP2;
    }

}
