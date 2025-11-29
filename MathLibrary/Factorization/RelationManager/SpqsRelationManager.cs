using MathLibrary.Factorization.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Factorization.RelationManager
{
    public sealed class SpqsRelationManager : ISpqsRelationManager
    {
        private List<BigInteger>? _XmodN;
        private List<ushort[]>? _Exps;
        private int _fullCount;

        private readonly object _gate = new();

        public int FullCount => Volatile.Read(ref _fullCount);

        public void InitializeTargets(List<BigInteger> xmodN, List<ushort[]> Exps)
        {
            _XmodN = xmodN;
            _Exps = Exps;
            _fullCount = Exps.Count;
        }

        public void SubmitCandidate(SpqsContext ctx, BigInteger xmodN, ushort[] exps, BigInteger remainder)
        {
            if (!remainder.IsOne) return;

            lock (_gate)
            {
                _XmodN!.Add(xmodN);
                _Exps!.Add(exps);
            }
            _ = Interlocked.Increment(ref _fullCount);
        }
    }
}
