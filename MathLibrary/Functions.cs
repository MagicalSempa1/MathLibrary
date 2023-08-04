using System.Numerics;

namespace MathLibrary
{
    public static class Functions
    {
        public static BigInteger Factorial(long n)
        {
            var primes = Sieves.AtkinSieve(n);
            BigInteger result = 1;
            for (int i = 0; i < primes.Length; i++)
            {
                BigInteger power = n / primes[i];
                BigInteger t = BigInteger.Pow(primes[i], 2);
                while (t <= n)
                {
                    power += n / t;
                    t *= primes[i];
                }
                result *= Pow(primes[i], power);
            }
            return result;
        }

        public static BigInteger Binomial(BigInteger n, BigInteger k)
        {
            if (n == k)
                return 1;
            if (n < k)
                return 0;
            BigInteger result = 1;
            BigInteger d = 1;
            for (BigInteger i = 0; i < k; i++)
            {
                result *= n - i;
                d *= i + 1;
            }
            return result / d;
        }

        public static BigInteger Pow(this BigInteger n, BigInteger m)
        {
            if (m <= int.MaxValue)
                return BigInteger.Pow(n, (int)m);
            return BigInteger.Pow(BigInteger.Pow(n, int.MaxValue), (int)(m / int.MaxValue)) * BigInteger.Pow(n, (int)(m % int.MaxValue));
        }

        public static (BigInteger gcd, BigInteger x, BigInteger y) GCD(BigInteger a, BigInteger b)
        {
            (BigInteger gcd, BigInteger x, BigInteger y) retvals = (0, 0, 0);
            (BigInteger, BigInteger) aa = (1, 0), bb = (0, 1);
            BigInteger q;
            while (true)
            {
                q = a / b; a %= b;
                aa.Item1 -= q * aa.Item2; bb.Item1 -= q * bb.Item2;
                if (a == 0)
                {
                    retvals.gcd = b; retvals.x = aa.Item2; retvals.y = bb.Item2;
                    return retvals;
                };
                q = b / a; b %= a;
                aa.Item2 -= q * aa.Item1; bb.Item2 -= q * bb.Item1;
                if (b == 0)
                {
                    retvals.gcd = a; retvals.x = aa.Item1; retvals.y = bb.Item1;
                    return retvals;
                };
            }
        }

        public static double PowerMean(double d, params double[] values)
        {
            double result;
            if (d == 0)
            {
                result = 1;
                for (int i = 0; i < values.Length; i++)
                {
                    result *= values[i];
                }
                return Math.Pow(result, 1.0 / values.Length);
            }
            if (d == 1)
                return values.Average();
            if (d == double.NegativeInfinity)
                return values.Min();
            if (d == double.PositiveInfinity)
                return values.Max();
            result = 0;
            for (int i = 0; i < values.Length; i++)
            {
                result += Math.Pow(values[i], d);
            }
            result *= 1.0 / values.Length;
            return Math.Pow(result, 1 / d);
        }

        public static double AGM(double x, double y, int n)
        {
            if (x == y)
                return x;
            double a_0 = x, a_1 = 0;
            double g_0 = y, g_1 = 0;
            for (int i = 0; i < n; i++)
            {
                a_1 = (a_0 + g_0) / 2;
                g_1 = Math.Sqrt(a_0 * g_0);
                a_0 = a_1;
                g_0 = g_1;
            }
            return (a_1 + g_1) / 2;
        }
    }
}