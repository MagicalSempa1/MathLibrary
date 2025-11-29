using System.Numerics;
using System.Runtime.CompilerServices;

namespace MathLibrary.Factorization.Types
{
    public readonly struct QSPolynomial(BigInteger a, BigInteger b, BigInteger c)
    {
        public readonly BigInteger A = a;
        public readonly BigInteger B = b;
        public readonly BigInteger C = c;

        public static QSPolynomial MonicSquareMinusN(BigInteger N) => new QSPolynomial(1, 0, -N);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InitBlock(BigInteger leftX, out BigInteger y0, out BigInteger inc0, out BigInteger incStep)
        {
            y0 = A * leftX * leftX + (B << 1) * leftX + C;
            inc0 = (A << 1) * leftX + (A + (B << 1));
            incStep = A << 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsMonicX2MinusN(in BigInteger N) => A.IsOne && B.IsZero && C == -N;
    }
}