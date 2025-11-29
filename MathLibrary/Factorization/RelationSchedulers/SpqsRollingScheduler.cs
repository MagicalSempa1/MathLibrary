using MathLibrary.Factorization.BlockSieves;
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
    public sealed class SpqsRollingScheduler : ISpqsRelationScheduler
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long ZigZag(long i) =>
            (i & 1) == 0 ? (i >> 1) : -((i >> 1) + 1);

        public void Collect(
            SpqsContext ctx,
            ISpqsBlockSieveFactory sieveFactory,
            ISmoothnessChecker smoothChecker,
            ISpqsRelationManager relMgr,
            int needRelations,
            BigInteger baseLeft0,
            CancellationToken token)
        {
            if (relMgr.FullCount >= needRelations || token.IsCancellationRequested)
                return;

            sieveFactory.SetSmoothChecker(smoothChecker);

            int dopRequested = ctx.Options.DegreeOfParallelism ?? Environment.ProcessorCount;
            if (dopRequested <= 0)
                dopRequested = 1;

            int active = dopRequested;
            if (active <= 0)
                return;

            var sp = ctx.SP;

            if (active == 1)
            {
                var worker = sieveFactory.CreateWorker();
                var plan = worker.BuildPlan(ctx, sp, baseLeft0);

                while (!token.IsCancellationRequested && relMgr.FullCount < needRelations)
                {
                    long local = ctx.NextBlockLocal++;
                    long s = ZigZag(local);

                    worker.SieveBlock(ctx, plan, s, relMgr);

                    if (relMgr.FullCount >= needRelations)
                        break;
                }

                return;
            }

            var workers = new ISpqsBlockSieveWorker[active];
            for (int i = 0; i < active; i++)
                workers[i] = sieveFactory.CreateWorker();

            var planParallel = workers[0].BuildPlan(ctx, sp, baseLeft0);

            ExceptionDispatchInfo? captured = null;
            using var stop = CancellationTokenSource.CreateLinkedTokenSource(token);

            int L = planParallel.BlockLen;
            long defaultChunk = Math.Min(64L, Math.Max(8L, 32768L / Math.Max(1L, L)));
            long chunkSize = defaultChunk;

            _ = Parallel.For(0, active, t =>
            {
                var w = workers[t];

                try
                {
                    while (!stop.IsCancellationRequested)
                    {
                        long start = Interlocked.Add(ref ctx.NextBlockLocal, chunkSize) - chunkSize;
                        long end = start + chunkSize;

                        for (long local = start;
                             local < end && !stop.IsCancellationRequested;
                             local++)
                        {
                            long s = ZigZag(local);
                            w.SieveBlock(ctx, planParallel, s, relMgr);

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

            if (captured != null)
                captured.Throw();
        }
    }
}
