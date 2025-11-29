using MathLibrary.Functions;
using MathLibrary.Extensions;
using MathLibrary.Factorization.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Factorization.PolynomialSource
{
    public sealed class GreedyMpqsPolynomialSource : IPolynomialSource
    {
        private int _cursor;
        private BigInteger _sqrtN;
        private readonly int _blocksPerPoly;
        private readonly int _aMaxBits;

        public GreedyMpqsPolynomialSource(int blocksPerPoly = 3, int aMaxBits = 32)
        {
            _blocksPerPoly = blocksPerPoly;
            _aMaxBits = aMaxBits;
        }

        public int BlocksPerPolynomial => _blocksPerPoly;

        public void Reset(MpqsContext ctx)
        {
            _sqrtN = ctx.N.CeilingSqrt();
            _cursor = 0;
        }

        public bool TryNext(MpqsContext ctx, out QSPolynomial poly)
        {
            var FB = ctx.FB;
            var SP = ctx.SP;

            if (FB.Length == 0)
            {
                poly = default;
                return false;
            }

            BigInteger M = ctx.Options.BlockLen >> 1;
            if (M.IsZero) M = 1;
            BigInteger targetA = _sqrtN / M;
            if (targetA < 3) targetA = 3;

            BigInteger A = BigInteger.One;

            int fbLen = FB.Length;
            int maxKeep = Math.Min(fbLen, 64);
            Span<int> usedIdx = maxKeep <= 64 ? stackalloc int[maxKeep] : new int[maxKeep];
            int usedCount = 0;

            int i = _cursor;
            int limitForCursor = Math.Min(fbLen, 256);

            for (int pass = 0; pass < 2 && usedCount == 0; pass++)
            {
                int start = (pass == 0) ? i : 0;
                for (int k = start; k < fbLen; k++)
                {
                    int p = FB[k];
                    if (p == 2) continue;
                    if (usedCount >= usedIdx.Length) break;

                    BigInteger A2 = A * p;
                    usedIdx[usedCount++] = k;
                    A = A2;

                    if (A.GetBitLength() >= _aMaxBits || A >= targetA)
                        break;
                }
            }

            if (usedCount == 0)
            {
                poly = default;
                return false;
            }

            if (limitForCursor > 0)
                _cursor = (_cursor + 1) % limitForCursor;

            BigInteger r = BigInteger.Zero;
            BigInteger m = BigInteger.One;

            for (int t = 0; t < usedCount; t++)
            {
                int idx = usedIdx[t];
                int p = FB[idx];
                int s = SP[idx].s1;

                int rp = (int)(r % p); if (rp < 0) rp += p;
                int delta = s - rp; if (delta < 0) delta += p;

                int inv = ((int)(m % p)).Inverse(p);
                int stepModP = (int)((long)delta * inv % p);

                if (stepModP != 0)
                    r += (BigInteger)stepModP * m;

                m *= p;
            }

            r %= A; if (r.Sign < 0) r += A;

            BigInteger K = (_sqrtN - r + (A >> 1)) / A;
            BigInteger B = r + K * A;

            BigInteger C = (B * B - ctx.N) / A;

            poly = new QSPolynomial(A, B, C);
            return true;
        }
    }

}
