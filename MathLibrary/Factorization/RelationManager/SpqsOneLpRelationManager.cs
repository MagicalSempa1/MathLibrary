using MathLibrary.Factorization.Types;
using MathLibrary.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Factorization.RelationManager
{
    public sealed class SpqsOneLpRelationManager : ISpqsRelationManager
    {
        private sealed class PartialRelation
        {
            public readonly BigInteger XmodN;
            public readonly ushort[] Exps;

            public PartialRelation(BigInteger xmodN, ushort[] exps)
            {
                XmodN = xmodN;
                Exps = exps;
            }
        }

        private readonly object _fullGate = new();

        private List<BigInteger>? _XmodN;
        private List<ushort[]>? _Exps;

        private int _fullCount;
        public int FullCount => Volatile.Read(ref _fullCount);

        private sealed class Shard
        {
            public readonly object Gate = new();
            public readonly Dictionary<int, PartialRelation> Partials = new Dictionary<int, PartialRelation>();
        }

        private const int ShardCount = 16;
        private const int ShardMask = ShardCount - 1;

        private readonly Shard[] _shards;

        public SpqsOneLpRelationManager()
        {
            _shards = new Shard[ShardCount];
            for (int i = 0; i < ShardCount; i++)
                _shards[i] = new Shard();
        }

        public void InitializeTargets(List<BigInteger> XmodN, List<ushort[]> Exps)
        {
            lock (_fullGate)
            {
                _XmodN = XmodN;
                _Exps = Exps;
                _fullCount = Exps.Count;
            }

            // очистить все partial-отношения
            for (int s = 0; s < ShardCount; s++)
            {
                var shard = _shards[s];
                lock (shard.Gate)
                {
                    shard.Partials.Clear();
                }
            }
        }

        public void SubmitCandidate(SpqsContext ctx, BigInteger xmodN, ushort[] exps, BigInteger remainder)
        {
            // Полностью гладкое — просто добавляем
            if (remainder.IsOne)
            {
                AddFullRelation(xmodN, exps);
                return;
            }

            // LP отключены — игнор
            if (ctx.Options.EnableLargePrimes == QSLPOpt.NoLP)
                return;

            // Remainder должен быть положительным простым (или по крайней мере > 0)
            if (remainder.Sign <= 0)
                return;

            // В этой реализации храним lp как int
            if (remainder > int.MaxValue)
                return;

            int lp = (int)remainder;

            Shard shard = _shards[lp & ShardMask];

            PartialRelation? first = null;

            // 1. Локально принимаем решение — есть ли уже partial с этим lp
            lock (shard.Gate)
            {
                if (shard.Partials.TryGetValue(lp, out var existing))
                {
                    shard.Partials.Remove(lp);
                    first = existing;
                }
                else
                {
                    // Первый partial для этого lp — просто запоминаем
                    shard.Partials[lp] = new PartialRelation(xmodN, exps);
                }
            }

            // 2. Если second == null, значит мы только сохранили partial и всё
            if (first is null)
                return;

            // 3. Склейка двух partial-отношений с одним и тем же lp
            //    Q(x1) = lp * ∏ p^e1
            //    Q(x2) = lp * ∏ p^e2
            // => Q(x1) Q(x2) = lp^2 * ∏ p^(e1+e2)
            //    хотим отношение вида:
            //    x'^2 ≡ ∏ p^(e1+e2) (mod N)
            //    где x' = x1 * x2 * lp^{-1} (mod N)

            // Сначала проверим, что lp обратим по модулю N
            BigInteger N = ctx.N;
            BigInteger lpBig = lp;
            BigInteger g = BigInteger.GreatestCommonDivisor(lpBig, N);

            if (g != BigInteger.One)
            {
                // Теоретически здесь можно попытаться использовать g как делитель N,
                // но текущий интерфейс ISpqsRelationManager не позволяет
                // вернуть найденный фактор наружу, поэтому просто игнорируем
                // эту пару partial-отношений.
                return;
            }

            var ex1 = first.Exps;
            var ex2 = exps;
            int m = ex1.Length;

            var mergedExps = new ushort[m];
            for (int i = 0; i < m; i++)
                mergedExps[i] = (ushort)(ex1[i] + ex2[i]);

            BigInteger xCombined = first.XmodN * xmodN % N;
            BigInteger invLp = lpBig.Inverse(N);
            xCombined = xCombined * invLp % N;

            AddFullRelation(xCombined, mergedExps);
        }

        private void AddFullRelation(BigInteger xmodN, ushort[] exps)
        {
            lock (_fullGate)
            {
                _XmodN!.Add(xmodN);
                _Exps!.Add(exps);
                _fullCount = _Exps!.Count;
            }
        }
    }
}
