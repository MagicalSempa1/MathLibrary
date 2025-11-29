using MathLibrary.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Functions
{
    public static partial class ArithmeticFunctions
    {
        private static readonly byte[] _mask12 = [0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 0, 0, 0, 0, 0, 2, 0, 0, 0, 4, 0, 1, 0, 4, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 2, 5, 0, 0, 0, 4, 2, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 2, 0, 1, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 4, 0, 1, 0, 4, 0, 2, 0, 0, 0, 0, 0, 2, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 5, 0, 2, 0, 4, 0, 0, 0, 6, 0, 0, 0];

        public static int SimplePi(int limit)
        {
            if (limit < 2) return 0;
            if (limit == 2) return 1;
            if (limit < 5) return 2;

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

            int result = 0;

            if (limit >= 2)
                result++;
            if (limit >= 3)
                result++;

            limit -= 6;
            int l;
            for (l = 5; l <= limit; l += 6)
            {
                if (isPrime[l])
                    result++;
                if (isPrime[l + 2])
                    result++;
            }
            limit += 6;
            if (isPrime[l])
                result++;
            l += 2;
            if (l <= limit && isPrime[l])
                result++;
            #endregion

            return result;
        }

        public static int MySimplePi(int limit)
        {
            if (limit < 2) return 0;
            if (limit == 2) return 1;
            if (limit < 5) return 2;

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
            #endregion

            return isPrime.CountTrue() + 2;
        }


        public static int LegendrePi(int n)
        {
            if (n < 2) return 0;
            if (n < 3) return 1;
            if (n < 5) return 2;
            if (n < 7) return 3;

            var sqrt = (int)Math.Sqrt(n);
            return 1;
        }
    }
}
