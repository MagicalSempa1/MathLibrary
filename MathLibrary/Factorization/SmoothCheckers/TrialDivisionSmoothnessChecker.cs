using MathLibrary.Factorization.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Factorization.SmoothCheckers
{
    public sealed class TrialDivisionSmoothnessChecker : ISmoothnessChecker
    {
        public SmoothnessResult Check(BigInteger x, ReadOnlySpan<int> FB, in SmoothnessOptions options)
        {
            int len = FB.Length;
            Span<ushort> tmp = len <= 512 ? stackalloc ushort[len] : new ushort[len];

            BigInteger v = x;
            for (int i = 0; i < len && v != 1; i++)
            {
                int p = FB[i];
                while (v % p == 0)
                {
                    v /= p;
                    tmp[i]++;
                }
            }

            if (v == 1)
            {
                return new SmoothnessResult(true, tmp.ToArray(), BigInteger.One);
            }

            return new SmoothnessResult(false, null, v);
        }
    }
}
