using MathLibrary.Factorization.BlockSieves;
using MathLibrary.Factorization.PolynomialSource;
using MathLibrary.Factorization.RelationManager;
using MathLibrary.Factorization.SmoothCheckers;
using MathLibrary.Factorization.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Factorization.RelationSchedulers
{
    public interface ISiqsRelationScheduler
    {
        void Collect(
            MpqsContext ctx,
            ISiqsBlockSieveFactory sieveFactory,
            ISmoothnessChecker smoothChecker,
            IMpqsRelationManager relMgr,
            ISiqsPolynomialSource polySrc,
            int needRelations,
            CancellationToken token);
    }

    public interface IMpqsRelationScheduler
    {
        void Collect(
            MpqsContext ctx,
            IMpqsBlockSieveFactory sieveFactory,
            ISmoothnessChecker smoothChecker,
            IMpqsRelationManager relMgr,
            IPolynomialSource polySrc,
            int needRelations,
            CancellationToken token);
    }

    public interface ISpqsRelationScheduler
    {
        void Collect(
            SpqsContext ctx,
            ISpqsBlockSieveFactory sieveFactory,
            ISmoothnessChecker smoothChecker,
            ISpqsRelationManager relMgr,
            int needRelations,
            BigInteger baseLeft0,
            CancellationToken token);
    }
}
