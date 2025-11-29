using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Extensions
{
    public static partial class Extensions
    {
        public static bool IsPerfectPower(this BigInteger n, out BigInteger a, out int b)
        {
            if (n == 0) { a = 0; b = 1; return true; }
            if (n == 1) { a = 1; b = 1; return true; }
            if (n == -1) { a = -1; b = 1; return true; }

            var absN = BigInteger.Abs(n);
            int maxB = (int)absN.GetBitLength();

            if (n.Sign > 0)
            {
                for (int p = maxB; p >= 2; --p)
                {
                    BigInteger r = absN.FloorNroot(p);
                    if (BigInteger.Pow(r, p) == absN)
                    {
                        a = r; b = p;
                        return true;
                    }
                }
            }
            else
            {
                int pStart = (maxB % 2 == 0) ? maxB - 1 : maxB;
                for (int p = pStart; p >= 3; p -= 2)
                {
                    BigInteger r = absN.FloorNroot(p);
                    if (BigInteger.Pow(r, p) == absN)
                    {
                        a = -r; b = p;
                        return true;
                    }
                }
            }

            a = 0; b = 0;
            return false;
        }
    }
}
