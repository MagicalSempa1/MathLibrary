using MathLibrary.Factorization.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Factorization.RelationManager
{
    public sealed class MpqsOneLpRelationManager : IMpqsRelationManager
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

        private List<BigInteger>? _XmodN;
        private List<ushort[]>? _Exps;

        private int _fullCount;
        public int FullCount => Volatile.Read(ref _fullCount);

        private readonly object _fullGate = new();

        private sealed class Shard
        {
            public readonly object Gate = new();
            public readonly Dictionary<int, PartialRelation> Partials = new Dictionary<int, PartialRelation>();
        }

        private const int ShardCount = 16;
        private const int ShardMask = ShardCount - 1;

        private readonly Shard[] _shards;

        public MpqsOneLpRelationManager()
        {
            _shards = new Shard[ShardCount];
            for (int i = 0; i < ShardCount; i++)
                _shards[i] = new Shard();
        }

        public void InitializeTargets(List<BigInteger> xmodN, List<ushort[]> Exps)
        {
            lock (_fullGate)
            {
                _XmodN = xmodN;
                _Exps = Exps;
                _fullCount = Exps.Count;
            }

            for (int s = 0; s < ShardCount; s++)
            {
                var shard = _shards[s];
                lock (shard.Gate)
                {
                    shard.Partials.Clear();
                }
            }
        }

        public void SubmitCandidate(MpqsContext ctx, BigInteger xmodN, ushort[] exps, BigInteger remainder)
        {
            if (remainder.IsOne)
            {
                AddFullRelation(xmodN, exps);
                return;
            }

            if (ctx.Options.EnableLargePrimes == QSLPOpt.NoLP)
                return;

            if (remainder.Sign <= 0)
                return;

            var fb = ctx.FB;
            if (fb.Length == 0)
                return;

            var maxLp = (BigInteger)(fb[^1] * ctx.Options.LargePrimeBoundMultiplier);
            if (remainder > maxLp)
                return;

            if (remainder > int.MaxValue)
                return;

            int lp = (int)remainder;

            var shard = _shards[lp & ShardMask];

            ushort[]? mergedExps = null;
            BigInteger xmodNCombined = BigInteger.Zero;

            lock (shard.Gate)
            {
                if (shard.Partials.TryGetValue(lp, out var existing))
                {
                    shard.Partials.Remove(lp);

                    var ex1 = existing.Exps;
                    var ex2 = exps;
                    int m = ex1.Length;

                    var merged = new ushort[m];
                    for (int i = 0; i < m; i++)
                        merged[i] = (ushort)(ex1[i] + ex2[i]);

                    mergedExps = merged;

                    xmodNCombined = existing.XmodN * xmodN % ctx.N;
                }
                else
                {
                    shard.Partials[lp] = new PartialRelation(xmodN, exps);
                }
            }

            if (mergedExps is null)
                return;

            AddFullRelation(xmodNCombined, mergedExps);
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
