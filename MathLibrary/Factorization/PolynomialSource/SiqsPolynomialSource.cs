using MathLibrary.Extensions;
using MathLibrary.Factorization.Types;
using MathLibrary.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Factorization.PolynomialSource
{
    public sealed class SiqsPolynomialSource : ISiqsPolynomialSource
    {
        private readonly int _blocksPerPoly;
        private readonly int _aMaxBits;
        private readonly int _fbWindow;
        private readonly Random _rng;

        private BigInteger _A = BigInteger.One;
        private BigInteger _B = BigInteger.Zero;
        private BigInteger[] _bTerms = Array.Empty<BigInteger>();
        private int[] _factorIndices = Array.Empty<int>();
        private int _k;                 // число простых в A
        private long _polyIndex;        // индекс полинома в текущей семье
        private long _familySize;       // обычно 2^(k-1)

        public int BlocksPerPolynomial => _blocksPerPoly;

        public BigInteger[] CurrentBTerms => _bTerms;

        public SiqsPolynomialSource(
            int blocksPerPoly = 1,
            int aMaxBits = 32,
            int fbWindow = 256,
            int? seed = null)
        {
            _blocksPerPoly = Math.Max(1, blocksPerPoly);
            _aMaxBits = Math.Max(8, aMaxBits);
            _fbWindow = Math.Max(32, fbWindow);
            _rng = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        public void Reset(MpqsContext ctx)
        {
            if (!BuildInitialFamily(ctx))
            {
                _bTerms = Array.Empty<BigInteger>();
                _polyIndex = 0;
                _familySize = 0;
                return;
            }

            _polyIndex = 0; // первый полином семьи
        }

        public bool TryNext(
            MpqsContext ctx,
            out QSPolynomial poly,
            out int flippedIndex,
            out int flipSign)
        {
            if (_bTerms.Length == 0 || _familySize == 0)
            {
                if (!BuildInitialFamily(ctx))
                {
                    poly = default;
                    flippedIndex = -1;
                    flipSign = 0;
                    return false;
                }
                _polyIndex = 0;
            }

            BigInteger N = ctx.N;

            // Первый полином семьи (аналог _initialize_first_polynomial)
            if (_polyIndex == 0)
            {
                BigInteger C0 = (_B * _B - N) / _A;
                poly = new QSPolynomial(_A, _B, C0);
                flippedIndex = -1;   // новая семья
                flipSign = 0;
                _polyIndex = 1;
                return true;
            }

            // Семья исчерпана — строим новую (новое A, новый набор B_v)
            if (_polyIndex >= _familySize)
            {
                if (!BuildInitialFamily(ctx))
                {
                    poly = default;
                    flippedIndex = -1;
                    flipSign = 0;
                    return false;
                }

                _polyIndex = 1;
                BigInteger C0 = (_B * _B - N) / _A;
                poly = new QSPolynomial(_A, _B, C0);
                flippedIndex = -1;
                flipSign = 0;
                return true;
            }

            // Следующий полином внутри текущей семьи по Gray-схеме
            long i = _polyIndex; // i >= 1

            int v = 1;
            long j = i;
            while ((j & 1) == 0)
            {
                v++;
                j >>= 1;
            }

            long pow2v = 1L << v;
            long ceil = (i + pow2v - 1) >> v;
            int sign = ((ceil & 1L) != 0) ? -1 : 1; // neg_pow в Python-коде

            int termIndex = v - 1;
            if (termIndex < 0 || termIndex >= _k)
            {
                // что-то пошло не так: безопасно перезапустить семью
                if (!BuildInitialFamily(ctx))
                {
                    poly = default;
                    flippedIndex = -1;
                    flipSign = 0;
                    return false;
                }

                _polyIndex = 1;
                BigInteger C0 = (_B * _B - N) / _A;
                poly = new QSPolynomial(_A, _B, C0);
                flippedIndex = -1;
                flipSign = 0;
                return true;
            }

            BigInteger deltaB = 2 * _bTerms[termIndex] * sign;
            _B += deltaB;

            BigInteger C = (_B * _B - N) / _A;
            poly = new QSPolynomial(_A, _B, C);

            flippedIndex = termIndex; // какой B_v меняли
            flipSign = sign;          // +1 или -1

            _polyIndex++;
            return true;
        }

        /// <summary>Строим новую семью полиномов (новое A, массив B_v и B = sum B_v).</summary>
        private bool BuildInitialFamily(MpqsContext ctx)
        {
            var FB = ctx.FB;
            var SP = ctx.SP;
            int m = FB.Length;
            if (m == 0) return false;

            // Цель: A ~ sqrt(2N) / (BlockLen/2)
            double blockHalf = Math.Max(1.0, ctx.Options.BlockLen / 2.0);
            double lnN = BigInteger.Log(ctx.N);
            double lnApproxA = 0.5 * (lnN + Math.Log(2.0)) - Math.Log(blockHalf);

            // Диапазон простых для A ~ [1000, 5000]
            int start = 0;
            while (start < m && FB[start] <= 1000) start++;

            int end = m - 1;
            while (end > start && FB[end] > 5000) end--;

            if (start >= end)
            {
                // fallback: берём всю FB, если она маленькая
                start = 0;
                end = m - 1;
            }

            double bestDiff = double.PositiveInfinity;
            int[]? bestIndices = null;
            const int maxAttempts = 50;
            double ln2 = Math.Log(2.0);

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                var used = new List<int>();
                double logA = 0.0;

                while (logA < lnApproxA && used.Count < _fbWindow)
                {
                    int idx = _rng.Next(start, end + 1);
                    int p = FB[idx];
                    if (p == 2) continue;
                    if (used.Contains(idx)) continue;

                    double newLogA = logA + Math.Log(p);
                    // Ограничение по битовой длине A
                    if (newLogA > _aMaxBits * ln2)
                        continue;

                    used.Add(idx);
                    logA = newLogA;
                }

                if (used.Count == 0) continue;

                double diff = Math.Abs(logA - lnApproxA);
                if (diff < bestDiff)
                {
                    bestDiff = diff;
                    bestIndices = used.ToArray();
                }
            }

            if (bestIndices == null || bestIndices.Length == 0)
                return false;

            Array.Sort(bestIndices);
            _factorIndices = bestIndices;
            _k = bestIndices.Length;
            if (_k == 0) return false;

            // Строим A = ∏ q_i
            BigInteger A = BigInteger.One;
            for (int i = 0; i < _k; i++)
            {
                A *= FB[_factorIndices[i]];
            }

            if (A.GetBitLength() > _aMaxBits)
                return false;

            _A = A;

            // Строим B_v = (A / q_i) * gamma_i, gamma_i из sqrt(N) mod q_i
            _bTerms = new BigInteger[_k];
            for (int i = 0; i < _k; i++)
            {
                int idx = _factorIndices[i];
                int p = FB[idx];

                int tmem = SP[idx].s1;    // sqrt(N) mod p

                BigInteger aOverP = A / p;
                int aOverPmodP = (int)(aOverP % p);
                if (aOverPmodP < 0) aOverPmodP += p;

                int inv = aOverPmodP.Inverse(p);
                int gamma = (int)((long)tmem * inv % p);
                if (gamma > p / 2) gamma = p - gamma;

                _bTerms[i] = aOverP * gamma;
            }

            // B = sum B_v
            BigInteger B = BigInteger.Zero;
            for (int i = 0; i < _k; i++)
                B += _bTerms[i];
            _B = B;

            // Размер семейства: 2^(k-1) полиномов (как в ssiqs.py)
            if (_k <= 1)
            {
                _familySize = 1;
            }
            else if (_k >= 62)
            {
                _familySize = long.MaxValue; // на практике сюда не дойдём
            }
            else
            {
                _familySize = 1L << (_k - 1);
            }

            return true;
        }
    }
}
