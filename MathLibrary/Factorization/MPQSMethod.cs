using MathLibrary.Extensions;
using MathLibrary.Factorization.BlockSieves;
using MathLibrary.Factorization.DependencySolver;
using MathLibrary.Factorization.PolynomialSource;
using MathLibrary.Factorization.RelationManager;
using MathLibrary.Factorization.RelationSchedulers;
using MathLibrary.Factorization.SmoothCheckers;
using MathLibrary.Factorization.Types;
using MathLibrary.Functions;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Factorization
{
    public static partial class Factorization
    {
        public static BigInteger[] MPQSMethod(BigInteger n, QSLPOpt lpOpt)
        {
            var options = MpqsOptions.Build(n) with
            {
                EnableLargePrimes = lpOpt,
            };
            ISmoothnessChecker checker = new EarlyAbortSmoothnessChecker();

            IMpqsRelationManager manager = lpOpt switch
            {
                QSLPOpt.NoLP => new MpqsNoLpRelationManager(),
                QSLPOpt.OneLP => new MpqsOneLpRelationManager(),
                _ => new MpqsNoLpRelationManager()
            };

            return MPQSMethod(
                n,
                options,
                checker,
                sieveFactory: new MpqsBlockSieveFactory(),
                scheduler: new MpqsRollingScheduler(),
                relationManager: manager,
                polySrc: null,
                dependencySolver: new GaussianZ2SolverAdapter()
            );
        }

        public static BigInteger[] MPQSMethod(
    BigInteger n,
    MpqsOptions options,
    ISmoothnessChecker smoothChecker,
    IMpqsBlockSieveFactory sieveFactory,
    IMpqsRelationScheduler scheduler,
    IMpqsRelationManager relationManager,
    IPolynomialSource? polySrc,
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

            var FB = Sieves.AtkinSieve(options.B, p => ArithmeticFunctions.LegendreSymbol(n, p) != -1);

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

                if (PrimalityTests.MillerTest(n))
                {
                    small.Add(n);
                    return [.. small];
                }

                var tail = MPQSMethod(
                    n,
                    options,
                    smoothChecker,
                    sieveFactory,
                    scheduler,
                    relationManager,
                    polySrc,
                    dependencySolver);

                small.AddRange(tail);
                return [.. small];
            }

            var swTotal = Stopwatch.StartNew();
            var swSieving = new Stopwatch();
            var swLinear = new Stopwatch();

            int sievingCalls = 0;

            if (smoothChecker is IUsesFactorBase binder)
                binder.Bind(FB);

            var ctx = new MpqsContext(n, options)
            {
                FB = FB
            };

            ctx.SP = PrepareSievePrimes(ctx, ctx.FB);

            int fbLen = ctx.FB.Length;
            int baseNeed = fbLen + options.Safety;

            var XmodN = new List<BigInteger>(baseNeed << 1);
            var Exps = new List<ushort[]>(baseNeed << 1);

            relationManager.InitializeTargets(XmodN, Exps);

            if (polySrc is null)
            {
                polySrc = new LogTargetSwapMpqsPolynomialSource(
                    options.BlocksPerPolynomial,
                    options.AMaxBits);
            }

            polySrc.Reset(ctx);
            sieveFactory.SetSmoothChecker(smoothChecker);

            void CollectRelations(int target)
            {
                sievingCalls++;
                scheduler.Collect(
                    ctx,
                    sieveFactory,
                    smoothChecker,
                    relationManager,
                    polySrc,
                    target,
                    CancellationToken.None);
                swSieving.Stop();
            }

            swSieving.Start();

            CollectRelations(baseNeed);

            int maxRounds = 10;
            int safetyStep = Math.Max(8, options.Safety);
            int maxTarget = baseNeed + 8 * safetyStep;

            for (int round = 0; round < maxRounds; round++)
            {
                int r = Exps.Count;

                int rowCount = r > 0 ? Exps[0].Length : (fbLen + 1);

                if (r < rowCount + 1)
                {
                    int target = Math.Min(maxTarget, r + safetyStep);
                    if (relationManager.FullCount < target)
                        CollectRelations(target);

                    r = Exps.Count;
                    rowCount = r > 0 ? Exps[0].Length : (fbLen + 1);

                    if (r < rowCount + 1 && round == maxRounds - 1)
                        break;
                }

                swLinear.Start();

                dependencySolver.FromParityColumns(Exps);
                var (pivotFlags, pivotOfRow) = dependencySolver.Solve();

                var freeCols = new List<int>(r);
                for (int c = 0; c < r; c++)
                    if (!pivotFlags[c]) freeCols.Add(c);

                var select = new bool[r];

                foreach (int freeCol in freeCols)
                {
                    Array.Clear(select, 0, select.Length);
                    dependencySolver.BuildDependencyVector(pivotOfRow, freeCol, select);

                    BigInteger a = BigInteger.One;
                    for (int j = 0; j < r; j++)
                        if (select[j])
                            a = a * XmodN[j] % ctx.N;

                    int[] E = ArrayPool<int>.Shared.Rent(rowCount);
                    Array.Clear(E, 0, rowCount);

                    for (int j = 0; j < r; j++)
                    {
                        if (!select[j]) continue;
                        var ex = Exps[j];
                        for (int i = 0; i < rowCount; i++)
                            E[i] += ex[i];
                    }

                    int signIndex = rowCount - 1;

                    BigInteger b = BigInteger.One;
                    for (int i = 0; i < fbLen; i++)
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
                        result.AddRange(SQUFOFMethod(g1));
                        result.AddRange(SQUFOFMethod(n / g1));

                        swLinear.Stop();
                        swTotal.Stop();

                        Console.WriteLine("=== MPQS profile ===");
                        Console.WriteLine($"N digits         = {n.DecimalDigits()}");
                        Console.WriteLine($"FB size          = {ctx.FB.Length}");
                        Console.WriteLine($"B (bound)        = {options.B}");
                        Console.WriteLine($"BlockLen         = {options.BlockLen}");
                        Console.WriteLine($"BlocksPerPoly    = {options.BlocksPerPolynomial}");
                        Console.WriteLine($"A MaxBits        = {options.AMaxBits}");
                        Console.WriteLine($"Total time       = {swTotal.Elapsed}");
                        Console.WriteLine($"Sieving time     = {swSieving.Elapsed} (calls = {sievingCalls})");
                        Console.WriteLine($"Linear algebra   = {swLinear.Elapsed}");
                        Console.WriteLine($"Relations total  = {Exps.Count}");
                        Console.WriteLine($"Need relations   = {ctx.FB.Length + options.Safety}");
                        Console.WriteLine();

                        return [.. result];
                    }

                    BigInteger sum = (a + b) % ctx.N;
                    var g2 = BigInteger.GreatestCommonDivisor(sum, ctx.N);
                    if (g2 > 1 && g2 < ctx.N)
                    {
                        var result = new List<BigInteger>();
                        result.AddRange(SQUFOFMethod(g2));
                        result.AddRange(SQUFOFMethod(n / g2));

                        swLinear.Stop();
                        swTotal.Stop();

                        Console.WriteLine("=== MPQS profile ===");
                        Console.WriteLine($"N digits         = {n.DecimalDigits()}");
                        Console.WriteLine($"FB size          = {ctx.FB.Length}");
                        Console.WriteLine($"B (bound)        = {options.B}");
                        Console.WriteLine($"BlockLen         = {options.BlockLen}");
                        Console.WriteLine($"BlocksPerPoly    = {options.BlocksPerPolynomial}");
                        Console.WriteLine($"A MaxBits        = {options.AMaxBits}");
                        Console.WriteLine($"Total time       = {swTotal.Elapsed}");
                        Console.WriteLine($"Sieving time     = {swSieving.Elapsed} (calls = {sievingCalls})");
                        Console.WriteLine($"Linear algebra   = {swLinear.Elapsed}");
                        Console.WriteLine($"Relations total  = {Exps.Count}");
                        Console.WriteLine($"Need relations   = {ctx.FB.Length + options.Safety}");
                        Console.WriteLine();

                        return [.. result];
                    }
                }

                swLinear.Stop();

                int nextTarget = Math.Min(maxTarget, Exps.Count + safetyStep);
                if (relationManager.FullCount < nextTarget)
                    CollectRelations(nextTarget);
            }

            swTotal.Stop();

            Console.WriteLine("=== MPQS profile ===");
            Console.WriteLine($"N digits         = {n.DecimalDigits()}");
            Console.WriteLine($"FB size          = {ctx.FB.Length}");
            Console.WriteLine($"B (bound)        = {options.B}");
            Console.WriteLine($"BlockLen         = {options.BlockLen}");
            Console.WriteLine($"BlocksPerPoly    = {options.BlocksPerPolynomial}");
            Console.WriteLine($"A MaxBits        = {options.AMaxBits}");
            Console.WriteLine($"Total time       = {swTotal.Elapsed}");
            Console.WriteLine($"Sieving time     = {swSieving.Elapsed} (calls = {sievingCalls})");
            Console.WriteLine($"Linear algebra   = {swLinear.Elapsed}");
            Console.WriteLine($"Relations total  = {Exps.Count}");
            Console.WriteLine($"Need relations   = {ctx.FB.Length + options.Safety}");
            Console.WriteLine();


            return [ctx.N];
        }

        private static SievePrime[] PrepareSievePrimes(MpqsContext ctx, int[] FB)
        {
            if (FB.Length < 1) throw new ArgumentException(null, nameof(FB));

            var SP = new SievePrime[FB.Length];
            int blockLen = ctx.Options.BlockLen;
            int LOG_SCALE = ctx.Options.LogScale;

            int p = FB[0];
            int logp = (int)Math.Round(Math.Log(p) * LOG_SCALE);

            int s1 = 1, s2 = 1;
            int nMod4 = (int)(ctx.N & 3);
            bool hasP2 = (nMod4 == 1) && blockLen >= 4;
            int p2 = hasP2 ? 4 : 0;
            int s1p2 = hasP2 ? 1 : 0;
            int s2p2 = hasP2 ? 3 : 0;

            SP[0] = new SievePrime(p, (ushort)logp, hasP2, p2, s1, s2, s1p2, s2p2);

            for (int i = 1; i < FB.Length; i++)
            {
                p = FB[i];

                (s1, s2) = ArithmeticFunctions.TonelliShanks(ctx.N, p);
                logp = (int)Math.Round(Math.Log(p) * LOG_SCALE);

                hasP2 = false; p2 = 0; s1p2 = 0; s2p2 = 0;

                long lp2 = (long)p * p;
                if (lp2 <= blockLen && lp2 <= int.MaxValue && (s1 % p) != 0)
                {
                    p2 = (int)lp2;
                    s1p2 = ArithmeticFunctions.LiftRootModP2(ctx.N, p, s1);
                    s2p2 = p2 - s1p2; if (s2p2 == p2) s2p2 = 0;
                    hasP2 = true;
                }

                SP[i] = new SievePrime(p, (ushort)logp, hasP2, p2, s1, s2, s1p2, s2p2);
            }

            return SP;
        }

    }
}
