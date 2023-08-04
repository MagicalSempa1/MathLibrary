using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary
{
    public static partial class ArithmeticFunctions
    {
        public static int JacobiSymbol(BigInteger a, BigInteger n)
        {
            if (BigInteger.GreatestCommonDivisor(a, n) != 1)
                return 0;
            (int e, int a, int b, int d) S = (0, (int)(a % 4), (int)(n % 4), 1);

            static (int e, int a, int b, int d) Jupdate((int e, int a, int b, int d) Sl, int d, int m)
            {
                if (Sl.d != d && Sl.a % 2 == 1 && Sl.b % 2 == 1)
                    Sl.e += (Sl.a - 1) * (Sl.b - 1) >> 2;
                Sl.d = d;
                if (d == 1)
                {
                    if (Sl.b == 2)
                        Sl.e += ((m * (Sl.a - 1)) >> 1) + ((m * (m - 1)) >> 1);
                    Sl.a -= m * Sl.b;
                }
                else
                {
                    if (Sl.a == 2)
                        Sl.e += ((m * (Sl.b - 1)) >> 1) + ((m * (m - 1)) >> 1);
                    Sl.b -= m * Sl.a;
                }
                Sl.e %= 2;
                return Sl;
            }
            while (true)
            {
                if (a > n)
                {
                    BigInteger m = a / n;
                    a %= n;
                    S = Jupdate(S, 1, (int)(m % 4));
                    if (a == 0)
                        return S.e % 2 == 0 ? 1 : -1;
                }
                else
                {
                    BigInteger m = n / a;
                    n %= a;
                    S = Jupdate(S, 0, (int)(m % 4));
                    if (n == 0)
                        return S.e % 2 == 0 ? 1 : -1;
                }
            }
        }

        public static int WikiJacobiSymbol(BigInteger a, BigInteger n)
        {
            if (BigInteger.GreatestCommonDivisor(a, n) != 1)
                return 0;
            int r = 1;
            if (a < 0)
            {
                a = -a;
                if (n % 4 == 3)
                    r = -r;
            }
            do
            {
                BigInteger t = 0;
                while (a % 2 == 0)
                {
                    t += 1;
                    a >>= 1;
                }
                if (t % 2 == 1)
                {
                    if (n % 8 == 3 || n % 8 == 5)
                        r = -r;
                }
                if (a % 4 == n % 4 && a % 4 == 3)
                    r = -r;
                BigInteger c = a;
                a = n % c;
                n = c;
            }
            while (a != 0);
            return r;
        }

        //public static int BYJacobiSymbol(BigInteger a, BigInteger b, int batch = 32)
        //{
        //    (int u, BigInteger delta, (BigInteger ai, BigInteger bi, BigInteger ci, BigInteger di)) BatchMatrix(BigInteger x, BigInteger y, BigInteger delta, int batch)
        //    {
        //        BigInteger ai = 1, bi = 0, ci = 0, di = 1, u = 0;

        //        for (int i = 0; i < batch; i++)
        //        {
        //            BigInteger yi = y;

        //            if (delta >= 0 && !x.IsEven)
        //            {
        //                (delta, x, y) = (-delta, (x - y) >> 1, x);
        //                (ai, bi, ci, di) = (ai - ci, bi - di, ai << 1, bi << 1);
        //            }
        //            else if (!x.IsEven)
        //            {
        //                (delta, x) = (1 + delta, (x + y) >> 1);
        //                (ai, bi, ci, di) = (ai + ci, bi + di, ci << 1, di << 1);
        //            }
        //            else
        //            {
        //                (delta, x) = (1 + delta, x >> 1);
        //                (ci, di) = (ci << 1, di << 1);
        //            }

        //            u += ((yi & y) ^ (y >> 1)) & 2;
        //            u += (u.IsEven ? 0 : 1) ^ (ci < 0 ? 1 : 0);
        //            u %= 4;
        //        }

        //        return ((int)u, delta, (ai, bi, ci, di));
        //    }
        //    a %= b;
        //    BigInteger delta = 0;
        //    int t = 0;
        //    long nbits = b.GetBitLength();
        //    long niters = (45907 * nbits + 26313) / 19929;
        //    BigInteger mask = (BigInteger.One << (batch + 2)) - 1;

        //    for (int i = 0; i < niters; i += batch)
        //    {
        //        (int u, BigInteger newDelta, (BigInteger ai, BigInteger bi, BigInteger ci, BigInteger di)) = BatchMatrix(a & mask, b & mask, delta, batch);
        //        (a, b) = ((ai * a + bi * b) >> batch, (ci * a + di * b) >> batch);
        //        t = (t + u) % 4;
        //        t = (t + ((t & 1) ^ (b < 0 ? 1 : 0))) % 4;
        //        delta = newDelta;
        //    }

        //    t = (t + (t & 1)) % 4;

        //    if (b == -1 || b == 1)
        //        return 1 - t;
        //    else
        //        return 0;
        //}


        public static int BYJacobiSymbol(BigInteger a, BigInteger b, int batch = 32)
        {
            (int u, BigInteger delta, (BigInteger ai, BigInteger bi, BigInteger ci, BigInteger di)) BatchMatrix(BigInteger x, BigInteger y, BigInteger delta, int batch)
            {
                BigInteger ai = 1, bi = 0, ci = 0, di = 1, u = 0;

                for (int i = 0; i < batch; i++)
                {
                    BigInteger yi = y;

                    if (delta >= 0 && !x.IsEven)
                    {
                        (delta, x, y) = (-delta, (x - y) >> 1, x);
                        (ai, bi, ci, di) = (ai - ci, bi - di, ai << 1, bi << 1);
                    }
                    else if (!x.IsEven)
                    {
                        (delta, x) = (1 + delta, (x + y) >> 1);
                        (ai, bi, ci, di) = (ai + ci, bi + di, ci << 1, di << 1);
                    }
                    else
                    {
                        (delta, x) = (1 + delta, x >> 1);
                        (ci, di) = (ci << 1, di << 1);
                    }

                    u += ((yi & y) ^ (y >> 1)) & 2;
                    u += (u.IsEven ? 0 : 1) ^ (ci < 0 ? 1 : 0);
                    u %= 4;
                }

                return ((int)u, delta, (ai, bi, ci, di));
            }
            a %= b;
            BigInteger delta = 0;
            int t = 0;
            BigInteger mask = (BigInteger.One << (batch + 2)) - 1;

            do
            {
                (int u, BigInteger newDelta, (BigInteger ai, BigInteger bi, BigInteger ci, BigInteger di)) = BatchMatrix(a & mask, b & mask, delta, batch);
                (a, b) = ((ai * a + bi * b) >> batch, (ci * a + di * b) >> batch);
                t = (t + u) % 4;
                t = (t + ((t & 1) ^ (b < 0 ? 1 : 0))) % 4;
                delta = newDelta;
            } while (a != 0);

            t = (t + (t & 1)) % 4;

            if (b == -1 || b == 1)
                return 1 - t;
            else
                return 0;
        }

        public static (BigInteger q, BigInteger r) BinaryDividePos(BigInteger a, BigInteger b)
        {
            int j = 0;
            BigInteger v = b;
            while (v.IsEven)
            {
                j++;
                v >>= 1;
            }

            BigInteger denominator = BigInteger.One << (j + 1);
            BigInteger q = BigInteger.Divide(-a, v);
            q %= denominator;

            // Ensure q is odd and positive
            if (q <= 0)
                q += denominator;

            BigInteger r = a + q * v;
            return (q, r);
        }

        public static (int, int, (int e, int a, int b, int d)) HalfBinaryGCD(BigInteger a, BigInteger b, int k)
        {
            var N = Math.Min(a.GetBitLength(), a.GetBitLength());
            var S = (N >> 1) + 1;
            throw new NotImplementedException();
        }
    }
}
