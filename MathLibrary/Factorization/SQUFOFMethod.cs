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
        public static BigInteger[] SQUFOFMethod(BigInteger n)
        {
            var primes = new List<BigInteger>();
            while (n % 2 == 0)
            {
                primes.Add(2);
                n /= 2;
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
                return primes.ToArray();
            if (PrimalityTests.MillerTest(n))
            {
                primes.Add(n);
                return primes.ToArray();
            }
            var root = n.FloorSqrt();
            if (root * root == n)
            {
                primes.AddRange(new[] { root, root });
                return primes.ToArray();
            }
            var multiplyers = new[] {1, 3, 5, 7,
                11, 3*5, 3*7, 3*11,
                5*7, 5*11, 7*11,
                3*5*7, 3*5*11, 3*7*11,
                5*7*11, 3*5*7*11 };
            for (int i = 0; i < multiplyers.Length; i++)
            {
                var nk = n * multiplyers[i];
                root = nk.FloorSqrt();
                (BigInteger p0, BigInteger p1) Pk = (0, root);
                (BigInteger q0, BigInteger q1) Qk = (1, nk - root * root);
                BigInteger d = 0;
                do
                {
                    Pk.p0 = root - (root + Pk.p1) % Qk.q1;
                    Qk.q0 = (nk - Pk.p0 * Pk.p0) / Qk.q1;
                    Pk.p1 = root - (root + Pk.p0) % Qk.q0;
                    Qk.q1 = (nk - Pk.p1 * Pk.p1) / Qk.q0;
                } while (!Qk.q0.IsSqrt(ref d));
                if (d == 1)
                    continue;
                var gcd = BigInteger.GreatestCommonDivisor(n, d);
                if (gcd != 1 && gcd != n)
                {
                    primes.AddRange(new[] { gcd, n / gcd });
                    return primes.ToArray();
                }
                Pk.p0 = -Pk.p0;
                Qk.q0 = d;
                Pk.p1 = root - (Pk.p0 + root) % Qk.q0;
                Qk.q1 = (nk - Pk.p1 * Pk.p1) / Qk.q0;
                do
                {
                    Pk = (Pk.p1, root - (root + Pk.p1) % Qk.q1);
                    Qk = (Qk.q1, (nk - Pk.p1 * Pk.p1) / Qk.q1);
                } while (Pk.p0 != Pk.p1);
                gcd = BigInteger.GreatestCommonDivisor(Qk.q0, n);
                if (gcd == 1 || gcd == n)
                    continue;
                primes.AddRange(new[] { gcd, n / gcd });
                return primes.ToArray();
            }
            throw new Exception("factor is not found");
        }
    }
}
