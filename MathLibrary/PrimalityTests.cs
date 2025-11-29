using MathLibrary.Extensions;
using System;
using System.Collections.Concurrent;
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

        public static bool IsStrongProbablePrime(BigInteger n, BigInteger b)
        {
            if (n < 2) return false;
            if (n.IsEven) return n == 2;
            if (BigInteger.GreatestCommonDivisor(b, n) != 1)
                return false;

            BigInteger d = n - 1;
            int s = 0;
            while (d.IsEven)
            {
                d >>= 1;
                s++;
            }

            var x = BigInteger.ModPow(b, d, n);
            if (x == 1 || x == n - 1)
                return true;

            for (int i = 1; i < s; i++)
            {
                x = x * x % n;
                if (x == n - 1)
                    return true;
            }

            return false;
        }

        public static bool IsProbableLucasPrime(BigInteger P, BigInteger Q, BigInteger n)
        {
            BigInteger D = P * P - Q << 2;
            BigInteger k = n - Functions.ArithmeticFunctions.MollerJacobiSymbol(D, n);
            return Sequences.U(k, P, Q) % n == 0;
        }

        public static bool BPSWTest(BigInteger n)
        {
            if (!IsStrongProbablePrime(n, 2))
                return false;
            if (n.IsSqrt())
                return false;
            BigInteger D = 5, P = 1, Q;
            while (Functions.ArithmeticFunctions.MollerJacobiSymbol(D, n) != -1)
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
            if (n.IsEven)
                return n == 2;
            int[] bases;

            if (n < 2047)
                bases = [2];
            else if (n < 1373653)
                bases = [2, 3];
            else if (n < 25326001)
                bases = [2, 3, 5];
            else if (n < 3215031751)
                bases = [2, 3, 5, 7];
            else if (n < 2152302898747)
                bases = [2, 3, 5, 7, 11];
            else if (n < 3474749660383)
                bases = [2, 3, 5, 7, 11, 13];
            else if (n < 3825123056546413051)
                bases = [2, 3, 5, 7, 11, 13, 17, 19, 23];
            else if (n < new BigInteger(new byte[] { 229, 183, 133, 252, 249, 23, 40, 233, 122, 67 }))
                bases = [2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37];
            else if (n < new BigInteger(new byte[] { 253, 165, 16, 36, 178, 197, 173, 81, 105, 190, 2 }))
                bases = [2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41];
            else if (n < new BigInteger(new byte[] { 197, 24, 149, 180, 142, 107, 106, 20, 151, 113, 105, 199, 22, 76 }))
                bases = [2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61];
            else
                bases = Sieves.AtkinSieve((int)Math.Ceiling(BigInteger.Log(n) * Math.Log(BigInteger.Log(n, 2))));

            for (int i = 0; i < bases.Length; i++)
                if (!IsStrongProbablePrime(n, bases[i]))
                    return false;
            return true;
        }
    }
}
