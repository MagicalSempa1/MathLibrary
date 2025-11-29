using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Factorization.Types
{
    public sealed class QSContext(BigInteger n, MpqsOptions opt)
    {
        public readonly BigInteger N = n;
        public readonly MpqsOptions Options = opt;
        public int[] FB = [];
        public SievePrime[] SP = [];
        public BigInteger XStart;
        public QSPolynomial Polynomial = QSPolynomial.MonicSquareMinusN(n);
    }
}
