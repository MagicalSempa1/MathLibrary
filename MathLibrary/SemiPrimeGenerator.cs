using MathLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary
{
    public static class SemiPrimeGenerator
    {
        public static BigInteger[] RandomSemiprimes(
        int digits, int n,
        bool requireDistinctFactors = true,
        bool requireBlumPrimes = true,
        int? smoothnessBound = null,
        int fermatSlackBits = 0,
        int maxTries = 500_000,
        int? seed = null)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(digits);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(n);

            int smoothB = smoothnessBound ?? RecommendSmoothnessBound(digits);
            var result = new List<BigInteger>(n);
            var seen = new HashSet<BigInteger>();

            using var rng = seed.HasValue ? new SeededRng(seed.Value) : RandomNumberGenerator.Create();

            while (result.Count < n)
            {
                var sp = RandomSemiprime(
                    digits,
                    rng,                       // <— используем общий RNG
                    requireDistinctFactors,
                    requireBlumPrimes,
                    smoothB,
                    fermatSlackBits,
                    maxTries);

                if (seen.Add(sp))
                    result.Add(sp);
            }
            return result.ToArray();
        }

        // 3) Перегрузка, принимающая RNG (общая реализация)
        public static BigInteger RandomSemiprime(
            int digits,
            RandomNumberGenerator rng,       // <—
            bool requireDistinctFactors = true,
            bool requireBlumPrimes = true,
            int smoothnessBound = 0,
            int fermatSlackBits = 0,
            int maxTries = 500_000)
        {
            //ArgumentOutOfRangeException.ThrowIfNegativeOrZero(digits);

            int activeSmoothBound = (smoothnessBound > 0)
                ? smoothnessBound
                : RecommendSmoothnessBound(digits);

            var minN = BigInteger.Pow(10, digits - 1);
            var maxN = BigInteger.Pow(10, digits) - 1;

            int dpLo = Math.Max(1, digits / 2 - 1);
            int dpHi = Math.Min(digits - 1, digits - dpLo);

            int[] smallPrimes = Sieves.AtkinSieve(activeSmoothBound);

            long cNoP = 0, cDigits = 0, cNoQ = 0, cSmooth = 0, cGcd = 0, cFermat = 0;

            for (int outer = 0; outer < maxTries; outer++)
            {
                bool localRequireBlum = requireBlumPrimes && outer < 50_000;
                int localFermatSlackBits = (outer >= 100_000) ? Math.Max(fermatSlackBits, 8) : fermatSlackBits;
                int localGcdCap = (outer >= 200_000) ? int.MaxValue : 210;

                int dp = BalancedDigitSplit(dpLo, dpHi, rng);

                var pMin = BigInteger.Pow(10, dp - 1);
                var pMax = BigInteger.Pow(10, dp) - 1;

                var p = RandomPrimeInRange(
                    pMin, pMax, rng,
                    exclude: BigInteger.Zero,
                    accept: x => !localRequireBlum || ((x & 3) == 3)
                );
                if (p == 0) { cNoP++; continue; }

                BigInteger qMin = (minN + p - 1) / p;
                BigInteger qMax = maxN / p;
                if (qMax < 2 || qMin > qMax) { cDigits++; continue; }

                var minQ = BigInteger.Max(qMin, 2);

                var q = RandomPrimeInRange(
                    minQ, qMax, rng,
                    exclude: requireDistinctFactors ? p : BigInteger.Zero,
                    accept: x => !localRequireBlum || ((x & 3) == 3)
                );
                if (q == 0) { cNoQ++; continue; }

                if (IsFullyBSmooth(p - 1, smallPrimes) ||
                    IsFullyBSmooth(p + 1, smallPrimes) ||
                    IsFullyBSmooth(q - 1, smallPrimes) ||
                    IsFullyBSmooth(q + 1, smallPrimes))
                { cSmooth++; continue; }

                if (BigInteger.GreatestCommonDivisor(p - 1, q - 1) > localGcdCap)
                { cGcd++; continue; }

                BigInteger n = p * q;
                if (n < minN || n > maxN) { cDigits++; continue; }

                if (!PassesFermatGapFilter(p, q, n, localFermatSlackBits))
                { cFermat++; continue; }

                return n;
            }

            throw new InvalidOperationException(
                $"Не удалось сгенерировать подходящее полупростое. " +
                $"Диагностика: noP={cNoP}, digitsRange={cDigits}, noQ={cNoQ}, smooth={cSmooth}, gcd={cGcd}, fermat={cFermat}");
        }

        // 4) Старую сигнатуру сохраняем как обёртку (создаёт собственный RNG)
        public static BigInteger RandomSemiprime(
            int digits,
            bool requireDistinctFactors = true,
            bool requireBlumPrimes = true,
            int smoothnessBound = 0,
            int fermatSlackBits = 0,
            int maxTries = 500_000)
        {
            using var rng = RandomNumberGenerator.Create();
            return RandomSemiprime(digits, rng, requireDistinctFactors, requireBlumPrimes,
                                   smoothnessBound, fermatSlackBits, maxTries);
        }

        public static int RecommendSmoothnessBound(int digits, double uTarget = 5.0)
        {
            double log10p = digits / 2.0;

            double log10B_u = log10p / uTarget;
            double Bu = Math.Pow(10.0, log10B_u);

            double Bcap = Math.Pow(10.0, log10p / 2.0) / 8.0;

            double Braw = Math.Min(Bu, Bcap);

            int B = (int)Math.Max(10.0, Math.Min(1_000_000.0, Math.Round(Braw)));
            if (B >= 100_000) B = (B + 9_999) / 10_000 * 10_000;
            else if (B >= 10_000) B = (B + 999) / 1_000 * 1_000;
            else if (B >= 1_000) B = (B + 99) / 100 * 100;
            else if (B >= 100) B = (B + 9) / 10 * 10;

            return Math.Max(10, B);
        }

        private static bool IsFullyBSmooth(BigInteger n, int[] primes)
        {
            if (n < 0) n = -n;
            if (n <= 1) return true;

            foreach (int p in primes)
            {
                if (p <= 1) continue;
                while (n % p == 0) n /= p;
                if (n == 1) return true;
                if ((BigInteger)p * p > n) break;
            }
            return n == 1;
        }

        private static BigInteger RandomPrimeInRange(
            BigInteger min, BigInteger max, RandomNumberGenerator rng,
            BigInteger exclude = default,
            Func<BigInteger, bool>? accept = null)
        {
            if (max < 2 || min > max) return 0;
            if (min < 2) min = 2;

            BigInteger oddMin = min.IsEven ? min + 1 : min;
            if (oddMin > max) return 0;

            BigInteger K = ((max - oddMin) >> 1) + 1; 
            if (K <= 0) return 0;

            for (int attempt = 0; attempt < 5000; attempt++)
            {
                BigInteger k = RandomBelow(K, rng);
                BigInteger x = oddMin + (k << 1);

                if (accept != null && !accept(x))
                {
                    for (int j = 0; j < 4 && !accept(x); j++)
                    {
                        x += 2;
                        if (x > max) x = oddMin;
                    }
                }

                if (exclude != 0 && x == exclude) continue;

                if (PrimalityTests.MillerTest(x)) return x;
            }
            return 0;
        }

        private static BigInteger RandomBelow(BigInteger n, RandomNumberGenerator rng)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(n);
            int len = n.ToByteArray().Length;
            while (true)
            {
                var bytes = new byte[len];
                rng.GetBytes(bytes);
                var unsigned = new byte[len + 1];
                Array.Copy(bytes, unsigned, len);
                var r = new BigInteger(unsigned);
                if (r >= 0 && r < n) return r;
            }
        }

        private static int BalancedDigitSplit(int dpLo, int dpHi, RandomNumberGenerator rng)
        {
            if (dpHi < dpLo) dpHi = dpLo;
            int width = Math.Max(1, dpHi - dpLo + 1);
            Span<byte> b = stackalloc byte[1];
            rng.GetBytes(b);
            int dp = dpLo + (b[0] % width);
            return Math.Clamp(dp, 1, dpHi);
        }

        private static bool PassesFermatGapFilter(BigInteger p, BigInteger q, BigInteger n, int slackBits)
        {
            var gap = BigInteger.Abs(p - q);
            BigInteger threshold = n.FloorNroot(4);
            if (slackBits > 0) threshold <<= slackBits;
            return gap > threshold;
        }
    }
    public sealed class SeededRng : RandomNumberGenerator
    {
        private readonly Random _rnd;
        public SeededRng(int seed) => _rnd = new Random(seed);

        public override void GetBytes(byte[] data) => _rnd.NextBytes(data);

        public override void GetNonZeroBytes(Span<byte> data)
        {
            _rnd.NextBytes(data);
            for (int i = 0; i < data.Length; i++)
                if (data[i] == 0) data[i] = (byte)_rnd.Next(1, 256);
        }
    }
}
