using MathLibrary.Factorization.BlockSieves;
using MathLibrary.Factorization.PolynomialSource;
using MathLibrary.Factorization.RelationManager;
using MathLibrary.Factorization.SmoothCheckers;
using MathLibrary.Factorization.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Factorization.RelationSchedulers
{
    public sealed class SiqsRollingScheduler : ISiqsRelationScheduler
    {
        private static int[] BuildBlockOrder(int blocksPerPoly)
        {
            var order = new int[blocksPerPoly];
            int center = blocksPerPoly / 2;

            int idx = 0;
            order[idx++] = center;

            for (int offset = 1; idx < blocksPerPoly; offset++)
            {
                int right = center + offset;
                if (right < blocksPerPoly)
                {
                    order[idx++] = right;
                    if (idx >= blocksPerPoly) break;
                }

                int left = center - offset;
                if (left >= 0)
                {
                    order[idx++] = left;
                }
            }

            return order;
        }

        public void Collect(
            MpqsContext ctx,
            ISiqsBlockSieveFactory sieveFactory,
            ISmoothnessChecker smoothChecker,
            IMpqsRelationManager relMgr,
            ISiqsPolynomialSource polySrc,
            int needRelations,
            CancellationToken token)
        {
            if (relMgr.FullCount >= needRelations || token.IsCancellationRequested)
                return;

            sieveFactory.SetSmoothChecker(smoothChecker);

            int dopRequested = ctx.Options.DegreeOfParallelism ?? Environment.ProcessorCount;
            if (dopRequested <= 0)
                dopRequested = 1;

            // Для SIQS имеет смысл один раз проинициализировать источник полиномов с контекстом
            polySrc.Reset(ctx);

            var sp = ctx.SP;
            int L = ctx.Options.BlockLen;

            // План решета для текущей семьи (фиксированное A, набор B_v)
            SiqsSievePlan? plan = null;

            while (!token.IsCancellationRequested && relMgr.FullCount < needRelations)
            {
                // Берём следующий полином из SIQS-источника
                if (!polySrc.TryNext(ctx, out ctx.Polynomial, out int flippedIndex, out int flipSign))
                    break;

                var poly = ctx.Polynomial;

                int blocksPerPoly = Math.Max(1, polySrc.BlocksPerPolynomial);
                if (blocksPerPoly <= 0)
                    continue;

                // x-диапазон для данной A,B: blocksPerPoly блоков длиной L, центр около 0
                BigInteger polyBaseLeft0 = -((BigInteger)blocksPerPoly * L) / 2;

                int active = Math.Min(dopRequested, blocksPerPoly);
                if (active <= 0)
                    continue;

                var workers = new ISiqsBlockSieveWorker[active];
                for (int i = 0; i < active; i++)
                    workers[i] = sieveFactory.CreateWorker();

                // Если план ещё не построен, или пришла новая семья (flippedIndex < 0),
                // или поменялось A, или поменялся BlockLen → строим новый план.
                if (plan is null
                    || flippedIndex < 0
                    || plan.Poly.A != poly.A
                    || plan.BlockLen != L)
                {
                    plan = workers[0].BuildInitialPlan(
                        ctx,
                        sp,
                        poly,
                        polyBaseLeft0,
                        polySrc.CurrentBTerms
                    );
                }
                else
                {
                    // Та же A, тот же L: обновляем только корни по delta B_v
                    workers[0].UpdateRoots(plan, flippedIndex, flipSign);
                    plan.Poly = poly;
                }

                int[] blockOrder = BuildBlockOrder(blocksPerPoly);

                // --- Однопоточная ветка ---
                if (active == 1)
                {
                    var w = workers[0];

                    for (int local = 0;
                         local < blocksPerPoly && !token.IsCancellationRequested;
                         local++)
                    {
                        int blockIndex = blockOrder[local];
                        w.SieveBlock(ctx, plan, blockIndex, relMgr);

                        if (relMgr.FullCount >= needRelations)
                            return;
                    }

                    continue;
                }

                // --- Многопоточная ветка ---
                ExceptionDispatchInfo? captured = null;
                using var stop = CancellationTokenSource.CreateLinkedTokenSource(token);

                bool useStaticPartition = blocksPerPoly <= active * 8;

                if (useStaticPartition)
                {
                    // Статическое разбиение: каждый поток берёт индексы local = t, t+active, ...
                    _ = Parallel.For(0, active, t =>
                    {
                        var w = workers[t];
                        try
                        {
                            for (int local = t;
                                 local < blocksPerPoly && !stop.IsCancellationRequested;
                                 local += active)
                            {
                                int blockIndex = blockOrder[local];
                                w.SieveBlock(ctx, plan, blockIndex, relMgr);

                                if (relMgr.FullCount >= needRelations)
                                {
                                    stop.Cancel();
                                    break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (Interlocked.CompareExchange(
                                    ref captured,
                                    ExceptionDispatchInfo.Capture(ex),
                                    null) == null)
                            {
                                stop.Cancel();
                            }
                        }
                    });
                }
                else
                {
                    // Динамическое разбиение чанками по local-индексам
                    long defaultChunk = Math.Min(
                        64L,
                        Math.Max(8L, 32768L / Math.Max(1L, L))
                    );
                    long fair = (blocksPerPoly + active - 1L) / active;
                    int chunkSize = (int)Math.Max(1L, Math.Min(defaultChunk, fair));

                    int nextIdx = 0;

                    _ = Parallel.For(0, active, t =>
                    {
                        var w = workers[t];
                        try
                        {
                            while (!stop.IsCancellationRequested)
                            {
                                int start = Interlocked.Add(ref nextIdx, chunkSize) - chunkSize;
                                if (start >= blocksPerPoly)
                                    break;

                                int end = Math.Min(start + chunkSize, blocksPerPoly);

                                for (int local = start;
                                     local < end && !stop.IsCancellationRequested;
                                     local++)
                                {
                                    int blockIndex = blockOrder[local];
                                    w.SieveBlock(ctx, plan, blockIndex, relMgr);

                                    if (relMgr.FullCount >= needRelations)
                                    {
                                        stop.Cancel();
                                        break;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (Interlocked.CompareExchange(
                                    ref captured,
                                    ExceptionDispatchInfo.Capture(ex),
                                    null) == null)
                            {
                                stop.Cancel();
                            }
                        }
                    });
                }

                if (captured != null)
                    captured.Throw();

                if (relMgr.FullCount >= needRelations)
                    break;
            }
        }
    }
}
