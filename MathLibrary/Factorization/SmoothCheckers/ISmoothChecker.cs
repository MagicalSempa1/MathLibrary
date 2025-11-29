using MathLibrary.Factorization.RelationManager;
using MathLibrary.Factorization.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Factorization.SmoothCheckers
{
    public interface ISmoothnessChecker
    {
        SmoothnessResult Check(BigInteger value, ReadOnlySpan<int> FB, in SmoothnessOptions options);
    }
}
