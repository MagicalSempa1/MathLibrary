using MathLibrary.Extensions;
using System.Numerics;

namespace MathLibrary.Factorization
{
    public static partial class Factorization
    {
        public static BigInteger[] LehmanMethod(BigInteger n)
        {
            if (PrimalityTests.MillerTest(n))
                return [n];
            var primes = new List<BigInteger>();
            BigInteger qbrt = n.FloorNroot(3);
            if (qbrt <= uint.MaxValue)
                primes.AddRange(PartialTrialDivision(ref n, (uint)qbrt));
            else if (qbrt <= ulong.MaxValue)
                primes.AddRange(PartialTrialDivision(ref n, (ulong)qbrt));
            else if (qbrt <= UInt128.MaxValue)
                primes.AddRange(PartialTrialDivision(ref n, (UInt128)qbrt));
            else
                primes.AddRange(PartialTrialDivision(ref n, qbrt));

            if (n == 1) return [.. primes];

            if (primes.Count > 0 && PrimalityTests.MillerTest(n))
            {
                primes.Add(n);
                return [.. primes];
            }
            qbrt = n.FloorNroot(3);

            if (qbrt <= uint.MaxValue)
                return LehmanMethodCore(ref n, ref primes, (uint)qbrt);
            else if (qbrt <= ulong.MaxValue)
                return LehmanMethodCore(ref n, ref primes, (ulong)qbrt);
            else if (qbrt <= UInt128.MaxValue)
                return LehmanMethodCore(ref n, ref primes, (UInt128)qbrt);
            else
                return LehmanMethodCore(ref n, ref primes, qbrt);
        }

        private static BigInteger[] LehmanMethodCore(ref BigInteger n, ref List<BigInteger> primes, uint qbrt)
        {
            BigInteger t, A, f;
            BigInteger B = 0;
            BigInteger n4 = n << 2;
            uint sqrtK, L;
            uint sixroot = (uint)n.CeilingNroot(6);
            for (uint k = 1; k <= qbrt; k++)
            {
                sqrtK = (uint)Math.Floor(Math.Sqrt(k));
                L = sixroot / (sqrtK << 2);
                if (L == 0) L = 1;
                t = k * n4;
                A = t.CeilingSqrt();
                f = A * A - t;
                var S = (A << 1) + 1;
                for (uint d = 0; d < L; d++)
                {
                    if (f.IsSqrt(ref B))
                    {
                        var gcd = BigInteger.GreatestCommonDivisor(A - B, n);
                        if (gcd != 1 && gcd != n)
                        {
                            primes.Add(gcd);
                            primes.Add(n / gcd);
                            return [.. primes];
                        }
                        gcd = BigInteger.GreatestCommonDivisor(A + B, n);
                        if (gcd != 1 && gcd != n)
                        {
                            primes.Add(gcd);
                            primes.Add(n / gcd);
                            return [.. primes];
                        }
                    }
                    f += S;
                    S += 2;
                    A++;
                }
            }
            return [.. primes];
        }

        private static BigInteger[] LehmanMethodCore(ref BigInteger n, ref List<BigInteger> primes, ulong qbrt)
        {
            BigInteger t, A, f;
            BigInteger B = 0;
            BigInteger n4 = n << 2;
            ulong sqrtK, L;
            var sixroot = (ulong)n.CeilingNroot(6);
            for (ulong k = 1; k <= qbrt; k++)
            {
                sqrtK = (ulong)Math.Floor(Math.Sqrt(k));
                L = sixroot / (sqrtK << 2);
                if (L == 0) L = 1;
                t = k * n4;
                A = t.CeilingSqrt();
                f = A * A - t;
                var S = (A << 1) + 1;
                for (ulong d = 0; d < L; d++)
                {
                    if (f.IsSqrt(ref B))
                    {
                        BigInteger gcd = BigInteger.GreatestCommonDivisor(A - B, n);
                        if (gcd != 1 && gcd != n)
                        {
                            primes.Add(gcd);
                            primes.Add(n / gcd);
                            return [.. primes];
                        }
                        gcd = BigInteger.GreatestCommonDivisor(A + B, n);
                        if (gcd != 1 && gcd != n)
                        {
                            primes.Add(gcd);
                            primes.Add(n / gcd);
                            return primes.ToArray();
                        }
                    }
                    f += S;
                    S += 2;
                    A++;
                }
            }
            return [.. primes];
        }

        private static BigInteger[] LehmanMethodCore(ref BigInteger n, ref List<BigInteger> primes, UInt128 qbrt)
        {
            BigInteger t, A, f;
            BigInteger B = 0;
            BigInteger n4 = n << 2;
            UInt128 sqrtK, L;
            var sixroot = (UInt128)n.CeilingNroot(6);
            for (ulong k = 1; k <= qbrt; k++)
            {
                sqrtK = (UInt128)Math.Floor(Math.Sqrt(k));
                L = sixroot / (sqrtK << 2);
                if (L == 0) L = 1;
                t = k * n4;
                A = t.CeilingSqrt();
                f = A * A - t;
                var S = (A << 1) + 1;
                for (UInt128 d = 0; d < L; d++)
                {
                    if (f.IsSqrt(ref B))
                    {
                        BigInteger gcd = BigInteger.GreatestCommonDivisor(A - B, n);
                        if (gcd != 1 && gcd != n)
                        {
                            primes.Add(gcd);
                            primes.Add(n / gcd);
                            return [.. primes];
                        }
                        gcd = BigInteger.GreatestCommonDivisor(A + B, n);
                        if (gcd != 1 && gcd != n)
                        {
                            primes.Add(gcd);
                            primes.Add(n / gcd);
                            return [.. primes];
                        }
                    }
                    f += S;
                    S += 2;
                    A++;
                }
            }
            return [.. primes];
        }

        private static BigInteger[] LehmanMethodCore(ref BigInteger n, ref List<BigInteger> primes, BigInteger qbrt)
        {
            BigInteger t, A, f;
            BigInteger B = 0;
            BigInteger n4 = n << 2;
            BigInteger sqrtK, L;
            var sixroot = n.CeilingNroot(6);
            for (BigInteger k = 1; k <= qbrt; k++)
            {
                sqrtK = k.FloorSqrt();
                L = sixroot / (sqrtK << 2);
                if (L == 0) L = 1;
                t = k * n4;
                A = t.CeilingSqrt();
                f = A * A - t;
                var S = (A << 1) + 1;
                for (BigInteger d = 0; d < L; d++)
                {
                    if (f.IsSqrt(ref B))
                    {
                        var gcd = BigInteger.GreatestCommonDivisor(A - B, n);
                        if (gcd != 1 && gcd != n)
                        {
                            primes.Add(gcd);
                            primes.Add(n / gcd);
                            return [.. primes];
                        }
                        gcd = BigInteger.GreatestCommonDivisor(A + B, n);
                        if (gcd != 1 && gcd != n)
                        {
                            primes.Add(gcd);
                            primes.Add(n / gcd);
                            return [.. primes];
                        }
                    }
                    f += S;
                    S += 2;
                    A++;
                }
            }
            return [.. primes];
        }
    }
}
