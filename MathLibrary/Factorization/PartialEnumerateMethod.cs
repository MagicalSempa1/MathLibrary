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
        private static readonly uint[] fr = [2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97, 101, 103, 107, 109, 113, 127, 131, 137, 139, 149, 151, 157, 163, 167, 169, 173, 179, 181, 193, 197, 199];


        private static readonly uint[] r = [1, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97, 101, 103, 107, 109, 113, 121, 127, 131, 137, 139, 143, 149, 151, 157, 163, 167, 169, 173, 179, 181, 187, 191, 193, 197, 199, 209];

        public static BigInteger[] PartialTrialDivision(ref BigInteger n, uint limit)
        {
            var primes = new List<BigInteger>();
            for (int i = 0; i < fr.Length; i++)
            {
                while (n % fr[i] == 0)
                {
                    primes.Add(fr[i]);
                    n /= fr[i];
                }
            }
            if (limit <= 210)
                return [.. primes];
            limit -= 210;
            uint k;
            for (k = 210; k <= limit; k += 210)
            {
                for (int i = 0; i < r.Length; i++)
                {
                    while (n % (k + r[i]) == 0)
                    {
                        primes.Add(k + r[i]);
                        n /= k + r[i];
                    }
                }
            }
            limit += 210;
            for (int i = 0; i < r.Length; i++)
            {
                if (k + r[i] > limit)
                    break;
                while (n % (k + r[i]) == 0)
                {
                    primes.Add(k + r[i]);
                    n /= k + r[i];
                }
            }
            return [.. primes];
        }

        public static BigInteger[] PartialTrialDivision(ref BigInteger n, ulong limit)
        {
            var primes = new List<BigInteger>();
            for (int i = 0; i < fr.Length; i++)
            {
                while (n % fr[i] == 0)
                {
                    primes.Add(fr[i]);
                    n /= fr[i];
                }
            }
            limit -= 210;
            ulong k;
            for (k = 210; k <= limit; k += 210)
            {
                for (int i = 0; i < r.Length; i++)
                {
                    while (n % (k + r[i]) == 0)
                    {
                        primes.Add(k + r[i]);
                        n /= k + r[i];
                    }
                }
            }
            limit += 210;
            for (int i = 0; i < r.Length; i++)
            {
                if (k + r[i] > limit)
                    break;
                while (n % (k + r[i]) == 0)
                {
                    primes.Add(k + r[i]);
                    n /= k + r[i];
                }
            }
            return [.. primes];
        }

        public static BigInteger[] PartialTrialDivision(ref BigInteger n, UInt128 limit)
        {
            var primes = new List<BigInteger>();
            for (int i = 0; i < fr.Length; i++)
            {
                while (n % fr[i] == 0)
                {
                    primes.Add(fr[i]);
                    n /= fr[i];
                }
            }
            limit -= 210;
            UInt128 k;
            for (k = 210; k <= limit; k += 210)
            {
                for (int i = 0; i < r.Length; i++)
                {
                    while (n % (k + r[i]) == 0)
                    {
                        primes.Add(k + r[i]);
                        n /= k + r[i];
                    }
                }
            }
            limit += 210;
            for (int i = 0; i < r.Length; i++)
            {
                if (k + r[i] > limit)
                    break;
                while (n % (k + r[i]) == 0)
                {
                    primes.Add(k + r[i]);
                    n /= k + r[i];
                }
            }
            return [.. primes];
        }

        public static BigInteger[] PartialTrialDivision(ref BigInteger n, BigInteger limit)
        {
            var primes = new List<BigInteger>();
            for (int i = 0; i < fr.Length; i++)
            {
                while (n % fr[i] == 0)
                {
                    primes.Add(fr[i]);
                    n /= fr[i];
                }
            }
            limit -= 210;
            ulong k;
            for (k = 210; k <= limit; k += 210)
            {
                for (int i = 0; i < r.Length; i++)
                {
                    while (n % (k + r[i]) == 0)
                    {
                        primes.Add(k + r[i]);
                        n /= k + r[i];
                    }
                }
            }
            limit += 210;
            for (int i = 0; i < r.Length; i++)
            {
                if (k + r[i] > limit)
                    break;
                while (n % (k + r[i]) == 0)
                {
                    primes.Add(k + r[i]);
                    n /= k + r[i];
                }
            }
            return [.. primes];
        }
    }
}
