using MathLibrary.Extensions;
using MathLibrary.Factorization.BlockSieves;
using MathLibrary.Factorization.DependencySolver;
using MathLibrary.Factorization.RelationManager;
using MathLibrary.Factorization.RelationSchedulers;
using MathLibrary.Factorization.SmoothCheckers;
using MathLibrary.Factorization.Types;
using MathLibrary.Functions;
using System.Buffers;
using System.Diagnostics;
using System.Numerics;

namespace MathLibrary.Factorization
{
    public static partial class Factorization
    {
        public static BigInteger[] SPQSMethod(BigInteger n)
        {
            var options = SpqsOptions.Build(n);

            return SPQSMethod(
                n,
                options,
                smoothChecker: new EarlyAbortSmoothnessChecker(),
                sieveFactory: new SpqsBlockSieveFactory(),
                scheduler: new SpqsRollingScheduler(),
                relationManager: new SpqsRelationManager(),
                dependencySolver: new GaussianZ2SolverAdapter()
            );
        }

        public static BigInteger[] SPQSMethod(
            BigInteger n,
            SpqsOptions options,
            ISmoothnessChecker smoothChecker,
            ISpqsBlockSieveFactory sieveFactory,
            ISpqsRelationScheduler scheduler,
            ISpqsRelationManager relationManager,
            IDependencySolver dependencySolver)
        {
            if (n <= 1) return [];
            if (PrimalityTests.MillerTest(n)) return [n];

            BigInteger root = BigInteger.Zero;
            if (n.IsSqrt(ref root))
            {
                if (PrimalityTests.MillerTest(root)) return [root, root];

                var factors = SQUFOFMethod(root);
                var dup = new BigInteger[factors.Length << 1];
                factors.CopyTo(dup, 0);
                factors.CopyTo(dup, factors.Length);
                return dup;
            }

            var FB = Sieves.AtkinSieve(
                options.B,
                x => ArithmeticFunctions.LegendreSymbol(n, x) != -1);

            var small = new List<BigInteger>();
            for (int i = 0; i < FB.Length; i++)
            {
                while (n % FB[i] == 0)
                {
                    small.Add(FB[i]);
                    n /= FB[i];
                }
            }

            if (small.Count > 0)
            {
                if (n == 1) return [.. small];
                if (PrimalityTests.MillerTest(n)) return [.. small, n];

                var tail = SQUFOFMethod(n);
                small.AddRange(tail);
                return [.. small];
            }

            if (smoothChecker is IUsesFactorBase binder)
                binder.Bind(FB);

            var ctx = new SpqsContext(n, options)
            {
                FB = FB
            };
            ctx.SP = PrepareSpqsSievePrimes(ctx, ctx.FB);

            int baseNeed = ctx.FB.Length + options.Safety;

            var XmodN = new List<BigInteger>(baseNeed << 1);
            var Exps = new List<ushort[]>(baseNeed << 1);

            relationManager.InitializeTargets(XmodN, Exps);

            void CollectRelations(int target)
            {
                ctx.XStart = ctx.N.CeilingSqrt();
                BigInteger baseLeft0 = ctx.XStart - (options.BlockLen >> 1);
                using var cts = new CancellationTokenSource();
                scheduler.Collect(
                    ctx,
                    sieveFactory,
                    smoothChecker,
                    relationManager,
                    target,
                    baseLeft0,
                    cts.Token
                );
            }

            CollectRelations(baseNeed);

            int maxRounds = 10;
            int safetyStep = Math.Max(8, options.Safety);
            int maxTarget = baseNeed + 8 * safetyStep;

            for (int round = 0; round < maxRounds; round++)
            {
                int m = ctx.FB.Length;
                int r = Exps.Count;

                if (r < m + 1)
                {
                    int target = Math.Min(maxTarget, r + safetyStep);
                    if (relationManager.FullCount < target)
                        CollectRelations(target);

                    r = Exps.Count;
                    if (r < m + 1 && round == maxRounds - 1)
                        break;
                }

                dependencySolver.FromParityColumns(Exps);
                var (pivotFlags, pivotOfRow) = dependencySolver.Solve();

                var freeCols = new List<int>(r);
                for (int c = 0; c < r; c++)
                    if (!pivotFlags[c])
                        freeCols.Add(c);

                var select = new bool[r];

                foreach (int f in freeCols)
                {
                    Array.Clear(select, 0, select.Length);
                    dependencySolver.BuildDependencyVector(pivotOfRow, f, select);

                    BigInteger a = BigInteger.One;
                    for (int j = 0; j < r; j++)
                        if (select[j])
                            a = a * XmodN[j] % ctx.N;

                    int[] E = ArrayPool<int>.Shared.Rent(m);
                    Array.Clear(E, 0, m);

                    for (int j = 0; j < r; j++)
                    {
                        if (!select[j]) continue;
                        var ex = Exps[j];
                        for (int i = 0; i < m; i++)
                            E[i] += ex[i];
                    }

                    BigInteger b = BigInteger.One;
                    for (int i = 0; i < m; i++)
                    {
                        int half = E[i] >> 1;
                        if (half != 0)
                        {
                            if (half <= 3)
                            {
                                for (int t = 0; t < half; t++)
                                    b = b * ctx.FB[i] % ctx.N;
                            }
                            else
                            {
                                b = b * BigInteger.ModPow(ctx.FB[i], half, ctx.N) % ctx.N;
                            }
                        }
                    }

                    ArrayPool<int>.Shared.Return(E, clearArray: true);

                    BigInteger diff = a >= b ? a - b : b - a;
                    var g1 = BigInteger.GreatestCommonDivisor(diff, ctx.N);
                    if (g1 > 1 && g1 < ctx.N)
                    {
                        var result = new List<BigInteger>();
                        result.AddRange(Factorization.SQUFOFMethod(g1));
                        result.AddRange(Factorization.SQUFOFMethod(n / g1));
                        return [.. result];
                    }

                    BigInteger sum = (a + b) % ctx.N;
                    var g2 = BigInteger.GreatestCommonDivisor(sum, ctx.N);
                    if (g2 > 1 && g2 < ctx.N)
                    {
                        var result = new List<BigInteger>();
                        result.AddRange(Factorization.SQUFOFMethod(g2));
                        result.AddRange(Factorization.SQUFOFMethod(n / g2));
                        return [.. result];
                    }
                }

                int nextTarget = Math.Min(maxTarget, Exps.Count + safetyStep);
                if (relationManager.FullCount < nextTarget)
                    CollectRelations(nextTarget);
            }

            return [ctx.N];
        }

        private static SievePrime[] PrepareSpqsSievePrimes(SpqsContext ctx, int[] FB)
        {
            if (FB.Length < 1) throw new ArgumentException(null, nameof(FB));

            var SP = new SievePrime[FB.Length];
            int blockLen = ctx.Options.BlockLen;
            int LOG_SCALE = ctx.Options.LogScale;

            int p = FB[0];
            ushort logp = (ushort)Math.Round(Math.Log(p) * LOG_SCALE);

            int s1 = 1, s2 = 1;
            int nMod4 = (int)(ctx.N & 3);
            bool hasP2 = (nMod4 == 1) && blockLen >= 4;
            int p2 = hasP2 ? 4 : 0;
            int s1p2 = hasP2 ? 1 : 0;
            int s2p2 = hasP2 ? 3 : 0;

            SP[0] = new SievePrime(p, logp, hasP2, p2, s1, s2, s1p2, s2p2);

            for (int i = 1; i < FB.Length; i++)
            {
                p = FB[i];

                (s1, s2) = Functions.ArithmeticFunctions.TonelliShanks(ctx.N, p);
                logp = (ushort)Math.Round(Math.Log(p) * LOG_SCALE);

                hasP2 = false; p2 = 0; s1p2 = 0; s2p2 = 0;

                long lp2 = (long)p * p;
                if (lp2 <= blockLen && lp2 <= int.MaxValue && (s1 % p) != 0)
                {
                    p2 = (int)lp2;
                    s1p2 = Functions.ArithmeticFunctions.LiftRootModP2(ctx.N, p, s1);
                    s2p2 = p2 - s1p2; if (s2p2 == p2) s2p2 = 0;
                    hasP2 = true;
                }

                SP[i] = new SievePrime(p, logp, hasP2, p2, s1, s2, s1p2, s2p2);
            }

            return SP;
        }
    }
}
