using MathLibrary.Factorization.Types;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Factorization.SmoothCheckers
{
    public sealed class EarlyAbortSmoothnessChecker(int cut1 = 1024, int cut2 = 4096, double margin = 0.5) : ISmoothnessChecker, IUsesFactorBase
    {
        private const double LN2 = 0.6931471805599453094;

        private double[] _lnp = [];
        private double[] _lnpref = [];

        private int _cut1 = cut1;
        private int _cut2 = cut2;
        private readonly double _margin = margin;

        public void Bind(ReadOnlySpan<int> fb)
        {
            _lnp = new double[fb.Length];
            _lnpref = new double[fb.Length + 1];

            for (int i = 0; i < fb.Length; i++)
            {
                double lp = Math.Log(fb[i]);
                _lnp[i] = lp;
                _lnpref[i + 1] = _lnpref[i] + lp;
            }

            _cut1 = Math.Clamp(_cut1, 0, fb.Length);
            _cut2 = Math.Clamp(Math.Max(_cut2, _cut1), 0, fb.Length);
        }

        public SmoothnessResult Check(BigInteger value, ReadOnlySpan<int> FB, in SmoothnessOptions options)
        {
            if (value.Sign < 0) value = -value;
            if (value.IsZero) return new SmoothnessResult(false, null, BigInteger.Zero);

            int m = FB.Length;
            int len = m + 1;

            Span<ushort> tmp = len <= 512
                ? stackalloc ushort[len]
                : new ushort[len];

            BigInteger v = value;
            double need = v.GetBitLength() * LN2;
            double got = 0.0;

            int tz = ((v & 1) == 0) ? (int)BigInteger.TrailingZeroCount(v) : 0;
            if (tz > 0)
            {
                v >>= tz;
                tmp[0] += (ushort)tz;
                got += tz * _lnp[0];
                if (v.IsOne)
                    return new SmoothnessResult(true, tmp.ToArray(), BigInteger.One);
            }

            int i = 1;

            for (; i < _cut1 && v != 1; i++)
            {
                int p = FB[i];
                while (v % p == 0)
                {
                    v /= p;
                    tmp[i]++;
                    got += _lnp[i];
                }
            }
            if (v.IsOne)
                return new SmoothnessResult(true, tmp.ToArray(), BigInteger.One);

            double maxGain = _lnpref[_cut2] - _lnpref[i];
            if (got + maxGain < need - _margin)
                return new SmoothnessResult(false, null, v);

            for (; i < _cut2 && v != 1; i++)
            {
                int p = FB[i];
                while (v % p == 0)
                {
                    v /= p;
                    tmp[i]++;
                }
            }
            if (v.IsOne)
                return new SmoothnessResult(true, tmp.ToArray(), BigInteger.One);

            if (v.GetBitLength() <= 64)
            {
                ulong u = (ulong)v;
                for (; i < m && u != 1; i++)
                {
                    uint p = (uint)FB[i];
                    while (u % p == 0)
                    {
                        u /= p;
                        tmp[i]++;
                    }
                }
                if (u == 1)
                    return new SmoothnessResult(true, tmp.ToArray(), BigInteger.One);

                v = u;
            }

            return new SmoothnessResult(false, null, v);
        }
    }
}
