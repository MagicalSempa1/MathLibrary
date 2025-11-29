using MathLibrary.Factorization.SmoothCheckers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Factorization.BlockSieves
{
    public interface ISiqsBlockSieveFactory
    {
        void SetSmoothChecker(ISmoothnessChecker smooth);
        ISiqsBlockSieveWorker CreateWorker();
    }

    public interface IMpqsBlockSieveFactory
    {
        void SetSmoothChecker(ISmoothnessChecker smooth);
        IMpqsBlockSieveWorker CreateWorker();
    }

    public interface ISpqsBlockSieveFactory
    {
        void SetSmoothChecker(ISmoothnessChecker smooth);
        ISpqsBlockSieveWorker CreateWorker();
    }
}
