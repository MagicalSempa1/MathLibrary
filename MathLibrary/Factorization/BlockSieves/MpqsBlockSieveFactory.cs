using MathLibrary.Functions;
using MathLibrary.Factorization.RelationManager;
using MathLibrary.Factorization.SmoothCheckers;
using MathLibrary.Factorization.Types;
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MathLibrary.Factorization.BlockSieves
{
    public sealed class MpqsBlockSieveFactory : IMpqsBlockSieveFactory
    {
        private ISmoothnessChecker _smooth = default!;

        public void SetSmoothChecker(ISmoothnessChecker smooth) => _smooth = smooth;

        public IMpqsBlockSieveWorker CreateWorker() => new Worker(_smooth);

        private sealed class Worker : IMpqsBlockSieveWorker
        {
            private const double LN2 = 0.6931471805599453094172321215;

            private readonly ISmoothnessChecker _smooth;
            private readonly ArrayPool<int> _pool = ArrayPool<int>.Shared;
            private int[] _acc = Array.Empty<int>();

            public Worker(ISmoothnessChecker smooth) => _smooth = smooth;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static int ScaledLowerBoundFromBitLen(in BigInteger v, int scale)
            {
                long bl = v.GetBitLength();
                if (bl <= 1) return 0;
                return (int)((bl - 1) * LN2 * scale + 0.5);
            }

            public PolySievePlan BuildPlan(
                MpqsContext ctx,
                SievePrime[] SP,
                QSPolynomial poly,
                BigInteger baseLeft0)
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
                bool isMonic = poly.IsMonicX2MinusN(ctx.N);

                // Для MPQS мы не используем экспоненты A, поэтому даём пустой массив
                ushort[] aExps = [];
                bool hasAExps = false;

                for (int k = 0; k < m; k++)
                {
                    ref readonly var sp = ref SP[k];

                    int p = sp.p;
                    P[k] = p;
                    logp[k] = sp.logpScaled;
                    hasP2[k] = sp.hasP2;
                    P2[k] = sp.p2;

                    if (isMonic)
                    {
                        // на случай, если кто-то захочет использовать монный полином
                        r1[k] = sp.s1;
                        r2[k] = sp.s2;

                        if (sp.hasP2)
                        {
                            r1p2[k] = sp.s1p2;
                            r2p2[k] = sp.s2p2;
                        }
                    }
                    else
                    {
                        int Am = (int)(poly.A % p);
                        if (Am < 0) Am += p;
                        int invA = Am.Inverse(p);

                        int Bm = (int)(poly.B % p);
                        if (Bm < 0) Bm += p;

                        // Ax + B ≡ ±sqrt(N) (mod p)
                        r1[k] = (int)((long)(sp.s1 - Bm + p) * invA % p);
                        r2[k] = (int)((long)(sp.s2 - Bm + p) * invA % p);

                        if (sp.hasP2)
                        {
                            int p2 = sp.p2;

                            int Am2 = (int)(poly.A % p2);
                            if (Am2 < 0) Am2 += p2;

                            if (Am2 != 0)
                            {
                                int invA2 = Am2.Inverse(p2);
                                int Bm2 = (int)(poly.B % p2);
                                if (Bm2 < 0) Bm2 += p2;

                                r1p2[k] = (int)((long)(sp.s1p2 - Bm2 + p2) * invA2 % p2);
                                r2p2[k] = (int)((long)(sp.s2p2 - Bm2 + p2) * invA2 % p2);
                            }
                        }
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

                return new PolySievePlan(
                    poly,
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
                    stepLmodP2,
                    aExps,
                    hasAExps
                );
            }

            public void SieveBlock(
    MpqsContext ctx,
    in PolySievePlan plan,
    long blockIndex,
    IMpqsRelationManager relMgr)
            {
                int L = plan.BlockLen;

                if (_acc.Length < L)
                    _acc = _pool.Rent(L);

                var acc = _acc;
                Array.Clear(acc, 0, L);

                // --- локальные ссылки на поля плана ---
                var P = plan.P;
                var logpScaled = plan.LogPScaled;
                var hasP2 = plan.HasP2;
                var P2 = plan.P2;
                var baseLeftModP = plan.BaseLeftModP;
                var baseLeftModP2 = plan.BaseLeftModP2;
                var stepLmodP = plan.StepLmodP;
                var stepLmodP2 = plan.StepLmodP2;

                int m = P.Length;
                long bi = blockIndex;       // чтобы не кастить в каждой итерации

                // ---------- ЛОГ-РЕШЕТО ----------
                for (int k = 0; k < m; k++)
                {
                    int p = P[k];
                    int logp = logpScaled[k];

                    int off = baseLeftModP[k] + (int)((bi * stepLmodP[k]) % p);
                    if (off >= p) off -= p;

                    int j = plan.R1[k] - off;
                    if (j < 0) j += p;
                    for (; j < L; j += p)
                        acc[j] += logp;

                    int r2 = plan.R2[k];
                    if (r2 != plan.R1[k])
                    {
                        j = r2 - off;
                        if (j < 0) j += p;
                        for (; j < L; j += p)
                            acc[j] += logp;
                    }

                    if (hasP2[k])
                    {
                        int p2 = P2[k];
                        if (p2 == 0) continue;

                        int off2 = baseLeftModP2[k] + (int)((bi * stepLmodP2[k]) % p2);
                        if (off2 >= p2) off2 -= p2;

                        int j2 = plan.R1P2[k] - off2;
                        if (j2 < 0) j2 += p2;
                        for (; j2 < L; j2 += p2)
                            acc[j2] += logp;

                        int r2p2 = plan.R2P2[k];
                        if (r2p2 != plan.R1P2[k])
                        {
                            j2 = r2p2 - off2;
                            if (j2 < 0) j2 += p2;
                            for (; j2 < L; j2 += p2)
                                acc[j2] += logp;
                        }
                    }
                }

                // ---------- ПРОВЕРКА КАНДИДАТОВ ----------
                var FB = ctx.FB;
                int fbLen = FB.Length;

                var smoothOpts = new SmoothnessOptions(
                    MaxLargePrimes: (int)ctx.Options.EnableLargePrimes,
                    LargePrimeBound: fbLen > 0
                        ? (BigInteger)(FB[^1] * ctx.Options.LargePrimeBoundMultiplier)
                        : BigInteger.Zero
                );

                int logScale = plan.LogScale;
                int logSlack = plan.LogSlack;
                BigInteger N = ctx.N;

                BigInteger A = plan.Poly.A;
                BigInteger B = plan.Poly.B;

                BigInteger left = plan.BaseLeft0 + (BigInteger)L * blockIndex;

                // X = A*x + B, x = left + j
                BigInteger X = A * left + B;
                BigInteger T = X * X - N;     // T0 = X0^2 - N

                BigInteger twoA = A << 1;
                BigInteger A2 = A * A;
                BigInteger incT = twoA * X + A2;  // при переходе X→X+A
                BigInteger dIncT = A2 << 1;        // инкремент инкремента
                BigInteger stepX = A;

                // под X (mod N) ведём отдельную рекурренту — дешевле, чем % каждый раз
                BigInteger stepXModN = stepX % N;
                if (stepXModN.Sign < 0) stepXModN += N;

                BigInteger XmodN = X % N;
                if (XmodN.Sign < 0) XmodN += N;

                for (int j = 0; j < L; j++)
                {
                    if (!T.IsZero)
                    {
                        int lower = ScaledLowerBoundFromBitLen(T, logScale);

                        if (acc[j] + logSlack >= lower)
                        {
                            bool neg = T.Sign < 0;
                            BigInteger Tabs = neg ? -T : T;

                            var res = _smooth.Check(Tabs, FB, in smoothOpts);
                            if (res.Accepted)
                            {
                                var exps = res.Exponents!;

                                // последний индекс вектора – бит знака
                                if (neg)
                                    exps[fbLen] = 1;

                                relMgr.SubmitCandidate(ctx, XmodN, exps, res.Remainder);
                            }
                        }
                    }

                    // обновляем T, incT, X, XmodN
                    T += incT;
                    incT += dIncT;
                    X += stepX;

                    XmodN += stepXModN;
                    if (XmodN >= N) XmodN -= N;
                }
            }
        }
    }
}
