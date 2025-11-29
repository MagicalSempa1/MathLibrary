using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Factorization.Types
{
    public readonly record struct SmoothnessOptions(int MaxLargePrimes, BigInteger LargePrimeBound);
}
