using MathLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Factorization
{
    public static partial class Factorization
    {
        public static BigInteger[] FermatMethod(BigInteger n)
        {
            var primes = new List<BigInteger>();
            while (n.IsEven)
            {
                primes.Add(2);
                n /= 2;
            }
            if (n == 1)
                return [.. primes];
            BigInteger x = n.CeilingSqrt();
            if (n == x * x)
                return [.. primes, x, x];
            var y2 = x * x - n;
            BigInteger y = 0;
            for (int k = 0; k < n; k++)
            {
                if (y2.IsSqrt(ref y))
                {
                    y2 = (y2 + n).FloorSqrt();
                    var gcd = BigInteger.GreatestCommonDivisor(y2 - y, n);
                    if (gcd != 1 & gcd != n)
                    {
                        primes.Add(gcd);
                        n /= gcd;
                        primes.Add(n);
                        return [.. primes];
                    }
                    gcd = BigInteger.GreatestCommonDivisor(y2 + y, n);
                    if (gcd != 1 & gcd != n)
                    {
                        primes.Add(gcd);
                        n /= gcd;
                        primes.Add(n);
                        return [.. primes];
                    }
                }
                x++;
                y2 += (x << 1) - 1;
            }
            return [.. primes];
        }

    }
}
