using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.EllipticCurves
{
    public record EllipticCurve
    {
        public static BigInteger? Factorize(BigInteger n, BigInteger a, BigInteger x, BigInteger y)
        {
            Point basePoint = new Point(x, y);
            Point currentPoint = new Point(x, y);

            for (BigInteger i = 2; i <= n; i++)
            {
                currentPoint = Point.Add(currentPoint, basePoint, a, n);
                if (currentPoint.IsInfinity)
                {
                    BigInteger divisor = BigInteger.GreatestCommonDivisor(n, i);
                    if (divisor > 1) return divisor;
                }
            }

            return null;
        }
    }
}
