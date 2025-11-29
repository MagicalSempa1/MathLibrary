using MathLibrary.Factorization.Types;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Factorization.SmoothCheckers
{
    public sealed class EarlyAbortOneLargePrimeSmoothnessChecker : ISmoothnessChecker, IUsesFactorBase
    {
        private const double LN2 = 0.6931471805599453094172321215;

        private double[] _lnp = Array.Empty<double>();
        private double[] _lnpref = Array.Empty<double>();

        private int _cut1;
        private int _cut2;
        private readonly double _margin;

        public EarlyAbortOneLargePrimeSmoothnessChecker(int cut1 = 1024, int cut2 = 4096, double margin = 0.5)
        {
            _cut1 = cut1;
            _cut2 = cut2;
            _margin = margin;
        }

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
            if (m == 0) return new SmoothnessResult(false, null, value);

            if (m <= 512)
            {
                Span<ushort> tmp = stackalloc ushort[m];
                return CheckCore(value, FB, options, tmp);
            }
            else
            {
                ushort[] rented = ArrayPool<ushort>.Shared.Rent(m);
                try
                {
                    Span<ushort> tmp = rented.AsSpan(0, m);
                    return CheckCore(value, FB, options, tmp);
                }
                finally
                {
                    ArrayPool<ushort>.Shared.Return(rented, clearArray: true);
                }
            }
        }

        private SmoothnessResult CheckCore(
            BigInteger value,
            ReadOnlySpan<int> FB,
            in SmoothnessOptions options,
            Span<ushort> tmp)
        {
            int m = FB.Length;
            tmp.Clear();

            BigInteger v = value;

            long bitLen = v.GetBitLength();
            double need = bitLen > 1 ? (bitLen - 1) * LN2 : 0.0;

            bool allowLP = options.MaxLargePrimes >= 1 && options.LargePrimeBound > 0;
            double lpLog = 0.0;
            if (allowLP)
            {
                long lpBits = options.LargePrimeBound.GetBitLength();
                lpLog = lpBits > 1 ? (lpBits - 1) * LN2 : 0.0;
            }

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
            if (got + maxGain + lpLog < need - _margin)
                return new SmoothnessResult(false, null, v);

            for (; i < _cut2 && v != 1; i++)
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


            if (allowLP && v.Sign > 0 && v <= options.LargePrimeBound)
            {
                return new SmoothnessResult(true, tmp.ToArray(), v);
            }

            return new SmoothnessResult(false, null, v);
        }
    }
}
