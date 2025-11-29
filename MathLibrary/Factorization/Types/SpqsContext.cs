using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Factorization.Types
{
    public sealed class SpqsContext(BigInteger n, SpqsOptions opt)
    {
        public readonly BigInteger N = n;
        public readonly SpqsOptions Options = opt;

        public int[] FB = [];
        public SievePrime[] SP = [];

        public BigInteger XStart;

        public long NextBlockLocal;
    }
}
