using MathLibrary.Factorization.RelationManager;
using MathLibrary.Factorization.SmoothCheckers;
using MathLibrary.Factorization.Types;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Factorization.BlockSieves
{
    public sealed class SpqsBlockSieveFactory : ISpqsBlockSieveFactory
    {
        private ISmoothnessChecker _smooth = default!;

        public void SetSmoothChecker(ISmoothnessChecker smooth) => _smooth = smooth;

        public ISpqsBlockSieveWorker CreateWorker() => new Worker(_smooth);

        private sealed class Worker : ISpqsBlockSieveWorker
        {
            private const double LN2 = 0.6931471805599453094;

            private readonly ISmoothnessChecker _smooth;
            private ushort[] _acc = [];

            public Worker(ISmoothnessChecker smooth) => _smooth = smooth;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static int ScaledLowerBoundFromBitLen(in BigInteger v, int scale)
            {
                long bl = v.GetBitLength();
                if (bl <= 1) return 0;
                return (int)((bl - 1) * LN2 * scale + 0.5);
            }

            public SpqsSievePlan BuildPlan(SpqsContext ctx, SievePrime[] SP, BigInteger baseLeft0)
            {
                int m = SP.Length;
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

                int L = ctx.Options.BlockLen;

                for (int k = 0; k < m; k++)
                {
                    ref readonly var sp = ref SP[k];

                    int p = sp.p;
                    P[k] = p;
                    logp[k] = sp.logpScaled;
                    hasP2[k] = sp.hasP2;
                    P2[k] = sp.p2;

                    r1[k] = sp.s1;
                    r2[k] = sp.s2;

                    if (sp.hasP2)
                    {
                        r1p2[k] = sp.s1p2;
                        r2p2[k] = sp.s2p2;
                    }

                    int left0p = (int)(baseLeft0 % p);
                    if (left0p < 0) left0p += p;
                    baseLeftModP[k] = left0p;
                    stepLmodP[k] = L % p;

                    if (sp.hasP2)
                    {
                        int p2 = sp.p2;
                        int left0p2 = (int)(baseLeft0 % p2);
                        if (left0p2 < 0) left0p2 += p2;
                        baseLeftModP2[k] = left0p2;
                        stepLmodP2[k] = L % p2;
                    }
                }

                return new SpqsSievePlan(
                    baseLeft0,
                    L,
                    ctx.Options.LogScale,
                    ctx.Options.LogSlack,
                    P,
                    logp,
                    hasP2,
                    P2,
                    r1,
                    r2,
                    r1p2,
                    r2p2,
                    baseLeftModP,
                    baseLeftModP2,
                    stepLmodP,
                    stepLmodP2
                );
            }

            public void SieveBlock(
    SpqsContext ctx,
    in SpqsSievePlan plan,
    long blockIndex,
    ISpqsRelationManager relMgr)
            {
                int L = plan.BlockLen;

                if (_acc.Length < L)
                    _acc = new ushort[L];
                else
                    Array.Clear(_acc, 0, L);

                int m = plan.P.Length;

                for (int k = 0; k < m; k++)
                {
                    int p = plan.P[k];
                    ushort logp = plan.LogPScaled[k];

                    int shift = (int)(blockIndex * plan.StepLmodP[k] % p);
                    if (shift < 0) shift += p;

                    int off = plan.BaseLeftModP[k] + shift;
                    if (off >= p) off -= p;

                    int j = plan.R1[k] - off;
                    if (j < 0) j += p;
                    for (; j < L; j += p)
                        _acc[j] += logp;

                    int r2 = plan.R2[k];
                    if (r2 != plan.R1[k])
                    {
                        j = r2 - off;
                        if (j < 0) j += p;
                        for (; j < L; j += p)
                            _acc[j] += logp;
                    }

                    if (plan.HasP2[k])
                    {
                        int p2 = plan.P2[k];

                        int shift2 = (int)(blockIndex * plan.StepLmodP2[k] % p2);
                        if (shift2 < 0) shift2 += p2;

                        int off2 = plan.BaseLeftModP2[k] + shift2;
                        if (off2 >= p2) off2 -= p2;

                        int j2 = plan.R1P2[k] - off2;
                        if (j2 < 0) j2 += p2;
                        for (; j2 < L; j2 += p2)
                            _acc[j2] += logp;

                        int r2p2 = plan.R2P2[k];
                        if (r2p2 != plan.R1P2[k])
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
                BigInteger x = left;
                BigInteger xModN = left % ctx.N;
                if (xModN.Sign < 0) xModN += ctx.N;

                BigInteger y = x * x - ctx.N;
                BigInteger inc = (x << 1) + 1;

                for (int j = 0; j < L; j++)
                {
                    int lower = ScaledLowerBoundFromBitLen(y, plan.LogScale);

                    if (_acc[j] + plan.LogSlack >= lower && !y.IsZero)
                    {
                        BigInteger ay = y.Sign < 0 ? -y : y;
                        var res = _smooth.Check(ay, FB, in smoothOpts);

                        if (res.Accepted)
                            relMgr.SubmitCandidate(ctx, xModN, res.Exponents!, res.Remainder);
                    }

                    y += inc;
                    inc += 2;

                    x += 1;
                    xModN += 1;
                    if (xModN >= ctx.N) xModN -= ctx.N;
                }
            }
        }
    }
}
