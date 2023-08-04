using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary
{
    public static partial class ArithmeticFunctions
    {
        public static int SimplePi(int limit)
        {
            if (limit < 2)
                return 0;
            if (limit < 3)
                return 1;
            if (limit < 5)
                return 2;
            if (limit < 7)
                return 3;
            int result = 4;
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
            for (int i = 11; i <= limit; i += 6)
            {
                if (isPrime[i])
                    result++;
                if (isPrime[i + 2])
                    result++;
            }
            #endregion

            return result;
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
