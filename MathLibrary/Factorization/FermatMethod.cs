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
            List<BigInteger> primes = new List<BigInteger>();
            while (n % 2 == 0)
            {
                primes.Add(2);
                n /= 2;
            }
            if (n == 1)
                return primes.ToArray();
            BigInteger sqrt = n.CeillingSqrt();
            if (n == sqrt * sqrt)
                return new BigInteger[] { sqrt, sqrt };
            var x = sqrt * sqrt - n;
            BigInteger y = 0;
            for (int k = 0; k < n; k++)
            {
                if (x.IsSqrt(ref y))
                {
                    x = (x + n).FloorSqrt();
                    var gcd = BigInteger.GreatestCommonDivisor(x - y, n);
                    if (gcd != 1 & gcd != n)
                    {
                        primes.Add(gcd);
                        n /= gcd;
                    }
                    gcd = BigInteger.GreatestCommonDivisor(x + y, n);
                    if (gcd != 1 & gcd != n)
                    {
                        primes.Add(gcd);
                        n /= gcd;
                    }
                    return primes.ToArray();
                }
                x += ((sqrt + k) << 1) + 1;
            }
            return primes.ToArray();
        }

    }
}
