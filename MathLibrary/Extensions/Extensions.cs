using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Extensions
{
    public static partial class Extensions
    {
        public static int CountTrue(this BitArray bits)
        {
            ArgumentNullException.ThrowIfNull(bits);
            int len = (bits.Length + 31) / 32;
            var pool = ArrayPool<int>.Shared;
            int[] buf = pool.Rent(len);

            try
            {
                Array.Clear(buf, 0, len);
                bits.CopyTo(buf, 0);

                int count = 0;

                for (int i = 0; i < len - 1; i++)
                    count += BitOperations.PopCount((uint)buf[i]);

                if (len > 0)
                {
                    int rem = bits.Length & 31;
                    uint last = (uint)buf[len - 1];
                    if (rem != 0) last &= (1u << rem) - 1;
                    count += BitOperations.PopCount(last);
                }

                return count;
            }
            finally
            {
                pool.Return(buf, clearArray: true);
            }
        }

        public static bool WithoutSquares(this BigInteger n)
        {
            for (BigInteger i = 4, k = 5; i <= n; i += k, k += 2)
            {
                //(n + 1) ^ 2 = n ^ 2 + 2n + 1
                // (6k + 1) ^ 2 = (6k) ^ 2 + 12k + 1
                if (n % i == 0)
                    return false;
            }
            return true;
        }

        public static int DecimalDigits(this BigInteger n) => n.ToString().Length;

        public static double ForwardFiniteDifference(this Func<double, double> function, int n, int k, double x0, double h, int order)
        {
            if (k < 0 & k > n - order)
                throw new Exception();
            if (order == 0)
                return function(x0);
            if (order == 1)
                return function(x0 + (k + 1) * h) - function(x0 + k * h);
            if (n - order >= 0)
                return ForwardFiniteDifference(function, n, k + 1, x0, h, order - 1) - ForwardFiniteDifference(function, n, k, x0, h, order - 1);
            throw new Exception();
        }
    }
}
