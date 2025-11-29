using MathLibrary.Extensions;
using MathLibrary.Factorization.BlockSieves;
using MathLibrary.Factorization.DependencySolver;
using MathLibrary.Factorization.PolynomialSource;
using MathLibrary.Factorization.RelationManager;
using MathLibrary.Factorization.RelationSchedulers;
using MathLibrary.Factorization.SmoothCheckers;
using MathLibrary.Factorization.Types;
using MathLibrary.Functions;
using MathLibrary.LinearAlgebraZ2;
using System.Buffers;
using System.Numerics;

namespace MathLibrary.Factorization
{
    public static partial class Factorization
    {
        public static BigInteger[] SIQSMethod(BigInteger n, QSLPOpt lpOpt)
        {
            // Пока можно использовать MpqsOptions.Build(n) — если потом заведёшь SiqsOptions,
            // просто поменяешь тут тип/фабрику.
            var options = MpqsOptions.Build(n) with
            {
                EnableLargePrimes = lpOpt
            };

            ISmoothnessChecker checker = new EarlyAbortSmoothnessChecker();

            // SIQS по сути та же модель отношений, что и MPQS, так что менеджеры можно переиспользовать
            IMpqsRelationManager manager = lpOpt switch
            {
                QSLPOpt.NoLP => new MpqsNoLpRelationManager(),
                QSLPOpt.OneLP => new MpqsOneLpRelationManager(),
                _ => new MpqsNoLpRelationManager()
            };

            return SIQSMethod(
                n,
                options,
                checker,
                sieveFactory: new SiqsBlockSieveFactory(),
                scheduler: new SiqsRollingScheduler(),
                relationManager: manager,
                polySrc: null, // создадим по умолчанию ниже
                dependencySolver: new GaussianZ2SolverAdapter()
            );
        }

        public static BigInteger[] SIQSMethod(
    BigInteger n,
    MpqsOptions options,
    ISmoothnessChecker smoothChecker,
    ISiqsBlockSieveFactory sieveFactory,
    ISiqsRelationScheduler scheduler,
    IMpqsRelationManager relationManager,
    ISiqsPolynomialSource? polySrc,
    IDependencySolver dependencySolver)
        {
            // База
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

            // Фактор-база
            var FB = Sieves.AtkinSieve(
                options.B,
                p => ArithmeticFunctions.LegendreSymbol(n, p) != -1
            );

            // Вытаскиваем все маленькие делители из n
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

                var tail = SIQSMethod(
                    n,
                    options,
                    smoothChecker,
                    sieveFactory,
                    scheduler,
                    relationManager,
                    polySrc,
                    dependencySolver
                );
                small.AddRange(tail);
                return [.. small];
            }

            // Привязываем фактор-базу к checker'у, если он её использует
            if (smoothChecker is IUsesFactorBase binder)
                binder.Bind(FB);

            // Контекст SIQS (можем переиспользовать MpqsContext)
            var ctx = new MpqsContext(n, options)
            {
                FB = FB
            };

            ctx.SP = PrepareSievePrimes(ctx, ctx.FB);

            int baseNeed = ctx.FB.Length + options.Safety;

            var XmodN = new List<BigInteger>(baseNeed << 1);
            var Exps = new List<ushort[]>(baseNeed << 1);
            // X нам для SIQS не нужен — всё уже в XmodN / экспонентах

            relationManager.InitializeTargets(
                xmodN: XmodN,
                Exps: Exps
            );

            if (polySrc is null)
            {
                polySrc = new SiqsPolynomialSource(
                    blocksPerPoly: options.BlocksPerPolynomial,
                    aMaxBits: options.AMaxBits,
                    fbWindow: 256
                );
            }

            polySrc.Reset(ctx);
            sieveFactory.SetSmoothChecker(smoothChecker);

            void CollectRelations(int target)
            {
                scheduler.Collect(
                    ctx,
                    sieveFactory,
                    smoothChecker,
                    relationManager,
                    polySrc,
                    target,
                    CancellationToken.None
                );
            }

            // Первая порция отношений
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

                // Линейная алгебра над F2
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

                    // a = произведение XmodN[j] по выбранным j
                    BigInteger a = BigInteger.One;
                    for (int j = 0; j < r; j++)
                        if (select[j])
                            a = a * XmodN[j] % ctx.N;

                    // E[i] – суммарная степень p_i (до деления пополам)
                    int[] E = ArrayPool<int>.Shared.Rent(m);
                    Array.Clear(E, 0, m);

                    for (int j = 0; j < r; j++)
                    {
                        if (!select[j]) continue;
                        var ex = Exps[j];
                        for (int i = 0; i < m; i++)
                            E[i] += ex[i];
                    }

                    // b = произведение p_i^{E[i]/2} mod N
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

                    // Пробуем gcd(|a - b|, N)
                    BigInteger diff = a >= b ? a - b : b - a;
                    var g1 = BigInteger.GreatestCommonDivisor(diff, ctx.N);
                    if (g1 > 1 && g1 < ctx.N)
                    {
                        var result = new List<BigInteger>();
                        result.AddRange(SQUFOFMethod(g1));
                        result.AddRange(SQUFOFMethod(n / g1));
                        return [.. result];
                    }

                    // И gcd(a + b, N) на всякий
                    BigInteger sum = (a + b) % ctx.N;
                    var g2 = BigInteger.GreatestCommonDivisor(sum, ctx.N);
                    if (g2 > 1 && g2 < ctx.N)
                    {
                        var result = new List<BigInteger>();
                        result.AddRange(SQUFOFMethod(g2));
                        result.AddRange(SQUFOFMethod(n / g2));
                        return [.. result];
                    }
                }

                int nextTarget = Math.Min(maxTarget, Exps.Count + safetyStep);
                if (relationManager.FullCount < nextTarget)
                    CollectRelations(nextTarget);
            }

            // Не удалось найти нетривиальный делитель
            return [ctx.N];
        }
    }
}
