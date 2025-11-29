using MathLibrary.Factorization.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Factorization.SmoothCheckers
{
    public sealed class OneLargePrimeSmoothnessChecker : ISmoothnessChecker
    {
        public SmoothnessResult Check(BigInteger y, ReadOnlySpan<int> FB, in SmoothnessOptions opts)
        {
            int len = FB.Length;
            Span<ushort> tmp = len <= 512 ? stackalloc ushort[len] : new ushort[len];

            BigInteger v = y;
            for (int i = 0; i < len && v != 1; i++)
            {
                int p = FB[i];
                while (v % p == 0) { v /= p; tmp[i]++; }
            }

            if (v.IsOne) return new SmoothnessResult(true, tmp.ToArray(), BigInteger.One);

            if (opts.MaxLargePrimes >= 1 && v > 1 && v <= opts.LargePrimeBound && PrimalityTests.MillerTest(v))
                return new SmoothnessResult(true, tmp.ToArray(), v);

            return new SmoothnessResult(false, null, v);
        }
    }
}
