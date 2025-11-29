using MathLibrary.Factorization.BlockSieves;
using MathLibrary.Factorization.PolynomialSource;
using MathLibrary.Factorization.RelationManager;
using MathLibrary.Factorization.SmoothCheckers;
using MathLibrary.Factorization.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Factorization.RelationSchedulers
{
    public sealed class TestSiqsRollingScheduler : ISiqsRelationScheduler
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

            polySrc.Reset(ctx);

            var sp = ctx.SP;
            int L = ctx.Options.BlockLen;

            while (!token.IsCancellationRequested && relMgr.FullCount < needRelations)
            {
                // TryNext с flippedIndex/flipSign – пока просто игнорируем их
                int flippedIndex, flipSign;
                if (!polySrc.TryNext(ctx, out ctx.Polynomial, out flippedIndex, out flipSign))
                    break;

                var poly = ctx.Polynomial;

                int blocksPerPoly = Math.Max(1, polySrc.BlocksPerPolynomial);
                if (blocksPerPoly <= 0)
                    continue;

                // Диапазон X: от -blocksPerPoly*L/2 до +blocksPerPoly*L/2
                BigInteger polyBaseLeft0 = -((BigInteger)blocksPerPoly * L) / 2;

                int active = Math.Min(dopRequested, blocksPerPoly);
                if (active <= 0)
                    continue;

                var workers = new ISiqsBlockSieveWorker[active];
                for (int i = 0; i < active; i++)
                    workers[i] = sieveFactory.CreateWorker();

                // Строим план для текущего полинома (из bTerms полиномного источника)
                var plan = workers[0].BuildInitialPlan(
                    ctx,
                    sp,
                    poly,
                    polyBaseLeft0,
                    polySrc.CurrentBTerms
                );

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
                    // динамическое распределение блоков по чанкам
                    long defaultChunk = Math.Min(
                        64L,
                        Math.Max(8L, 32768L / Math.Max(1L, (long)L))
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
