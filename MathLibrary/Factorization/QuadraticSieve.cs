using System.Numerics;

namespace MathLibrary.Factorization
{
    public static partial class Factorization
    {
        public static BigInteger[] QSMethod(BigInteger n)
        {
            var factorBase = Sieves.AtkinSieve(100, p => ArithmeticFunctions.JacobiSymbol(n, p) == 1);
            throw new NotImplementedException();
        }
    }
}
