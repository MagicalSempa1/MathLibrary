using MathLibrary.Factorization.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Factorization.RelationManager
{
    public interface IMpqsRelationManager
    {
        void InitializeTargets(List<BigInteger> xmodN, List<ushort[]> Exps);

        int FullCount { get; }

        void SubmitCandidate(MpqsContext ctx, BigInteger xmodN, ushort[] exps, BigInteger remainder);
    }

    public interface ISpqsRelationManager
    {
        void InitializeTargets(List<BigInteger> xmodN, List<ushort[]> Exps);

        int FullCount { get; }

        void SubmitCandidate(SpqsContext ctx, BigInteger xmodN, ushort[] exps, BigInteger remainder);
    }
}
