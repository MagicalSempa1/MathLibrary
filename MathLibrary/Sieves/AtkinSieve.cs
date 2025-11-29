using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary
{
    public static partial class Sieves
    {
        private static readonly byte[] _mask12 = [0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 0, 0, 0, 0, 0, 2, 0, 0, 0, 4, 0, 1, 0, 4, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 2, 5, 0, 0, 0, 4, 2, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 2, 0, 1, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 4, 0, 1, 0, 4, 0, 2, 0, 0, 0, 0, 0, 2, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 5, 0, 2, 0, 4, 0, 0, 0, 6, 0, 0, 0];

        public static int[] AtkinSieve(int limit)
        {
            if (limit < 2) return [];
            if (limit == 2) return [2];
            if (limit == 3) return [2, 3];

            var primes = new List<int>(limit / (int)Math.Log(limit));
            var isPrime = new BitArray(limit + 1);
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
                    if ((uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                }
                n = xStepsize + 4;
                for (int j = 12; j <= y_limit; j += 72)
                {
                    n += j;
                    if ((uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
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
                    if ((uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                }
                if (i + 8 < temp)
                {
                    xStepsize += i + 8;
                    n = xStepsize + 1;
                    tempTwo = ((int)Math.Sqrt(limit - xStepsize) << 2) - 3;
                    for (int j = 0; j < tempTwo; j += 8)
                    {
                        n += j;
                        if ((uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
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
                        if ((uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                    }
                    n = xStepsize + 1;
                    for (int j = 24; j <= y_limit; j += 72)
                    {
                        n += j;
                        if ((uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
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

                int r0 = n % 12;
                byte m = _mask12[r0 * 12 + (s % 12)];

                int jlim = i << 2;
                int j = s;

                for (; j + 16 < jlim; j += 24)
                {
                    n -= j; if ((m & 0b001) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                    int j1 = j + 8; n -= j1; if ((m & 0b010) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                    int j2 = j + 16; n -= j2; if ((m & 0b100) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                }

                if (j < jlim)
                {
                    n -= j; if ((m & 0b001) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                    if (j + 8 < jlim)
                    {
                        int j1 = j + 8; n -= j1; if ((m & 0b010) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                    }
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

                int r0 = n % 12;
                byte m = _mask12[r0 * 12 + (s % 12)];

                int jlim = i << 2;
                int j = s;

                for (; j + 16 < jlim; j += 24)
                {
                    n -= j; if ((m & 0b001) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                    int j1 = j + 8; n -= j1; if ((m & 0b010) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                    int j2 = j + 16; n -= j2; if ((m & 0b100) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                }
                if (j < jlim)
                {
                    n -= j; if ((m & 0b001) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                    if (j + 8 < jlim)
                    {
                        int j1 = j + 8; n -= j1; if ((m & 0b010) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                    }
                }
            }
            #endregion

            #region Eliminate Squares
            int cutoff = Math.Min(squareRoot, 46338);

            for (int i = 5; i <= cutoff; i += 6)
            {
                if (isPrime[i])
                {
                    int k = i * i;
                    for (int z = k; z <= limit; z += k) isPrime[z] = false;
                }

                int p = i + 2;
                if (isPrime[p])
                {
                    int k = p * p;
                    for (int z = k; z <= limit; z += k) isPrime[z] = false;
                }
            }

            if (squareRoot > cutoff)
            {
                int i0 = cutoff + 5;

                for (int i = i0; i <= squareRoot; i += 6)
                {
                    if (isPrime[i])
                    {
                        long kk = 1L * i * i;
                        for (long z = kk; z <= limit; z += kk) isPrime[(int)z] = false;
                    }

                    int p = i + 2;
                    if (p <= limit && isPrime[p])
                    {
                        long kk = 1L * p * p;
                        if (kk <= limit)
                            for (long z = kk; z <= limit; z += kk) isPrime[(int)z] = false;
                    }
                }
            }

            if (limit >= 2)
                primes.Add(2);
            if (limit >= 3)
                primes.Add(3);

            limit -= 6;
            int l;
            for (l = 5; l <= limit; l += 6)
            {
                if (isPrime[l])
                    primes.Add(l);
                if (isPrime[l + 2])
                    primes.Add(l + 2);
            }
            limit += 6;
            if (isPrime[l])
                primes.Add(l);
            l += 2;
            if (l <= limit && isPrime[l])
                primes.Add(l);
            #endregion

            return primes.ToArray();
        }


        public static int[] AtkinSieve(int limit, Predicate<int> predicate)
        {
            if (predicate is null)
                return AtkinSieve(limit);
            if (limit < 2) return [];
            if (limit == 2) return predicate(2) ? [2] : [];
            if (limit == 3) return predicate(2) ? (predicate(3) ? [2, 3] : [2]) : (predicate(3) ? [3] : []);

            var primes = new List<int>(limit / (int)Math.Log(limit));
            var isPrime = new BitArray(limit + 1);
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
                    if ((uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                }
                n = xStepsize + 4;
                for (int j = 12; j <= y_limit; j += 72)
                {
                    n += j;
                    if ((uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
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
                    if ((uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                }
                if (i + 8 < temp)
                {
                    xStepsize += i + 8;
                    n = xStepsize + 1;
                    tempTwo = ((int)Math.Sqrt(limit - xStepsize) << 2) - 3;
                    for (int j = 0; j < tempTwo; j += 8)
                    {
                        n += j;
                        if ((uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
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
                        if ((uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                    }
                    n = xStepsize + 1;
                    for (int j = 24; j <= y_limit; j += 72)
                    {
                        n += j;
                        if ((uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
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

                int r0 = n % 12;
                byte m = _mask12[r0 * 12 + (s % 12)];

                int jlim = i << 2;
                int j = s;

                for (; j + 16 < jlim; j += 24)
                {
                    n -= j; if ((m & 0b001) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                    int j1 = j + 8; n -= j1; if ((m & 0b010) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                    int j2 = j + 16; n -= j2; if ((m & 0b100) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                }

                if (j < jlim)
                {
                    n -= j; if ((m & 0b001) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                    if (j + 8 < jlim)
                    {
                        int j1 = j + 8; n -= j1; if ((m & 0b010) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                    }
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

                int r0 = n % 12;
                byte m = _mask12[r0 * 12 + (s % 12)];

                int jlim = i << 2;
                int j = s;

                for (; j + 16 < jlim; j += 24)
                {
                    n -= j; if ((m & 0b001) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                    int j1 = j + 8; n -= j1; if ((m & 0b010) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                    int j2 = j + 16; n -= j2; if ((m & 0b100) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                }
                if (j < jlim)
                {
                    n -= j; if ((m & 0b001) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                    if (j + 8 < jlim)
                    {
                        int j1 = j + 8; n -= j1; if ((m & 0b010) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                    }
                }
            }
            #endregion

            #region Eliminate Squares
            int cutoff = Math.Min(squareRoot, 46338);

            for (int i = 5; i <= cutoff; i += 6)
            {
                if (isPrime[i])
                {
                    int k = i * i;
                    for (int z = k; z <= limit; z += k) isPrime[z] = false;
                }

                int p = i + 2;
                if (isPrime[p])
                {
                    int k = p * p;
                    for (int z = k; z <= limit; z += k) isPrime[z] = false;
                }
            }

            if (squareRoot > cutoff)
            {
                int i0 = cutoff + 5;

                for (int i = i0; i <= squareRoot; i += 6)
                {
                    if (isPrime[i])
                    {
                        long kk = 1L * i * i;
                        for (long z = kk; z <= limit; z += kk) isPrime[(int)z] = false;
                    }

                    int p = i + 2;
                    if (p <= limit && isPrime[p])
                    {
                        long kk = 1L * p * p;
                        if (kk <= limit)
                            for (long z = kk; z <= limit; z += kk) isPrime[(int)z] = false;
                    }
                }
            }

            if (limit >= 2)
                primes.Add(2);
            if (limit >= 3)
                primes.Add(3);

            limit -= 6;
            int l;
            for (l = 5; l <= limit; l += 6)
            {
                if (isPrime[l])
                    primes.Add(l);
                if (isPrime[l + 2])
                    primes.Add(l + 2);
            }
            limit += 6;
            if (isPrime[l])
                primes.Add(l);
            l += 2;
            if (l <= limit && isPrime[l])
                primes.Add(l);
            #endregion

            return primes.ToArray();
        }

        public static int[] AtkinSieveExtendToNo(int N)
        {
            //if (N <= 0) return [];
            //if (N == 1) return [2];
            //if (N == 2) return [2, 3];
            //if (N == 3) return [2, 3, 5];
            //if (N == 4) return [2, 3, 5, 7];
            //if (N == 5) return [2, 3, 5, 7, 11];
            //if (N == 6) return [2, 3, 5, 7, 11, 13];
            //if (N == 7) return [2, 3, 5, 7, 11, 13, 17];
            //if (N == 8) return [2, 3, 5, 7, 11, 13, 17, 19];
            //if (N == 9) return [2, 3, 5, 7, 11, 13, 17, 19, 23];
            //if (N == 10) return [2, 3, 5, 7, 11, 13, 17, 19, 23, 29];
            //if (N == 11) return [2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31];

            int limit;

            var primes = new int[N];
            primes[0] = 2;
            primes[1] = 3;
            int count = 2;

            double logN = Math.Log(N);
            double logLogN = Math.Log(logN);
            limit = (int)(N * (logN + logLogN - 1 + logLogN / logN)) + 5;

            var isPrime = new BitArray(limit + 1);
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
                    if ((uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                }
                n = xStepsize + 4;
                for (int j = 12; j <= y_limit; j += 72)
                {
                    n += j;
                    if ((uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
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
                    if ((uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                }
                if (i + 8 < temp)
                {
                    xStepsize += i + 8;
                    n = xStepsize + 1;
                    tempTwo = ((int)Math.Sqrt(limit - xStepsize) << 2) - 3;
                    for (int j = 0; j < tempTwo; j += 8)
                    {
                        n += j;
                        if ((uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
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
                        if ((uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                    }
                    n = xStepsize + 1;
                    for (int j = 24; j <= y_limit; j += 72)
                    {
                        n += j;
                        if ((uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
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

                int r0 = n % 12;
                byte m = _mask12[r0 * 12 + (s % 12)];

                int jlim = i << 2;
                int j = s;

                for (; j + 16 < jlim; j += 24)
                {
                    n -= j; if ((m & 0b001) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                    int j1 = j + 8; n -= j1; if ((m & 0b010) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                    int j2 = j + 16; n -= j2; if ((m & 0b100) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                }

                if (j < jlim)
                {
                    n -= j; if ((m & 0b001) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                    if (j + 8 < jlim)
                    {
                        int j1 = j + 8; n -= j1; if ((m & 0b010) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                    }
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

                int r0 = n % 12;
                byte m = _mask12[r0 * 12 + (s % 12)];

                int jlim = i << 2;
                int j = s;

                for (; j + 16 < jlim; j += 24)
                {
                    n -= j; if ((m & 0b001) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                    int j1 = j + 8; n -= j1; if ((m & 0b010) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                    int j2 = j + 16; n -= j2; if ((m & 0b100) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                }
                if (j < jlim)
                {
                    n -= j; if ((m & 0b001) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                    if (j + 8 < jlim)
                    {
                        int j1 = j + 8; n -= j1; if ((m & 0b010) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                    }
                }
            }
            #endregion

            #region Eliminate Squares
            int cutoff = Math.Min(squareRoot, 46338);

            for (int i = 5; i <= cutoff; i += 6)
            {
                if (isPrime[i])
                {
                    int k = i * i;
                    for (int z = k; z <= limit; z += k) isPrime[z] = false;
                }

                int p = i + 2;
                if (isPrime[p])
                {
                    int k = p * p;
                    for (int z = k; z <= limit; z += k) isPrime[z] = false;
                }
            }

            if (squareRoot > cutoff)
            {
                int i0 = cutoff + 5;

                for (int i = i0; i <= squareRoot; i += 6)
                {
                    if (isPrime[i])
                    {
                        long kk = 1L * i * i;
                        for (long z = kk; z <= limit; z += kk) isPrime[(int)z] = false;
                    }

                    int p = i + 2;
                    if (p <= limit && isPrime[p])
                    {
                        long kk = 1L * p * p;
                        if (kk <= limit)
                            for (long z = kk; z <= limit; z += kk) isPrime[(int)z] = false;
                    }
                }
            }

            limit -= 6;
            int l;
            for (l = 5; l <= limit; l += 6)
            {
                if (count < N && isPrime[l])
                    primes[count++] = l;
                if (count < N && isPrime[l + 2])
                    primes[count++] = l + 2;
            }
            limit += 6;
            if (count < N && isPrime[l])
                primes[count++] = l;
            l += 2;
            if (count < N && l <= limit && isPrime[l])
                primes[count++] = l;
            #endregion

            return primes;
        }

        public static int[] AtkinSieveExtendToNo(int N, Predicate<int> predicate)
        {
            if (predicate is null) return AtkinSieveExtendToNo(N);

            if (N <= 11)
            {
                var result = new List<int>(11);
                if (N > 0 && predicate(2))
                    result.Add(2);
                if (N > 1 && predicate(3))
                    result.Add(3);
                if (N > 2 && predicate(5))
                    result.Add(5);
                if (N > 3 && predicate(7))
                    result.Add(7);
                if (N > 4 && predicate(11))
                    result.Add(11);
                if (N > 5 && predicate(13))
                    result.Add(13);
                if (N > 6 && predicate(17))
                    result.Add(17);
                if (N > 7 && predicate(19))
                    result.Add(19);
                if (N > 8 && predicate(23))
                    result.Add(23);
                if (N > 9 && predicate(29))
                    result.Add(29);
                if (N > 10 && predicate(31))
                    result.Add(31);
                return result.ToArray();
            }

            int limit;

            var primes = new List<int>(N);

            if (predicate(2))
                primes.Add(2);
            if (predicate(3))
                primes.Add(3);

            int count = 2;

            double logN = Math.Log(N);
            double logLogN = Math.Log(logN);
            limit = (int)(N * (logN + logLogN - 1 + logLogN / logN)) + 5;

            var isPrime = new BitArray(limit + 1);
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
                    if ((uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                }
                n = xStepsize + 4;
                for (int j = 12; j <= y_limit; j += 72)
                {
                    n += j;
                    if ((uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
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
                    if ((uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                }
                if (i + 8 < temp)
                {
                    xStepsize += i + 8;
                    n = xStepsize + 1;
                    tempTwo = ((int)Math.Sqrt(limit - xStepsize) << 2) - 3;
                    for (int j = 0; j < tempTwo; j += 8)
                    {
                        n += j;
                        if ((uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
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
                        if ((uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                    }
                    n = xStepsize + 1;
                    for (int j = 24; j <= y_limit; j += 72)
                    {
                        n += j;
                        if ((uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
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

                int r0 = n % 12;
                byte m = _mask12[r0 * 12 + (s % 12)];

                int jlim = i << 2;
                int j = s;

                for (; j + 16 < jlim; j += 24)
                {
                    n -= j; if ((m & 0b001) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                    int j1 = j + 8; n -= j1; if ((m & 0b010) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                    int j2 = j + 16; n -= j2; if ((m & 0b100) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                }

                if (j < jlim)
                {
                    n -= j; if ((m & 0b001) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                    if (j + 8 < jlim)
                    {
                        int j1 = j + 8; n -= j1; if ((m & 0b010) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                    }
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

                int r0 = n % 12;
                byte m = _mask12[r0 * 12 + (s % 12)];

                int jlim = i << 2;
                int j = s;

                for (; j + 16 < jlim; j += 24)
                {
                    n -= j; if ((m & 0b001) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                    int j1 = j + 8; n -= j1; if ((m & 0b010) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                    int j2 = j + 16; n -= j2; if ((m & 0b100) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                }
                if (j < jlim)
                {
                    n -= j; if ((m & 0b001) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                    if (j + 8 < jlim)
                    {
                        int j1 = j + 8; n -= j1; if ((m & 0b010) != 0 && (uint)n <= (uint)limit) isPrime[n] = !isPrime[n];
                    }
                }
            }
            #endregion

            #region Eliminate Squares
            int cutoff = Math.Min(squareRoot, 46338);

            for (int i = 5; i <= cutoff; i += 6)
            {
                if (isPrime[i])
                {
                    int k = i * i;
                    for (int z = k; z <= limit; z += k) isPrime[z] = false;
                }

                int p = i + 2;
                if (isPrime[p])
                {
                    int k = p * p;
                    for (int z = k; z <= limit; z += k) isPrime[z] = false;
                }
            }

            if (squareRoot > cutoff)
            {
                int i0 = cutoff + 5;

                for (int i = i0; i <= squareRoot; i += 6)
                {
                    if (isPrime[i])
                    {
                        long kk = 1L * i * i;
                        for (long z = kk; z <= limit; z += kk) isPrime[(int)z] = false;
                    }

                    int p = i + 2;
                    if (p <= limit && isPrime[p])
                    {
                        long kk = 1L * p * p;
                        if (kk <= limit)
                            for (long z = kk; z <= limit; z += kk) isPrime[(int)z] = false;
                    }
                }
            }

            limit -= 6;
            int l1, l2;
            for (l1 = 5; l1 <= limit; l1 += 6)
            {
                if (count < N && isPrime[l1])
                {
                    count++;
                    if (predicate(l1))
                        primes.Add(l1);
                }
                l2 = l1 + 2;
                if (count < N && isPrime[l2])
                {
                    count++;
                    if (predicate(l2))
                        primes.Add(l2);
                }
            }
            limit += 6;
            if (count < N && isPrime[l1])
            {
                count++;
                if (predicate(l1))
                    primes.Add(l1);
            }
            l2 = l1 + 2;
            if (count < N && l2 <= limit && isPrime[l2])
            {
                count++;
                if (predicate(l2))
                    primes.Add(l2);
            }
            #endregion

            return primes.ToArray();
        }
    }
}
