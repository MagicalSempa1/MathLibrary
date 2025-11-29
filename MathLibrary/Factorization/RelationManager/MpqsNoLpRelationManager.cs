using MathLibrary.Factorization.Types;
using System.Diagnostics;
using System.Numerics;

namespace MathLibrary.Factorization.RelationManager
{
    public sealed class MpqsNoLpRelationManager : IMpqsRelationManager
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

        public void SubmitCandidate(MpqsContext ctx, BigInteger xmodN, ushort[] exps, BigInteger remainder)
        {
            if (!remainder.IsOne) return;

            lock (_gate)
            {
                _XmodN!.Add(xmodN);
                _Exps!.Add(exps);

                _fullCount = _Exps!.Count;
            }
        }
    }
}
