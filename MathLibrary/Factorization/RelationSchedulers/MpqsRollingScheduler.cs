using MathLibrary.Factorization.BlockSieves;
using MathLibrary.Factorization.PolynomialSource;
using MathLibrary.Factorization.RelationManager;
using MathLibrary.Factorization.SmoothCheckers;
using MathLibrary.Factorization.Types;
using System.Numerics;
using System.Runtime.ExceptionServices;

namespace MathLibrary.Factorization.RelationSchedulers
{
    public sealed class MpqsRollingScheduler : IMpqsRelationScheduler
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
            IMpqsBlockSieveFactory sieveFactory,
            ISmoothnessChecker smoothChecker,
            IMpqsRelationManager relMgr,
            IPolynomialSource polySrc,
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
                if (!polySrc.TryNext(ctx, out ctx.Polynomial))
                    break;

                var poly = ctx.Polynomial;

                int blocksPerPoly = Math.Max(1, polySrc.BlocksPerPolynomial);
                if (blocksPerPoly <= 0)
                    continue;

                BigInteger polyBaseLeft0 = -((BigInteger)blocksPerPoly * L) / 2;

                int active = Math.Min(dopRequested, blocksPerPoly);
                if (active <= 0)
                    continue;

                var workers = new IMpqsBlockSieveWorker[active];
                for (int i = 0; i < active; i++)
                    workers[i] = sieveFactory.CreateWorker();

                var plan = workers[0].BuildPlan(ctx, sp, poly, polyBaseLeft0);

                int[] blockOrder = BuildBlockOrder(blocksPerPoly);

                if (active == 1)
                {
                    var w = workers[0];

                    for (int local = 0; local < blocksPerPoly && !token.IsCancellationRequested; local++)
                    {
                        int blockIndex = blockOrder[local];
                        w.SieveBlock(ctx, in plan, blockIndex, relMgr);

                        if (relMgr.FullCount >= needRelations)
                            return;
                    }

                    continue;
                }

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
                                w.SieveBlock(ctx, in plan, blockIndex, relMgr);

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
                    long defaultChunk = Math.Min(64L, Math.Max(8L, 32768L / Math.Max(1L, L)));
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
                                    w.SieveBlock(ctx, in plan, blockIndex, relMgr);

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
