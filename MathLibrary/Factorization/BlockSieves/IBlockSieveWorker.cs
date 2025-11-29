using MathLibrary.Factorization.RelationManager;
using MathLibrary.Factorization.Types;
using System.Numerics;

namespace MathLibrary.Factorization.BlockSieves
{
    public interface ISiqsBlockSieveWorker
    {
        /// <summary>Строит план решета для первой полиномной семьи (данное A,B и массив B_v).</summary>
        SiqsSievePlan BuildInitialPlan(
            MpqsContext ctx,
            SievePrime[] sp,
            QSPolynomial poly,
            BigInteger baseLeft0,
            BigInteger[] bTerms);

        /// <summary>Инкрементальное обновление корней r1/r2 (и r1p2/r2p2) при переходе к новому B.</summary>
        void UpdateRoots(SiqsSievePlan plan, int flippedIndex, int flipSign);

        /// <summary>Просеять один блок с текущими корнями и отправить кандидатов в менеджер отношений.</summary>
        void SieveBlock(
            MpqsContext ctx,
            SiqsSievePlan plan,
            long blockIndex,
            IMpqsRelationManager relMgr);
    }

    public interface IMpqsBlockSieveWorker
    {
        PolySievePlan BuildPlan(MpqsContext ctx, SievePrime[] sp, QSPolynomial poly, BigInteger baseLeft0);
        void SieveBlock(MpqsContext ctx, in PolySievePlan plan, long blockIndex, IMpqsRelationManager relMgr);
    }

    public interface ISpqsBlockSieveWorker
    {
        SpqsSievePlan BuildPlan(SpqsContext ctx, SievePrime[] sp, BigInteger baseLeft0);
        void SieveBlock(SpqsContext ctx, in SpqsSievePlan plan, long blockIndex, ISpqsRelationManager relMgr);
    }
}
