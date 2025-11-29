using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Factorization.Types
{
    public readonly record struct SmoothnessResult(bool Accepted, ushort[]? Exponents, BigInteger Remainder)
    {
        public bool IsFull => Accepted && Remainder.IsOne;
    }
}
