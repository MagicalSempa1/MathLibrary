using MathLibrary.Factorization.RelationManager;
using MathLibrary.Factorization.SmoothCheckers;
using MathLibrary.Factorization.Types;
using MathLibrary.Functions;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Factorization.BlockSieves
{
    public sealed class SiqsBlockSieveFactory : ISiqsBlockSieveFactory
    {
        private ISmoothnessChecker _smooth = default!;

        public void SetSmoothChecker(ISmoothnessChecker smooth) => _smooth = smooth;

        public ISiqsBlockSieveWorker CreateWorker() => new Worker(_smooth);

        private sealed class Worker : ISiqsBlockSieveWorker
        {
            private const double LN2 = 0.6931471805599453094172321215;

            private readonly ISmoothnessChecker _smooth;
            private readonly ArrayPool<int> _pool = ArrayPool<int>.Shared;
            private int[] _acc = [];

            public Worker(ISmoothnessChecker smooth) => _smooth = smooth;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static int ScaledLowerBoundFromBitLen(in BigInteger v, int scale)
            {
                long bl = v.GetBitLength();
                if (bl <= 1) return 0;
                return (int)((bl - 1) * LN2 * scale + 0.5);
            }

            public SiqsSievePlan BuildInitialPlan(
                MpqsContext ctx,
                SievePrime[] SP,
                QSPolynomial poly,
                BigInteger baseLeft0,
                BigInteger[] bTerms)
            {
                int m = SP.Length;
                int L = ctx.Options.BlockLen;

                var P = new int[m];
                var logp = new ushort[m];
                var hasP2 = new bool[m];
                var P2 = new int[m];
                var r1 = new int[m];
                var r2 = new int[m];
                var r1p2 = new int[m];
                var r2p2 = new int[m];
                var baseLeftModP = new int[m];
                var baseLeftModP2 = new int[m];
                var stepLmodP = new int[m];
                var stepLmodP2 = new int[m];
                var skipPrime = new bool[m];

                int termCount = bTerms.Length;
                int[][] deltaModP = termCount > 0 ? new int[termCount][] : [];
                int[][]? deltaModP2 = null;

                if (termCount > 0)
                {
                    for (int t = 0; t < termCount; t++)
                    {
                        deltaModP[t] = new int[m];
                    }

                    // Есть ли вообще простые с p^2?
                    bool anyP2 = false;
                    for (int i = 0; i < m; i++)
                    {
                        if (SP[i].hasP2) { anyP2 = true; break; }
                    }
                    if (anyP2)
                    {
                        deltaModP2 = new int[termCount][];
                        for (int t = 0; t < termCount; t++)
                            deltaModP2[t] = new int[m];
                    }
                }

                BigInteger A = poly.A;

                for (int k = 0; k < m; k++)
                {
                    ref readonly var sp = ref SP[k];

                    int p = sp.p;
                    P[k] = p;
                    logp[k] = sp.logpScaled;
                    hasP2[k] = sp.hasP2;
                    P2[k] = sp.p2;

                    int left0p = (int)(baseLeft0 % p);
                    if (left0p < 0) left0p += p;
                    baseLeftModP[k] = left0p;
                    stepLmodP[k] = L % p;

                    int Am = (int)(A % p);
                    if (Am < 0) Am += p;

                    if (Am == 0)
                    {
                        // p | A → этот простой не используем в решете (как soln1 = None в Python-коде)
                        skipPrime[k] = true;
                        continue;
                    }

                    skipPrime[k] = false;
                    int invA = Am.Inverse(p);

                    int Bm = (int)(poly.B % p);
                    if (Bm < 0) Bm += p;

                    // Корни по формуле Ax^2 + Bx + C ≡ 0 (mod p) из корней y^2 ≡ N (mod p)
                    r1[k] = (int)((long)(sp.s1 - Bm + p) * invA % p);
                    r2[k] = (int)((long)(sp.s2 - Bm + p) * invA % p);

                    // Δ для каждого B_v: b_ainv[v] = (2*B_v*A^{-1}) mod p
                    if (termCount > 0)
                    {
                        for (int t = 0; t < termCount; t++)
                        {
                            BigInteger Bt = bTerms[t];
                            int BtModP = (int)(Bt % p);
                            if (BtModP < 0) BtModP += p;

                            long twice = 2L * BtModP % p;
                            int delta = (int)(twice * invA % p);
                            deltaModP[t][k] = delta;
                        }
                    }

                    if (sp.hasP2)
                    {
                        int p2 = sp.p2;
                        int left0p2 = (int)(baseLeft0 % p2);
                        if (left0p2 < 0) left0p2 += p2;
                        baseLeftModP2[k] = left0p2;
                        stepLmodP2[k] = L % p2;

                        int Am2 = (int)(A % p2);
                        if (Am2 < 0) Am2 += p2;

                        if (Am2 != 0)
                        {
                            int invA2 = Am2.Inverse(p2);
                            int Bm2 = (int)(poly.B % p2);
                            if (Bm2 < 0) Bm2 += p2;

                            r1p2[k] = (int)((long)(sp.s1p2 - Bm2 + p2) * invA2 % p2);
                            r2p2[k] = (int)((long)(sp.s2p2 - Bm2 + p2) * invA2 % p2);

                            if (deltaModP2 != null)
                            {
                                for (int t = 0; t < termCount; t++)
                                {
                                    BigInteger Bt = bTerms[t];
                                    int BtModP2 = (int)(Bt % p2);
                                    if (BtModP2 < 0) BtModP2 += p2;

                                    long twice2 = 2L * BtModP2 % p2;
                                    int delta2 = (int)(twice2 * invA2 % p2);
                                    deltaModP2[t][k] = delta2;
                                }
                            }
                        }
                        else
                        {
                            // p^2 делит A — p^2 лучше вообще не использовать в решете
                            r1p2[k] = r2p2[k] = 0;
                        }
                    }
                    else
                    {
                        baseLeftModP2[k] = 0;
                        stepLmodP2[k] = 0;
                        r1p2[k] = r2p2[k] = 0;
                    }
                }

                return new SiqsSievePlan(
                    poly,
                    baseLeft0,
                    L,
                    ctx.Options.LogScale,
                    ctx.Options.LogSlack,
                    P,
                    logp,
                    hasP2,
                    P2,
                    baseLeftModP,
                    baseLeftModP2,
                    stepLmodP,
                    stepLmodP2,
                    r1,
                    r2,
                    r1p2,
                    r2p2,
                    skipPrime,
                    termCount,
                    deltaModP,
                    deltaModP2
                );
            }

            public void UpdateRoots(SiqsSievePlan plan, int flippedIndex, int flipSign)
            {
                if (flippedIndex < 0 || flippedIndex >= plan.TermCount || flipSign == 0)
                    return;

                int[] P = plan.P;
                int[] R1 = plan.R1;
                int[] R2 = plan.R2;
                int[] R1P2 = plan.R1P2;
                int[] R2P2 = plan.R2P2;

                int[] deltaP = plan.DeltaModP[flippedIndex];
                int[][]? deltaP2All = plan.DeltaModP2;

                bool[] skip = plan.SkipPrime;
                bool hasP2Array = deltaP2All != null;

                int m = P.Length;

                for (int i = 0; i < m; i++)
                {
                    if (skip[i]) continue;

                    int p = P[i];
                    int d = deltaP[i];
                    if (d != 0)
                    {
                        int s1 = R1[i] - flipSign * d;
                        if (s1 >= p) s1 -= p;
                        else if (s1 < 0) s1 += p;

                        int s2 = R2[i] - flipSign * d;
                        if (s2 >= p) s2 -= p;
                        else if (s2 < 0) s2 += p;

                        R1[i] = s1;
                        R2[i] = s2;
                    }

                    if (hasP2Array && plan.HasP2[i])
                    {
                        int p2 = plan.P2[i];
                        int d2 = deltaP2All![flippedIndex][i];
                        if (d2 != 0)
                        {
                            int t1 = R1P2[i] - flipSign * d2;
                            if (t1 >= p2) t1 -= p2;
                            else if (t1 < 0) t1 += p2;

                            int t2 = R2P2[i] - flipSign * d2;
                            if (t2 >= p2) t2 -= p2;
                            else if (t2 < 0) t2 += p2;

                            R1P2[i] = t1;
                            R2P2[i] = t2;
                        }
                    }
                }
            }

            public void SieveBlock(
                MpqsContext ctx,
                SiqsSievePlan plan,
                long blockIndex,
                IMpqsRelationManager relMgr)
            {
                int L = plan.BlockLen;

                if (_acc.Length < L)
                    _acc = _pool.Rent(L);

                Array.Clear(_acc, 0, L);

                int m = plan.P.Length;

                var P = plan.P;
                var LogP = plan.LogPScaled;
                var HasP2 = plan.HasP2;
                var P2 = plan.P2;
                var baseLeftModP = plan.BaseLeftModP;
                var baseLeftModP2 = plan.BaseLeftModP2;
                var stepLmodP = plan.StepLmodP;
                var stepLmodP2 = plan.StepLmodP2;
                var R1 = plan.R1;
                var R2 = plan.R2;
                var R1P2 = plan.R1P2;
                var R2P2 = plan.R2P2;
                var skip = plan.SkipPrime;

                // Логарифмическое решето по текущим корням
                for (int k = 0; k < m; k++)
                {
                    if (skip[k]) continue;

                    int p = P[k];
                    int logp = LogP[k];

                    int off = baseLeftModP[k]
                              + (int)(blockIndex * (long)stepLmodP[k] % p);
                    if (off >= p) off -= p;

                    int j = R1[k] - off;
                    if (j < 0) j += p;
                    for (; j < L; j += p)
                        _acc[j] += logp;

                    int r2 = R2[k];
                    if (r2 != R1[k])
                    {
                        j = r2 - off;
                        if (j < 0) j += p;
                        for (; j < L; j += p)
                            _acc[j] += logp;
                    }

                    if (HasP2[k] && P2[k] != 0)
                    {
                        int p2 = P2[k];

                        int off2 = baseLeftModP2[k]
                                   + (int)(blockIndex * (long)stepLmodP2[k] % p2);
                        if (off2 >= p2) off2 -= p2;

                        int j2 = R1P2[k] - off2;
                        if (j2 < 0) j2 += p2;
                        for (; j2 < L; j2 += p2)
                            _acc[j2] += logp;

                        int r2p2 = R2P2[k];
                        if (r2p2 != R1P2[k])
                        {
                            j2 = r2p2 - off2;
                            if (j2 < 0) j2 += p2;
                            for (; j2 < L; j2 += p2)
                                _acc[j2] += logp;
                        }
                    }
                }

                var FB = ctx.FB;
                int fbLen = FB.Length;

                var smoothOpts = new SmoothnessOptions(
                    MaxLargePrimes: (int)ctx.Options.EnableLargePrimes,
                    LargePrimeBound: fbLen > 0
                        ? (BigInteger)(FB[^1] * ctx.Options.LargePrimeBoundMultiplier)
                        : BigInteger.Zero
                );

                BigInteger left = plan.BaseLeft0 + (BigInteger)L * blockIndex;

                BigInteger A = plan.Poly.A;
                BigInteger B = plan.Poly.B;

                BigInteger X = A * left + B;
                BigInteger T = X * X - ctx.N;

                BigInteger twoA = A << 1;
                BigInteger A2 = A * A;
                BigInteger incT = twoA * X + A2;
                BigInteger dIncT = A2 << 1;
                BigInteger stepX = A;

                for (int j = 0; j < L; j++)
                {
                    int lower = ScaledLowerBoundFromBitLen(T, plan.LogScale);

                    if (_acc[j] + plan.LogSlack >= lower && !T.IsZero)
                    {
                        bool neg = T.Sign < 0;
                        BigInteger Tabs = neg ? -T : T;

                        var res = _smooth.Check(Tabs, FB, in smoothOpts);
                        if (res.Accepted)
                        {
                            var baseExps = res.Exponents!;
                            var exps = new ushort[fbLen + 1];

                            for (int i = 0; i < fbLen; i++)
                                exps[i] = baseExps[i];

                            // последний "простак" – знак
                            exps[fbLen] = (ushort)(neg ? 1 : 0);

                            relMgr.SubmitCandidate(ctx, X % ctx.N, exps, res.Remainder);
                        }
                    }

                    T += incT;
                    incT += dIncT;
                    X += stepX;
                }
            }
        }
    }
}
