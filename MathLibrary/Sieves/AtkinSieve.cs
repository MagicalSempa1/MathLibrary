using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary
{
    public static partial class Sieves
    {
        public static int[] AtkinSieve(int limit, Predicate<int>? predicate = null)
        {
            List<int> primes = new List<int>(limit / (int)Math.Log(limit));
            BitArray isPrime = new BitArray(limit + 1);
            int squareRoot = (int)Math.Sqrt(limit);
            int xStepsize = 3;
            int y_limit;
            int n;

            #region 3x^2 + y^2 computation
            int temp = 12 * (int)Math.Sqrt((limit - 1) / 3);
            for (int i = 0; i < temp; i += 24)
            {
                xStepsize += i;
                y_limit = 12 * (int)Math.Sqrt(limit - xStepsize) - 36;
                n = xStepsize + 16;
                for (int j = -12; j <= y_limit; j += 72)
                {
                    n += j;
                    isPrime[n] = !isPrime[n];
                }
                n = xStepsize + 4;
                for (int j = 12; j <= y_limit; j += 72)
                {
                    n += j;
                    isPrime[n] = !isPrime[n];
                }
            }
            #endregion

            #region 4x^2 + y^2 computation
            xStepsize = 0;
            temp = ((int)Math.Sqrt(limit - 1 >> 2) << 3) + 4;
            for (int i = 4; i < temp; i += 24)
            {
                xStepsize += i;
                n = xStepsize + 1;
                int tempTwo = ((int)Math.Sqrt(limit - xStepsize) << 2) - 3;
                for (int j = 0; j < tempTwo; j += 8)
                {
                    n += j;
                    isPrime[n] = !isPrime[n];
                }
                if (i + 8 < temp)
                {
                    xStepsize += i + 8;
                    n = xStepsize + 1;
                    tempTwo = ((int)Math.Sqrt(limit - xStepsize) << 2) - 3;
                    for (int j = 0; j < tempTwo; j += 8)
                    {
                        n += j;
                        isPrime[n] = !isPrime[n];
                    }
                }
                if (i + 16 < temp)
                {
                    xStepsize += i + 16;
                    y_limit = 12 * (int)Math.Sqrt(limit - xStepsize) - 36;
                    n = xStepsize + 25;
                    for (int j = -24; j <= y_limit; j += 72)
                    {
                        n += j;
                        isPrime[n] = !isPrime[n];
                    }
                    n = xStepsize + 1;
                    for (int j = 24; j <= y_limit; j += 72)
                    {
                        n += j;
                        isPrime[n] = !isPrime[n];
                    }
                }
            }
            #endregion

            #region 3x^2 - y^2 computation
            xStepsize = 1;
            temp = (int)Math.Sqrt(limit >> 1) + 1;
            for (int i = 3; i < temp; i += 2)
            {
                xStepsize += (i << 2) - 4;
                n = 3 * xStepsize;
                int s;
                if (n > limit)
                {
                    int min_y = (int)Math.Sqrt(n - limit) >> 2 << 2;
                    n -= min_y * min_y;
                    s = (min_y << 2) + 4;
                }
                else s = 4;
                for (int j = s; j < i << 2; j += 8)
                {
                    n -= j;
                    if (n <= limit && n % 12 == 11)
                        isPrime[n] = !isPrime[n];
                }
            }
            xStepsize = 0;
            for (int i = 2; i < temp; i += 2)
            {
                xStepsize += (i << 2) - 4;
                n = 3 * xStepsize;
                int s;
                if (n > limit)
                {
                    int min_y = ((int)Math.Sqrt(n - limit) >> 2 << 2) - 1;
                    n -= min_y * min_y;
                    s = (min_y << 2) + 4;
                }
                else
                {
                    n -= 1;
                    s = 0;
                }
                for (int j = s; j < i << 2; j += 8)
                {
                    n -= j;
                    if (n <= limit && n % 12 == 11)
                        isPrime[n] = !isPrime[n];
                }
            }
            #endregion

            #region Eliminate Squares
            for (int i = 5; i <= squareRoot; i += 6)
            {
                if (isPrime[i])
                {
                    int k = i * i;
                    for (int z = k; z <= limit; z += k)
                        isPrime[z] = false;
                }
                if (isPrime[i + 2])
                {
                    int k = (i + 2) * (i + 2);
                    for (int z = k; z <= limit; z += k)
                        isPrime[z] = false;
                }
            }
            if (limit >= 2)
                if (predicate == null || predicate(2))
                    primes.Add(2);
            if (limit >= 3)
                if (predicate == null || predicate(3))
                    primes.Add(3);
            for (int i = 5; i <= limit; i += 6)
            {
                if (isPrime[i] && (predicate == null || predicate(i)))
                    primes.Add(i);
                if (isPrime[i + 2] && (predicate == null || predicate(i + 2)))
                    primes.Add(i + 2);
            }
            #endregion

            return primes.ToArray();
        }

        public static long[] AtkinSieve(long limit, Predicate<long>? predicate = null)
        {
            List<long> primes = new List<long>();
            var isPrime = new BitArray[limit / int.MaxValue + 1];
            for (int i = 0; i < isPrime.Length - 1; i++)
                isPrime[i] = new BitArray(int.MaxValue);
            isPrime[^1] = new BitArray((int)(limit % int.MaxValue + 1));
            long squareRoot = (long)Math.Sqrt(limit);
            long xStepsize = 3L;
            long y_limit;
            long n;

            #region 3x^2 + y^2 computation
            long temp = 12 * (long)Math.Sqrt((limit - 1L) / 3L);
            for (long i = 0; i < temp; i += 24L)
            {
                xStepsize += i;
                y_limit = 12L * (long)Math.Sqrt(limit - xStepsize) - 36L;
                n = xStepsize + 16L;
                for (long j = -12L; j <= y_limit; j += 72L)
                {
                    n += j;
                    isPrime[n / int.MaxValue][(int)(n % int.MaxValue)] = !isPrime[n / int.MaxValue][(int)(n % int.MaxValue)];
                }
                n = xStepsize + 4;
                for (long j = 12L; j <= y_limit; j += 72L)
                {
                    n += j;
                    isPrime[n / int.MaxValue][(int)(n % int.MaxValue)] = !isPrime[n / int.MaxValue][(int)(n % int.MaxValue)];
                }
            }
            #endregion

            #region 4x^2 + y^2 computation
            xStepsize = 0;
            temp = ((long)Math.Sqrt(limit - 1L >> 2) << 3) + 4L;
            for (long i = 4L; i < temp; i += 8L)
            {
                xStepsize += i;
                if (xStepsize % 3 != 0)
                {
                    n = xStepsize + 1L;
                    long tempTwo = 4 * (long)Math.Sqrt(limit - xStepsize) - 3L;
                    for (long j = 0L; j < tempTwo; j += 8L)
                    {
                        n += j;
                        isPrime[n / int.MaxValue][(int)(n % int.MaxValue)] = !isPrime[n / int.MaxValue][(int)(n % int.MaxValue)];
                    }
                }
                else
                {
                    y_limit = 12L * (long)Math.Sqrt(limit - xStepsize) - 36;
                    n = xStepsize + 25;
                    for (long j = -24L; j <= y_limit; j += 72L)
                    {
                        n += j;
                        isPrime[n / int.MaxValue][(int)(n % int.MaxValue)] = !isPrime[n / int.MaxValue][(int)(n % int.MaxValue)];
                    }
                    n = xStepsize + 1L;
                    for (long j = 24L; j <= y_limit; j += 72L)
                    {
                        n += j;
                        isPrime[n / int.MaxValue][(int)(n % int.MaxValue)] = !isPrime[n / int.MaxValue][(int)(n % int.MaxValue)];
                    }
                }
            }
            #endregion

            #region 3x^2 - y^2 computation
            xStepsize = 1L;
            temp = (long)Math.Sqrt(limit >> 1);
            for (long i = 3L; i <= temp; i += 2L)
            {
                xStepsize += (i << 2) - 4;
                n = 3 * xStepsize;
                long s;
                if (n > limit)
                {
                    long min_y = (long)Math.Sqrt(n - limit) >> 2 << 2;
                    n -= min_y * min_y;
                    s = (min_y << 2) + 4L;
                }
                else s = 4L;
                for (long j = s; j < i << 2; j += 8L)
                {
                    n -= j;
                    if (n <= limit && n % 12 == 11L)
                        isPrime[n / int.MaxValue][(int)(n % int.MaxValue)] = !isPrime[n / int.MaxValue][(int)(n % int.MaxValue)];
                }

            }
            xStepsize = 0;
            for (long i = 2L; i <= temp; i += 2L)
            {
                xStepsize += (i << 2) - 4L;
                n = 3L * xStepsize;
                long s;
                if (n > limit)
                {
                    long min_y = ((long)Math.Sqrt(n - limit) >> 2 << 2) - 1L;
                    n -= min_y * min_y;
                    s = (min_y << 2) + 4;
                }
                else
                {
                    n -= 1;
                    s = 0;
                }
                for (long j = s; j < i << 2; j += 8L)
                {
                    n -= j;
                    if (n <= limit && n % 12 == 11L)
                        isPrime[n / int.MaxValue][(int)(n % int.MaxValue)] = !isPrime[n / int.MaxValue][(int)(n % int.MaxValue)];
                }
            }
            #endregion

            #region Eliminate Squares
            for (long i = 5; i <= squareRoot; i += 6L)
            {
                if (isPrime[i / int.MaxValue][(int)(i % int.MaxValue)] == true)
                {
                    long k = i * i;
                    for (long z = k; z <= limit; z += k)
                        isPrime[z / int.MaxValue][(int)(z % int.MaxValue)] = false;
                }
                if (isPrime[(i + 2) / int.MaxValue][(int)((i + 2) % int.MaxValue)] == true)
                {
                    long k = (i + 2) * (i + 2);
                    for (long z = k; z <= limit; z += k)
                        isPrime[z / int.MaxValue][(int)(z % int.MaxValue)] = false;
                }
            }
            if (limit >= 2L)
                if (predicate == null || predicate(2L))
                    primes.Add(2L);
            if (limit >= 3L)
                if (predicate == null || predicate(3))
                    primes.Add(3L);
            for (long i = 5L; i <= limit; i += 6L)
            {
                if (isPrime[i / int.MaxValue][(int)(i % int.MaxValue)] && (predicate == null || predicate(i)))
                    primes.Add(i);
                if (isPrime[(i + 2) / int.MaxValue][(int)((i + 2) % int.MaxValue)] && (predicate == null || predicate(i + 2)))
                    primes.Add(i + 2);
            }
            #endregion

            return primes.ToArray();
        }

        public static int[] FindPrimes(int limit)
        {
            var isPrime = new BitArray(limit + 1);

            // Предварительно вычисленные наборы попадающих позиций
            var s1 = new bool[] { false, false, true, true, false, true, false, false, false, false, false, true, false, true, false, false };
            var s2 = new bool[] { false, true, true, true, false, true, false, true, false, false, false, true, false, true, false, false };
            var s3 = new bool[] { false, true, true, true, false, true, false, true, false, true, false, true, false, true, false, true };

            // Перебираем x и y до соответствующих пределов
            for (int x = 1; x * x <= limit; x++)
            {
                for (int y = 1; y * y <= limit; y++)
                {
                    // Алгоритм step 3.1
                    int n = 4 * x * x + y * y;
                    if (n <= limit && (s1[n % 16] || s2[n % 16] || s3[n % 16]))
                    {
                        isPrime[n] = !isPrime[n]; // toggle state
                    }

                    // Алгоритм step 3.2
                    n = 3 * x * x + y * y;
                    if (n <= limit && s2[n % 16])
                    {
                        isPrime[n] = !isPrime[n]; // toggle state
                    }

                    // Алгоритм step 3.3
                    n = 3 * x * x - y * y;
                    if (x > y && n <= limit && s3[n % 16])
                    {
                        isPrime[n] = !isPrime[n]; // toggle state
                    }
                }
            }

            // Избавляемся от повторных вычислений для квадратов
            for (int x = 1; x * x <= limit; x++)
            {
                for (int y = x; y * y <= limit; y++)
                {
                    int n = 3 * x * x - y * y;
                    if (n < 0)
                        continue;
                    if (n <= limit && s3[n % 16])
                    {
                        isPrime[n] = !isPrime[n]; // toggle state
                    }
                }
            }

            // Избавляемся от повторных вычислений для трехкратных квадратов
            for (int n = 5; n * n <= limit; n++)
            {
                if (isPrime[n])
                {
                    int nSquared = n * n;
                    for (int k = nSquared; k <= limit; k += nSquared)
                    {
                        isPrime[k] = false;
                    }
                }
            }

            // Формируем список простых чисел
            var primes = new List<int> { 2, 3, 5 };
            for (int n = 7; n <= limit; n++)
            {
                if (isPrime[n])
                {
                    primes.Add(n);
                }
            }

            return primes.ToArray();
        }
    }
}
