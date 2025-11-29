using MathLibrary.Functions;
using MathLibrary.Extensions;
using MathLibrary.Factorization.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Factorization.PolynomialSource
{
    public sealed class LogTargetSwapMpqsPolynomialSource : IPolynomialSource
    {
        private readonly int _blocksPerPoly;
        private readonly int _aMaxBits;
        private readonly int _window;

        private BigInteger _sqrtN;
        private double _targetLogA;
        private int _cursor;
        private int[] _sel = Array.Empty<int>();
        private int _d;
        private BigInteger _A = BigInteger.One;
        private BigInteger _r = BigInteger.Zero;
        private bool _initialized;

        public LogTargetSwapMpqsPolynomialSource(int blocksPerPoly = 3, int aMaxBits = 32, int fbWindow = 256)
        {
            _blocksPerPoly = Math.Max(1, blocksPerPoly);
            _aMaxBits = Math.Max(8, aMaxBits);
            _window = Math.Max(32, fbWindow);
        }

        public int BlocksPerPolynomial => _blocksPerPoly;

        public void Reset(MpqsContext ctx)
        {
            _sqrtN = ctx.N.CeilingSqrt();

            int half = ctx.Options.BlockLen >> 1;
            if (half <= 0) half = 1;
            _targetLogA = 0.5 * BigInteger.Log(ctx.N) - Math.Log(half);

            _cursor = 0;
            _d = 0;
            _A = BigInteger.One;
            _r = BigInteger.Zero;
            _initialized = false;

            int m = ctx.FB.Length;
            if (_sel.Length != m) _sel = new int[m];
        }

        public bool TryNext(MpqsContext ctx, out QSPolynomial poly)
        {
            var FB = ctx.FB;
            var SP = ctx.SP;

            if (FB.Length == 0)
            {
                poly = default;
                return false;
            }

            if (!_initialized)
            {
                if (!BuildInitialA(FB, SP))
                {
                    poly = default; return false;
                }
                _initialized = true;
                BuildPoly(ctx, out poly);
                return true;
            }

            if (!SwapOnePrime(FB, SP))
            {
                _initialized = false;
                if (!BuildInitialA(FB, SP))
                {
                    poly = default; return false;
                }
                _initialized = true;
            }

            BuildPoly(ctx, out poly);
            return true;
        }

        private bool BuildInitialA(ReadOnlySpan<int> FB, ReadOnlySpan<SievePrime> SP)
        {
            double logA = 0.0;
            int m = FB.Length;

            _d = 0;
            _A = BigInteger.One;
            _r = BigInteger.Zero;

            int limitCursor = Math.Min(m, _window);
            int start = _cursor;

            bool stopPicking = false;

            for (int pass = 0; pass < 2 && !stopPicking; pass++)
            {
                int i0 = (pass == 0) ? start : 0;
                for (int i = i0; i < m; i++)
                {
                    int p = FB[i];
                    if (p == 2) continue;

                    if (_A.GetBitLength() >= _aMaxBits)
                    {
                        stopPicking = true;
                        break;
                    }

                    if (logA < _targetLogA || _d == 0)
                    {
                        _sel[_d++] = i;
                        _A *= p;
                        logA += Math.Log(p);

                        AddPrimeToCRT(p, SP[i].s1);

                        if (_A.GetBitLength() >= _aMaxBits || logA >= _targetLogA)
                        {
                            stopPicking = true;
                            break;
                        }
                    }

                    if (i - start >= limitCursor - 1) break;
                }
            }

            if (_d == 0) return false;

            if (limitCursor > 0)
                _cursor = (start + 1) % limitCursor;

            _r %= _A; if (_r.Sign < 0) _r += _A;
            return true;
        }

        private bool SwapOnePrime(ReadOnlySpan<int> FB, ReadOnlySpan<SievePrime> SP)
        {
            int m = FB.Length;
            if (_d == 0) return false;

            int idxOut = _sel[0];
            int pOut = FB[idxOut];

            BigInteger Adiv = _A / pOut;
            BigInteger rDiv = _r % Adiv; if (rDiv.Sign < 0) rDiv += Adiv;

            double baseLog = BigInteger.Log(Adiv);
            double target = _targetLogA;

            int bestIdx = -1;
            int bestP = 0;
            double bestDiff = double.PositiveInfinity;

            int limitCursor = Math.Min(m, _window);
            int start = _cursor;

            bool InCurrentSelection(int idx)
            {
                for (int t = 0; t < _d; t++)
                    if (_sel[t] == idx) return true;
                return false;
            }

            for (int pass = 0; pass < 2; pass++)
            {
                int i0 = (pass == 0) ? start : 0;
                for (int i = i0; i < m; i++)
                {
                    int p = FB[i];
                    if (p == 2) continue;
                    if (i == idxOut) continue;
                    if (InCurrentSelection(i)) continue;

                    if ((Adiv * p).GetBitLength() > _aMaxBits) continue;

                    double diff = Math.Abs(baseLog + Math.Log(p) - target);
                    if (diff < bestDiff)
                    {
                        bestDiff = diff;
                        bestIdx = i;
                        bestP = p;
                    }

                    if (i - start >= limitCursor - 1) break;
                }

                if (bestIdx >= 0) break;
            }

            if (bestIdx < 0)
            {
                for (int i = 0; i < m; i++)
                {
                    int p = FB[i];
                    if (p == 2) continue;
                    if (i == idxOut || InCurrentSelection(i)) continue;
                    if ((Adiv * p).GetBitLength() > _aMaxBits) continue;

                    double diff = Math.Abs(baseLog + Math.Log(p) - target);
                    if (diff < bestDiff)
                    {
                        bestDiff = diff;
                        bestIdx = i;
                        bestP = p;
                    }
                }
            }

            if (bestIdx < 0) return false;

            for (int t = 1; t < _d; t++) _sel[t - 1] = _sel[t];
            _sel[_d - 1] = bestIdx;

            _A = Adiv * bestP;
            _r = rDiv;

            AddPrimeToCRT(bestP, SP[bestIdx].s1, Adiv);

            _r %= _A; if (_r.Sign < 0) _r += _A;

            if (limitCursor > 0)
                _cursor = (start + 1) % limitCursor;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddPrimeToCRT(int p, int s, BigInteger? currentMod = null)
        {
            BigInteger M = currentMod ?? _A / p;
            int Mp = (int)(M % p); if (Mp < 0) Mp += p;

            int inv = Mp.Inverse(p);
            int rp = (int)(_r % p); if (rp < 0) rp += p;

            int delta = s - rp; if (delta < 0) delta += p;
            int step = (int)((long)delta * inv % p);

            if (step != 0)
                _r += (BigInteger)step * M;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BuildPoly(MpqsContext ctx, out QSPolynomial poly)
        {
            BigInteger A = _A;
            BigInteger r = _r;

            BigInteger k = (_sqrtN - r + (A >> 1)) / A;
            BigInteger B = r + k * A;
            BigInteger C = (B * B - ctx.N) / A;

            poly = new QSPolynomial(A, B, C);
        }
    }
}
