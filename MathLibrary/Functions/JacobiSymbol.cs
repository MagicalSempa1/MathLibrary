using System.Numerics;

namespace MathLibrary.Functions
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


        public static int BYJacobiSymbol(BigInteger a, BigInteger b, int batch = 8)
        {
            a %= b;
            if (a.IsZero) return b.IsOne ? 1 : 0;
            if (a.IsOne) return 1;
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

        private static (int u, BigInteger delta, (BigInteger ai, BigInteger bi, BigInteger ci, BigInteger di)) BatchMatrix(BigInteger x, BigInteger y, BigInteger delta, int batch)
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

        #region Moller

        public static int MollerJacobiSymbol(BigInteger a, BigInteger b)
        {
            if (b <= 0 || (b & 1) == 0) throw new ArgumentException("Jacobi is defined for odd positive b");
            if (b == 1) return 1;
            a %= b;
            if (a < 0)
                a += b;
            if (a == 0) return 0;
            if (a == 1) return 1;

            bool e = false;
            int d = 1;
            int alpha = (int)(a & 3);
            int beta = (int)(a & 3);

            while (true)
            {
                if (a >= b)
                {
                    var m = BigInteger.DivRem(a, b, out a);
                    int m4 = (int)(m & 3);
                    int d1 = 1;

                    e ^= (d != d1) && (alpha == 3) && (beta == 3);

                    d = d1;

                    if (beta == 2)
                        e ^= ((m4 & 1) != 0 && alpha == 3) ^ ((m4 & 2) != 0);
                    alpha -= m4 * beta;

                    if (a == 0)
                        return b == 1 ? (e ? -1 : 1) : 0;
                    if (a == 1)
                        return e ? -1 : 1;
                }
                else
                {
                    var m = BigInteger.DivRem(b, a, out b);
                    int m4 = (int)(m & 3);
                    int d1 = 0;

                    e ^= (d != d1) && (alpha == 3) && (beta == 3);

                    d = d1;

                    if (alpha == 2)
                        e ^= ((m4 & 1) != 0 && beta == 3) ^ ((m4 & 2) != 0);
                    beta -= m4 * alpha;

                    if (b == 0)
                        return a == 1 ? (e ? -1 : 1) : 0;
                    if (b == 1)
                        return e ? -1 : 1;
                }

                alpha &= 3;
                beta &= 3;
            }
        }

        public static int MollerJacobiSymbol(BigInteger a, int b)
        {
            if (b <= 0 || (b & 1) == 0) throw new ArgumentException("Jacobi is defined for odd positive b");
            if (b == 1) return 1;
            a %= b;
            int aInt = (int)a;
            if (aInt < 0)
                aInt += b;
            if (aInt == 0) return 0;
            if (aInt == 1) return 1;

            bool e = false;
            int d = 1;
            int alpha = aInt & 3;
            int beta = b & 3;

            while (true)
            {
                if (aInt >= b)
                {
                    int m = Math.DivRem(aInt, b, out aInt);
                    int m4 = m & 3;
                    int d1 = 1;

                    e ^= (d != d1) && (alpha == 3) && (beta == 3);

                    d = d1;

                    if (beta == 2)
                        e ^= ((m4 & 1) != 0 && alpha == 3) ^ ((m4 & 2) != 0);
                    alpha -= m4 * beta;

                    if (aInt == 0)
                        return b == 1 ? (e ? -1 : 1) : 0;
                    if (aInt == 1)
                        return e ? -1 : 1;
                }
                else
                {
                    int m = Math.DivRem(b, aInt, out b);
                    int m4 = m & 3;
                    int d1 = 0;

                    e ^= (d != d1) && (alpha == 3) && (beta == 3);

                    d = d1;

                    if (alpha == 2)
                        e ^= ((m4 & 1) != 0 && beta == 3) ^ ((m4 & 2) != 0);
                    beta -= m4 * alpha;

                    if (b == 0)
                        return aInt == 1 ? (e ? -1 : 1) : 0;
                    if (b == 1)
                        return e ? -1 : 1;
                }

                alpha &= 3;
                beta &= 3;
            }
        }

        public static int MollerJacobiSymbol(int a, int b)
        {
            if (b <= 0 || (b & 1) == 0) throw new ArgumentException("Jacobi is defined for odd positive b");
            if (b == 1) return 1;
            a %= b;

            if (a < 0)
                a += b;
            if (a == 0) return 0;
            if (a == 1) return 1;

            bool e = false;
            int d = 1;
            int alpha = a & 3;
            int beta = b & 3;

            while (true)
            {
                if (a >= b)
                {
                    int m = Math.DivRem(a, b, out a);
                    int m4 = m & 3;
                    int d1 = 1;

                    e ^= (d != d1) && (alpha == 3) && (beta == 3);

                    d = d1;


                    if (beta == 2)
                        e ^= ((m4 & 1) != 0 && alpha == 3) ^ ((m4 & 2) != 0);
                    alpha -= m4 * beta;

                    if (a == 0)
                        return b == 1 ? (e ? -1 : 1) : 0;
                    if (a == 1)
                        return e ? -1 : 1;
                }
                else
                {
                    int m = Math.DivRem(b, a, out b);
                    int m4 = m & 3;
                    int d1 = 0;

                    e ^= (d != d1) && (alpha == 3) && (beta == 3);

                    d = d1;

                    if (alpha == 2)
                        e ^= ((m4 & 1) != 0 && beta == 3) ^ ((m4 & 2) != 0);
                    beta -= m4 * alpha;

                    if (b == 0)
                        return a == 1 ? (e ? -1 : 1) : 0;
                    if (b == 1)
                        return e ? -1 : 1;
                }

                alpha &= 3;
                beta &= 3;
            }
        }

        private static void JUpdate(ref int e, ref int d, int d0, int m4, int alpha, int beta)
        {
            if (d != d0 && (alpha & 1) == 1 && (beta & 1) == 1)
            {
                e ^= (((alpha - 1) * (beta - 1)) >> 2) & 1;
                d = d0;
            }

            if (d == 1)
            {
                if (beta == 2) e ^= (m4 & 1 & ((alpha - 1) >> 1) & 1) ^ ((m4 >> 1) & 1);
            }
            else if (alpha == 2) e ^= (m4 & 1 & ((beta - 1) >> 1) & 1) ^ ((m4 >> 1) & 1);
        }

        #endregion
    }
}
