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
    public sealed class PolyParallelRollingScheduler : IMpqsRelationScheduler
    {
        /// <summary>
        /// Строит порядок блоков: центр, вправо, влево, вправо, влево...
        /// </summary>
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
            // Быстрый выход, если уже всё есть
            if (relMgr.FullCount >= needRelations || token.IsCancellationRequested)
                return;

            sieveFactory.SetSmoothChecker(smoothChecker);

            int dopRequested = ctx.Options.DegreeOfParallelism ?? Environment.ProcessorCount;
            if (dopRequested <= 0)
                dopRequested = 1;

            // Подготовка генератора полиномов (один на всех потоков)
            polySrc.Reset(ctx);
            var sp = ctx.SP;
            int L = ctx.Options.BlockLen;

            int blocksPerPoly = Math.Max(1, polySrc.BlocksPerPolynomial);
            var blockOrder = BuildBlockOrder(blocksPerPoly);

            // Общий синхронизатор для выдачи полиномов
            object polyGate = new();
            bool polyExhausted = false;

            bool TryGetNextPolynomial(out QSPolynomial poly, out BigInteger baseLeft0)
            {
                lock (polyGate)
                {
                    if (polyExhausted)
                    {
                        poly = default;
                        baseLeft0 = default;
                        return false;
                    }

                    if (!polySrc.TryNext(ctx, out poly))
                    {
                        polyExhausted = true;
                        baseLeft0 = default;
                        return false;
                    }

                    // Центруем полином вокруг нуля: [-BPP*L/2, +BPP*L/2)
                    baseLeft0 = -((BigInteger)blocksPerPoly * L) / 2;
                    return true;
                }
            }

            ExceptionDispatchInfo? captured = null;
            using var stop = CancellationTokenSource.CreateLinkedTokenSource(token);

            // Параллелим по потокам, каждый поток обрабатывает свои полиномы
            _ = Parallel.For(0, dopRequested, t =>
            {
                var w = sieveFactory.CreateWorker();

                try
                {
                    while (!stop.IsCancellationRequested &&
                           relMgr.FullCount < needRelations)
                    {
                        if (!TryGetNextPolynomial(out var poly, out var baseLeft0))
                            break; // полиномы закончились

                        // План для конкретного полинома
                        var plan = w.BuildPlan(ctx, sp, poly, baseLeft0);

                        // Просеиваем ВСЕ блоки этого полинома в "приятном" порядке
                        for (int local = 0;
                             local < blocksPerPoly && !stop.IsCancellationRequested;
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

            if (captured != null)
                captured.Throw();
        }
    }
}
