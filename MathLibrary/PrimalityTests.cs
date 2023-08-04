using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary
{
    public static class PrimalityTests
    {
        public static bool EnumTest(BigInteger n)
        {
            if (n % 2 == 0)
                return n == 2;
            if (n % 3 == 0)
                return n == 3;
            BigInteger sqrt = n.FloorSqrt();
            if (sqrt * sqrt == n)
                return false;
            for (BigInteger i = 5; i <= sqrt; i += 6)
            {
                if (n % i == 0)
                    return false;
                if (n % (i + 2) == 0)
                    return false;
            }
            return true;
        }

        public static bool FermatTest(BigInteger a, BigInteger n) => BigInteger.ModPow(a, n - 1, n) == 1;

        public static bool FrobeniusTest()
        {
            throw new Exception();
        }

        public static bool LucasLehmerTest(BigInteger n)
        {
            if (n == 1)
                return false;
            if (n == 2)
                return true;
            var M = Sequences.MersenneNumber(n);
            BigInteger s = 4;
            for (BigInteger i = 1; i < n - 1; i++)
                s = BigInteger.ModPow(s, 2, M) - 2;
            return s % M == 0;
        }

        public static bool PepinTest(int n)
        {
            var F = Sequences.FermatNumber(n);
            var t = Sequences.MersenneNumber(n);
            BigInteger b = 3;
            for (BigInteger i = 0; i < t; i++)
                b = BigInteger.ModPow(b, 2, F);
            return b == F - 1;
        }

        public static bool IsStrongProbablePrime(BigInteger b, BigInteger n)
        {
            if (n.IsEven)
                return n == 2;
            if (BigInteger.GreatestCommonDivisor(b, n) > 1)
                return false;
            var r = (n - 1) >> 1;
            for (; r.IsEven; r >>= 1)
            {
                if (BigInteger.ModPow(b, r, n) == n - 1)
                    return true;
            }
            var t = BigInteger.ModPow(b, r, n);
            if (t == 1 | t == n - 1)
                return true;
            return false;
        }

        public static bool IsProbableLucasPrime(BigInteger P, BigInteger Q, BigInteger n)
        {
            BigInteger D = P * P - Q << 2;
            BigInteger k = n - ArithmeticFunctions.JacobiSymbol(D, n);
            return Sequences.U(k, P, Q) % n == 0;
        }

        public static bool BPSWTest(BigInteger n)
        {
            if (!IsStrongProbablePrime(2, n))
                return false;
            if (n.IsSqrt())
                return false;
            BigInteger D = 5, P = 1, Q;
            while (ArithmeticFunctions.JacobiSymbol(D, n) != -1)
                D = D > 0 ? -(D + 2) : -(D - 2);
            Q = (1 - D) >> 4;
            BigInteger s = (n - 1) >> 1;
            do
            {

            } while (s % 2 == 0);

            return true;
        }

        public static bool MillerTest(BigInteger n)
        {
            if (n % 2 == 0)
                return n == 2;
            int limit;
            int[] bases;
            if (n < 2047)
                bases = new[] { 2 };
            else if (n < 1373653)
                bases = new[] { 2, 3 };
            else if (n < 25326001)
                bases = new[] { 2, 3, 5 };
            else if (n < 3215031751)
                bases = new[] { 2, 3, 5, 7 };
            else if (n < 2152302898747)
                bases = new[] { 2, 3, 5, 7, 11 };
            else if (n < 3474749660383)
                bases = new[] { 2, 3, 5, 7, 11, 13 };
            else if (n < 3825123056546413051)
                bases = new[] { 2, 3, 5, 7, 11, 13, 17, 19, 23 };
            else if (n < BigInteger.Parse("318665857834031151167461"))
                bases = new[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37 };
            else if (n < BigInteger.Parse("3317044064679887385961981"))
                bases = new[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41 };
            else if (n < BigInteger.Parse("1543267864443420616877677640751301"))
                bases = new[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61 };
            else
            {
                limit = (int)(Math.Ceiling(BigInteger.Log(n) * Math.Log(BigInteger.Log(n, 2))));
                bases = Sieves.AtkinSieve(limit);
            }
            for (int i = 0; i < bases.Length; i++)
                if (!IsStrongProbablePrime(bases[i], n))
                    return false;
            return true;
        }
    }
}
