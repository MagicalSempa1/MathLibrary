using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Factorization
{
    public static partial class Factorization
    {
        public static BigInteger[] EnumerateMethod(BigInteger n)
        {
            if (n == 2 | n == 3)
                return new BigInteger[] { n };
            List<BigInteger> primes = new List<BigInteger>();
            BigInteger sqrt = n.FloorSqrt();
            if (sqrt <= uint.MaxValue)
                primes.AddRange(PartialTrialDivision(ref n, (uint)sqrt));
            else if (sqrt <= ulong.MaxValue)
                primes.AddRange(PartialTrialDivision(ref n, (ulong)sqrt));
            else if (sqrt <= UInt128.MaxValue)
                primes.AddRange(PartialTrialDivision(ref n, (UInt128)sqrt));
            else
                primes.AddRange(PartialTrialDivision(ref n, sqrt));
            if (n != 1)
                primes.Add(n);
            return primes.ToArray();
        }

    }
}
