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
        public static BigInteger[] LehmanMethod(BigInteger n)
        {
            if (PrimalityTests.MillerTest(n))
                return new[] { n };
            List<BigInteger> primes = new List<BigInteger>();
            BigInteger qbrt = n.FloorNroot(3);
            if (qbrt <= uint.MaxValue)
                primes.AddRange(PartialTrialDivision(ref n, (uint)qbrt));
            else if (qbrt <= ulong.MaxValue)
                primes.AddRange(PartialTrialDivision(ref n, (ulong)qbrt));
            else if (qbrt <= UInt128.MaxValue)
                primes.AddRange(PartialTrialDivision(ref n, (UInt128)qbrt));
            else
                primes.AddRange(PartialTrialDivision(ref n, qbrt));
            if (primes.Count > 0 && PrimalityTests.MillerTest(n))
            {
                primes.Add(n);
                return primes.ToArray();
            }
            qbrt = n.FloorNroot(3);
            var sixroot = n.CeillingNroot(6);
            BigInteger sqrtK, L, A, f;
            BigInteger B = 0;
            BigInteger t = n << 2;
            for (BigInteger k = 1; k <= qbrt; k++)
            {
                sqrtK = k.FloorSqrt();
                L = sixroot / (sqrtK << 2) + 1;
                A = t.FloorSqrt();
                for (BigInteger d = 0; d <= L; d++)
                {
                    f = BigInteger.Pow(A, 2) - t;
                    if (f < 0)
                    {
                        A++;
                        continue;
                    }
                    if (f.IsSqrt(ref B))
                    {
                        BigInteger gcd = BigInteger.GreatestCommonDivisor(A - B, n);
                        if (gcd != 1 && gcd != n)
                        {
                            primes.Add(gcd);
                            primes.Add(n / gcd);
                            return primes.ToArray();
                        }
                        gcd = BigInteger.GreatestCommonDivisor(A + B, n);
                        if (gcd != 1 && gcd != n)
                        {
                            primes.Add(gcd);
                            primes.Add(n / gcd);
                            return primes.ToArray();
                        }
                    }
                    A++;
                }
                t += n << 2;
            }
            primes.Add(n);
            return primes.ToArray();
        }

    }
}
