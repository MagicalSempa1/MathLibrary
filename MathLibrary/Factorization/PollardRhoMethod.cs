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
        public static BigInteger[] PollardRhoMethod(BigInteger n)
        {
            BigInteger x = 2;
            BigInteger y = 2;
            BigInteger gcd;
            do
            {
                x = (x * x + 1) % n;
                BigInteger t = y * y;
                y = (t * (t + 2) + 2) % n;
                gcd = BigInteger.GreatestCommonDivisor(n, BigInteger.Abs(x - y));
            } while (gcd == 1 || gcd == n);
            return [gcd, n / gcd];
        }

        public static BigInteger[] ModyfiedPollardRhoMethod(BigInteger n)
        {
            BigInteger x = 2;
            BigInteger y = 2;
            BigInteger gcd;
            BigInteger i = 0;
            BigInteger stage = 2;
            do
            {
                if (i == stage)
                {
                    y = x;
                    stage <<= 1;
                }
                x = (x * x + 1) % n;
                i++;
                gcd = BigInteger.GreatestCommonDivisor(n, BigInteger.Abs(x - y));
            } while (gcd == 1 || gcd == n);
            return [gcd, n / gcd];
        }
    }
}
