using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Functions
{
    public static partial class ArithmeticFunctions
    {
        public static (int, int) TonelliShanks(BigInteger a, int p)
        {
            if (p <= 1)
                throw new ArgumentException("p must be greater than 1", nameof(p));

            a = ((a % p) + p) % p;

            switch (p)
            {
                case 2:
                    return a.IsEven ? (0, 0) : (1, 1);
                case 3:
                    var r = a % 3;
                    if (r == 0) return (0, 0);
                    if (r == 1) return (1, 2);
                    return (0, 0);
                case 5:
                    r = a % 5;
                    if (r == 0) return (0, 0);
                    if (r == 1) return (1, 4);
                    if (r == 4) return (2, 3);
                    return (0, 0);
                case 7:
                    r = a % 7;
                    if (r == 0) return (0, 0);
                    if (r == 1) return (1, 6);
                    if (r == 2) return (3, 4);
                    if (r == 4) return (2, 5);
                    return (0, 0);
                default:
                    break;
            }

            if (a == 0)
                return (0, 0);

            if (MollerJacobiSymbol(a, p) != 1)
                return (0, 0);

            if ((p & 3) == 3)
            {
                var root = (int)BigInteger.ModPow(a, (p + 1) >> 2, p);
                return (root, p - root);
            }

            if ((p & 7) == 5)
            {
                var alpha = BigInteger.ModPow(a, (p - 1) >> 2, p);
                var x = BigInteger.ModPow(a, (p + 3) >> 3, p);
                if (alpha != 1)
                    x = (x * BigInteger.One << ((p - 1) >> 2)) % p;

                int root = (int)x;
                return (root, p - root);
            }

            return TonelliShanksGeneral(a, p);
        }

        public static int LiftRootModP2(BigInteger a, int p, int r)
        {
            int p2 = checked(p * p);
            int inv = ((int)(2L * r % p)).Inverse(p);

            BigInteger rr = (BigInteger)r * r - a;
            BigInteger q = rr / p;
            int t = (int)((q % p + p) % p);

            int delta = (int)(1L * t * inv % p);
            int lifted = r - delta * p;
            lifted %= p2;
            if (lifted < 0) lifted += p2;
            return lifted;
        }

        private static (int,int) TonelliShanksGeneral(BigInteger a, int p)
        {
            int Q = p - 1;
            int S = 0;
            while ((Q & 1) == 0)
            {
                Q >>= 1;
                S++;
            }

            int z = FindQuadraticNonResidueOptimized(p);

            int M = S;
            var c = BigInteger.ModPow(z, Q, p);
            var t = BigInteger.ModPow(a, Q, p);
            var R = (int)BigInteger.ModPow(a, (Q + 1) / 2, p);

            while (t != 1)
            {
                if (t == 0)
                    return (0, 0);

                int i = 1;
                BigInteger temp = t;
                while (i < M && BigInteger.ModPow(temp, 1 << i, p) != 1)
                    i++;

                if (i >= M)
                    return (0, 0);

                var b = (int)BigInteger.ModPow(c, 1 << (M - i - 1), p);
                M = i;
                c = BigInteger.ModPow(b, 2, p);
                t = t * c % p;
                R = (int)((long)R * b % p);
            }

            return (R, p - R);
        }

        private static readonly Dictionary<int, int> _nonResidueCache = [];

        private static int FindQuadraticNonResidueOptimized(int p)
        {
            if (_nonResidueCache.TryGetValue(p, out int cached))
                return cached;

            int result;

            int[] smallNonResidues = [2, 2, 2, 3, 2, 2, 3, 2, 5, 2, 3, 2, 6, 3, 2, 2, 2, 2, 7, 5, 2, 2, 2, 3, 2];

            if (p <= 100)
            {
                int[] smallPrimes = [3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97];
                int index = Array.IndexOf(smallPrimes, p);
                if (index >= 0 && index < smallNonResidues.Length)
                {
                    result = smallNonResidues[index];
                    _nonResidueCache[p] = result;
                    return result;
                }
            }

            int[] candidates = [2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47];

            foreach (int candidate in candidates)
            {
                if (candidate >= p) break;
                if (MollerJacobiSymbol(candidate, p) == -1)
                {
                    result = candidate;
                    _nonResidueCache[p] = result;
                    return result;
                }
            }

            for (int z = 2; z < Math.Min(p, 1000); z++)
            {
                if (MollerJacobiSymbol(z, p) == -1)
                {
                    result = z;
                    _nonResidueCache[p] = result;
                    return result;
                }
            }

            throw new InvalidOperationException($"Could not find quadratic non-residue for p = {p}");
        }
    }
}
