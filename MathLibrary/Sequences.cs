using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary
{
    public static class Sequences
    {
        public static BigInteger U(BigInteger n, BigInteger p, BigInteger q)
        {
            if (n == 0)
                return 2;
            if (n == 1)
                return p;
            if (n.IsEven)
                return U(n >> 1, p, q) * V(n >> 1, p, q);
            return BigInteger.Pow(U((n + 1) >> 1, p, q), 2) - q * BigInteger.Pow(U((n - 1) >> 1, p, q), 2);
        }

        public static BigInteger V(BigInteger n, BigInteger p, BigInteger q)
        {
            if (n == 0)
                return 2;
            if (n == 1)
                return p;
            if (n.IsEven)
                return BigInteger.Pow(V(n >> 1, p, q), 2) - 2 * BigInteger.Pow(q, (int)(n >> 1));
            else
                return V((n + 1) >> 1, p, q) * V((n - 1) >> 1, p, q) - BigInteger.Pow(q, (int)((n - 1) >> 1)) * p;
        }

        public static BigInteger Fib(BigInteger n)
        {
            if (n == 0)
                return 0;
            if (n == 1 || n == 2)
                return 1;
            var F = new Dictionary<BigInteger, BigInteger>
            {
                { 0, 0 },
                { 1, 1 },
                { 2, 1 },
                { 3, 2 },
                { 4, 3 },
                { 5, 5 },
                { 6, 8 },
                { 7, 13 },
                { 8, 21 },
                { 9, 34 },
                { 10, 55 },
                { 11, 89 },
                { 12, 144 },
                { 13, 233 },
                { 14, 377 },
                { 15, 610 },
                { 16, 987 },
                { 17, 1597 },
                { 18, 2584 },
                { 19, 4181 },
                { 20, 6765 },
                { 21, 10946 },
                { 22, 17711 },
                { 23, 28657 },
                { 24, 46368 }
            };
            BigInteger Fibonacci(BigInteger k)
            {
                if (F.TryGetValue(k, out BigInteger value))
                    return value;
                if (k.IsEven)
                {
                    if (!F.ContainsKey((k >> 1) - 1))
                        F.Add((k >> 1) - 1, Fibonacci((k >> 1) - 1));
                    if (!F.ContainsKey((k >> 1) + 1))
                        F.Add((k >> 1) + 1, Fibonacci((k >> 1) + 1));
                    return BigInteger.Pow(F[(k >> 1) + 1], 2) - BigInteger.Pow(F[(k >> 1) - 1], 2);
                }
                if (!F.ContainsKey((k - 1) >> 1))
                    F.Add((k - 1) >> 1, Fibonacci((k - 1) >> 1));
                if (!F.ContainsKey((k + 1) >> 1))
                    F.Add((k + 1) >> 1, Fibonacci((k + 1) >> 1));
                return BigInteger.Pow(F[(k - 1) >> 1], 2) + BigInteger.Pow(F[(k + 1) >> 1], 2);
            }
            return Fibonacci(n);
        }

        public static BigInteger Fib3(BigInteger n)
        {
            if (n == 0)
                return 0;
            if (n == 1 || n == 2)
                return 1;
            BigInteger m = n / 3;
            if (n % 3 == 0)
            {
                var f3 = Fib3(m);
                return 5 * BigInteger.Pow(f3, 3) + 3 * (m.IsEven ? f3 : -f3);
            }
            else if (n % 3 == 1)
            {
                var f1 = Fib3(m);
                var f2 = f1 + Fib3(m - 1);
                return BigInteger.Pow(f2, 3) + 3 * f2 * BigInteger.Pow(f1, 2) - BigInteger.Pow(f1, 3);
            }
            else
            {
                var f1 = Fib3(m);
                var f2 = f1 + Fib3(m - 1);
                return BigInteger.Pow(f2, 3) + 3 * f1 * BigInteger.Pow(f2, 2) + BigInteger.Pow(f1, 3);
            }
        }

        public static BigInteger Fib31(BigInteger n)
        {
            if (n == 0)
                return 0;
            if (n == 1 || n == 2)
                return 1;
            var F = new Dictionary<BigInteger, BigInteger>
            {
                { 0, 0 },
                { 1, 1 },
                { 2, 1 },
                { 3, 2 },
                { 4, 3 },
                { 5, 5 },
                { 6, 8 },
                { 7, 13 },
                { 8, 21 }
            };
            BigInteger Fibonacci(BigInteger k)
            {
                if (F.TryGetValue(k, out BigInteger value))
                    return value;
                if (k % 3 == 0)
                {
                    if (!F.ContainsKey(k / 3))
                        F.Add(k / 3, Fibonacci(k / 3));
                    return 5 * BigInteger.Pow(F[k / 3], 3) + 3 * (k.IsEven ? F[k / 3] : -F[k / 3]);
                }
                else if (k % 3 == 1)
                {
                    if (!F.ContainsKey(k / 3 - 1))
                        F.Add(k / 3 - 1, Fibonacci(k / 3 - 1));
                    if (!F.ContainsKey(k / 3))
                        F.Add(k / 3, Fibonacci(k / 3));
                    return BigInteger.Pow(F[k], 3) + 3 * F[k] * BigInteger.Pow(F[k], 2) - BigInteger.Pow(F[k], 3);
                }
                else
                {
                    if (!F.ContainsKey(k / 3 - 1))
                        F.Add(k / 3 - 1, Fibonacci(k / 3 - 1));
                    if (!F.ContainsKey(k / 3))
                        F.Add(k / 3, Fibonacci(k / 3));
                    return BigInteger.Pow(F[k], 3) + 3 * F[k] * BigInteger.Pow(F[k], 2) - BigInteger.Pow(F[k], 3);
                }
            }
            return Fibonacci(n);
        }

        public static BigInteger P(int n, int k)
        {
            if (k > n)
                k = n;
            if (k == 0)
            {
                if (n == 0)
                    return 1;
                return 0;
            }
            if (k == 1)
                return 1;
            if (k == 2)
                return n >> 1 + 1;
            if (n == k)
            {
                if (n == 3)
                    return 3;
                return P(n);
            }
            return P(n, k - 1) + P(n - k, k);
        }

        public static BigInteger P(int n)
        {
            //ArgumentOutOfRangeException.ThrowIfNegative(n);
            if (n == 0 || n == 1) return 1;
            BigInteger[] P = new BigInteger[n];
            P[0] = 1; P[1] = 1;
            BigInteger Partition(int k)
            {
                if (k < 0) return 0;
                if (k < n && P[k] != 0) return P[k];
                BigInteger result = 0;
                var list = new List<(int, int, int)>();
                for (int f = 1, q = 1; f <= k; f += 3 * q + 1, q++)
                {
                    if ((q & 1) == 1)
                        list.Add((1, k - f - q, k - f));
                    else
                        list.Add((-1, k - f - q, k - f));
                }
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (list[i].Item2 >= 0 && P[list[i].Item2] != 0)
                        result += list[i].Item1 * P[list[i].Item2];
                    else
                        result += list[i].Item1 * Partition(list[i].Item2);
                    if (P[list[i].Item3] != 0)
                        result += list[i].Item1 * P[list[i].Item3];
                    else
                        result += list[i].Item1 * Partition(list[i].Item3);
                }
                if (k != n) P[k] = result;
                
                return result;
            }
            return Partition(n);
        }

        public static BigInteger NewP(int n)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(n);
            var dp = new BigInteger[n + 1];
            dp[0] = BigInteger.One;

            for (int i = 1; i <= n; i++)
            {
                BigInteger sum = BigInteger.Zero;

                long g1 = 1, g2 = 2;
                long inc1 = 4, inc2 = 5;
                bool isPos = true;

                while (g1 <= i)
                {
                    if (isPos)
                    {
                        sum += dp[i - (int)g1];
                        if (g2 <= i) sum += dp[i - (int)g2];
                    }
                    else
                    {
                        sum -= dp[i - (int)g1];
                        if (g2 <= i) sum -= dp[i - (int)g2];
                    }

                    g1 += inc1; inc1 += 3;
                    g2 += inc2; inc2 += 3;
                    isPos = !isPos;
                }

                dp[i] = sum;
            }

            return dp[n];
        }

        public static BigInteger StirlingNumberOfFirstKind(BigInteger n, BigInteger k)
        {
            if (n == 0)
            {
                if (k == 0)
                    return 1;
                return 0;
            }
            if (k == 0)
                return 0;
            return StirlingNumberOfFirstKind(n - 1, k - 1) - (n - 1) * StirlingNumberOfFirstKind(n - 1, k);
        }

        public static BigInteger StirlingNumberOfSecondKind(BigInteger n, BigInteger k)
        {
            BigInteger result = 0;
            for (BigInteger i = 0; i <= k; i++)
            {
                if ((k + i) % 2 == 0)
                    result += OtherFunctions.Binomial(k, i) * i.Pow(n);
                else
                    result -= OtherFunctions.Binomial(k, i) * i.Pow(n);
            }
            return result / OtherFunctions.Factorial((int)k);
        }

        public static BigInteger Fusc(BigInteger n)
        {
            if (n.IsPowerOfTwo)
                return 1;
            BigInteger a = 1;
            BigInteger b = 0;
            while (n != 0)
            {
                if (n % 2 == 0)
                {
                    a += b;
                    n >>= 1;
                }
                else
                {
                    b += a;
                    n = (n - 1) >> 1;
                }
            }
            return b;
        }

        public static BigInteger MersenneNumber(BigInteger n)
        {
            int r = (int)(n / int.MaxValue);
            BigInteger result = 1;
            for (int i = 0; i < r; i++)
                result <<= int.MaxValue;
            return (result << (int)(n % int.MaxValue)) - 1;
        }

        public static BigInteger FermatNumber(int n)
        {
            BigInteger result = 1;
            BigInteger p;
            for (p = BigInteger.One << n; p > int.MaxValue; p -= int.MaxValue)
                result *= BigInteger.One << int.MaxValue;
            return result * (BigInteger.One << (int)p) + 1;
        }

        public static Func<double, double> ChebyshevPolynomialFirstKind(int n)
        {
            if (n == 0)
                return (x) => 1;
            if (n == 1)
                return (x) => x;
            if (n % 2 == 0)
                return (x) => 2 * System.Math.Pow(ChebyshevPolynomialFirstKind(n >> 1)(x), 2) - 1;
            else
                return (x) => 2 * ChebyshevPolynomialFirstKind((n + 1) >> 1)(x) * ChebyshevPolynomialFirstKind((n - 1) >> 1)(x) - x;
        }

        public static Func<double, double> ChebyshevPolynomialSecondKind(int n) // U_m(x)^2 = sum U_2k from 0 to m
        {
            if (n == 0)
                return (x) => 1;
            if (n == 1)
                return (x) => 2 * x;
            if (n == 2)
                return (x) => 4 * System.Math.Pow(x, 2) - 1;
            if (n == 3)
                return (x) => 8 * System.Math.Pow(x, 3) - 4 * x;
            return (x) => ChebyshevPolynomialSecondKind(n - 2)(x) * (4 * System.Math.Pow(x, 2) - 2) - ChebyshevPolynomialSecondKind(n - 4)(x);
        }

        public static Func<double, double> LaguerePolynomial(int n)
        {
            if (n == 0)
                return (x) => 1;
            if (n == 1)
                return (x) => 1 - x;
            return (x) => ((2 * n - 1 - x) * LaguerePolynomial(n - 1)(x) - (n - 1) * LaguerePolynomial(n - 2)(x)) / n;
        }

        public static Func<double, double> GeneralizedLaguerePolynomial(int n, double a)
        {
            if (n == 0)
                return (x) => 1;
            if (n == 1)
                return (x) => 1 - x;
            return (x) => ((2 * n - 1 + a - x) * GeneralizedLaguerePolynomial(n - 1, a)(x) - (n - 1 + a) * GeneralizedLaguerePolynomial(n - 2, a)(x)) / n;
        }

        public static double F(int n, double x) => n > 1 ? F(n - 1, x) / F(n - 1, x + (1 << (n - 1))) : x;
    }

}
