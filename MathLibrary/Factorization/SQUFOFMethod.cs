using MathLibrary.Extensions;
using System.Numerics;

namespace MathLibrary.Factorization
{
    public static partial class Factorization
    {
        public static BigInteger[] SQUFOFMethod(BigInteger n)
        {
            if (n <= 1) return [];
            var primes = new List<BigInteger>();
            while (n.IsEven)
            {
                primes.Add(2);
                n >>= 1;
            }
            while (n % 3 == 0)
            {
                primes.Add(3);
                n /= 3;
            }
            while (n % 5 == 0)
            {
                primes.Add(5);
                n /= 5;
            }
            while (n % 7 == 0)
            {
                primes.Add(7);
                n /= 7;
            }
            while (n % 11 == 0)
            {
                primes.Add(11);
                n /= 11;
            }
            if (n == 1)
                return [.. primes];
            if (PrimalityTests.MillerTest(n))
            {
                primes.Add(n);
                return [.. primes];
            }
            BigInteger root = 0;
            if (n.IsSqrt(ref root))
            {
                primes.AddRange([root, root]);
                return [.. primes];
            }
            int[] multipliers = [1, 3, 5, 7,
                11, 3*5, 3*7, 3*11,
                5*7, 5*11, 7*11,
                3*5*7, 3*5*11, 3*7*11,
                5*7*11, 3*5*7*11];

            var entries = new (int k, BigInteger nk, BigInteger root, BigInteger Q,
                   int jacobi, bool noAmb2)[multipliers.Length];

            for (int i = 0; i < multipliers.Length; i++)
            {
                int k = multipliers[i];
                var nk = n * k;
                var r0 = nk.FloorSqrt();
                var q0 = nk - r0 * r0;
                var jacobi = Functions.ArithmeticFunctions.MollerJacobiSymbol(k, n);
                var noAmb2 = TwoIsInert(nk);
                entries[i] = (k, nk, r0, q0, jacobi, noAmb2);
            }

            Array.Sort(entries, (a, b) =>
            {
                int riskA = (a.jacobi == 1 && !a.noAmb2 && a.Q <= 9) ? 1 : 0;
                int riskB = (b.jacobi == 1 && !b.noAmb2 && b.Q <= 9) ? 1 : 0;
                int byRisk = riskA.CompareTo(riskB);
                return byRisk != 0 ? byRisk : a.Q.CompareTo(b.Q);
            });


            for (int i = 0; i < entries.Length; i++)
            {
                var nk = entries[i].nk;
                root = entries[i].root;
                BigInteger Pprev = root, P = root, Qprev = 1, Q = entries[i].Q, q, b;
                if (Q == 0 || Q == 1) continue;
                BigInteger d = 0;
                for (; ; )
                {
                    b = (root + P) / Q;
                    P = b * Q - P;
                    q = Q;
                    Q = Qprev + b * (Pprev - P);
                    if (Q.IsSqrt(ref d))
                        break;
                    Qprev = q;
                    Pprev = P;
                    b = (root + P) / Q;
                    P = b * Q - P;
                    q = Q;
                    Q = Qprev + b * (Pprev - P);
                    Qprev = q;
                    Pprev = P;
                }
                if (d == 1)
                    continue;
                var gcd = BigInteger.GreatestCommonDivisor(n, d);
                if (gcd != 1 && gcd != n)
                {
                    primes.AddRange([gcd, n / gcd]);
                    return [.. primes];
                }
                b = (root - P) / d;
                Pprev = P = b * d + P;
                Qprev = d;
                Q = (nk - Pprev * Pprev) / Qprev;
                do
                {
                    b = (root + P) / Q;
                    Pprev = P;
                    P = b * Q - P;
                    q = Q;
                    Q = Qprev + b * (Pprev - P);
                    Qprev = q;
                } while (P != Pprev);
                gcd = BigInteger.GreatestCommonDivisor(Qprev, n);
                if (gcd == 1 || gcd == n)
                    continue;
                if (PrimalityTests.MillerTest(gcd))
                    primes.Add(gcd);
                else
                    primes.AddRange(SQUFOFMethod(gcd));
                n /= gcd;
                if (PrimalityTests.MillerTest(n))
                    primes.Add(n);
                else
                    primes.AddRange(SQUFOFMethod(n));
                return [.. primes];
            }
            throw new InvalidOperationException("SQUFOF failed to find a factor with given multipliers");
        }

        private static bool TwoIsInert(BigInteger kn)
        {
            var DeltaMod8 = ((kn & 3) == 1) ? (kn & 7) : ((kn << 2) & 7);
            return DeltaMod8 == 5;
        }
    }
}