using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary
{
    public static class Extensions
    {
        public static UInt128 FloorSqrt(this UInt128 n)
        {
            if (n == 0)
                return 0;
            if (n > 0)
            {
                if (n < uint.MaxValue)
                    return (UInt128)Math.Sqrt((uint)n);
                UInt128 x = n >> 2;
                UInt128 x1 = n;
                while (x != x1)
                {
                    x1 = x;
                    x = (x + n / x) >> 1;
                }
                return x;
            }
            else
                throw new Exception("square root of negative number");
        }

        public static BigInteger FloorSqrt(this BigInteger n)
        {
            if (n < 144838757784765629)
            {
                uint vInt = (uint)Math.Sqrt((ulong)n);
                if ((n >= 4503599761588224) && ((ulong)vInt * vInt > (ulong)n))
                    vInt--;
                return vInt;
            }
            double xAsDub = (double)n;
            if (xAsDub < 8.5e37)
            {
                ulong vInt = (ulong)Math.Sqrt(xAsDub);
                BigInteger v = (vInt + ((ulong)(n / vInt))) >> 1;
                return (v * v <= n) ? v : v - 1;
            }
            if (xAsDub < 4.3322e127)
            {
                BigInteger v = (BigInteger)Math.Sqrt(xAsDub);
                v = (v + (n / v)) >> 1;
                if (xAsDub > 2e63)
                    v = (v + (n / v)) >> 1;
                return (v * v <= n) ? v : v - 1;
            }
            int xLen = (int)n.GetBitLength();
            int wantedPrecision = (xLen + 1) >> 1;
            int xLenMod = xLen + (xLen & 1) + 1;
            long tempX = (long)(n >> (xLenMod - 63));
            double tempSqrt1 = Math.Sqrt(tempX);
            ulong valLong = (ulong)BitConverter.DoubleToInt64Bits(tempSqrt1) & 0x1fffffffffffffL;
            if (valLong == 0)
                valLong = 1UL << 53;
            BigInteger val = ((BigInteger)valLong << 52) + (n >> xLenMod - (3 * 53)) / valLong;
            int size = 106;
            for (; size < 256; size <<= 1)
                val = (val << (size - 1)) + (n >> xLenMod - (3 * size)) / val;
            if (xAsDub > 4e254)
            {
                int numOfNewtonSteps = BitOperations.Log2((uint)(wantedPrecision / size)) + 2;
                int wantedSize = (wantedPrecision >> numOfNewtonSteps) + 2;
                int needToShiftBy = size - wantedSize;
                val >>= needToShiftBy;
                size = wantedSize;
                do
                {
                    int shiftX = xLenMod - (3 * size);
                    BigInteger valSqrd = (val * val) << (size - 1);
                    BigInteger valSU = (n >> shiftX) - valSqrd;
                    val = (val << size) + (valSU / val);
                    size <<= 1;
                } while (size < wantedPrecision);
            }
            int oversidedBy = size - wantedPrecision;
            BigInteger saveDroppedDigitsBI = val & ((BigInteger.One << oversidedBy) - 1);
            int downby = (oversidedBy < 64) ? (oversidedBy >> 2) + 1 : (oversidedBy - 32);
            ulong saveDroppedDigits = (ulong)(saveDroppedDigitsBI >> downby);
            val >>= oversidedBy;
            if ((saveDroppedDigits == 0) && (val * val > n))
                val--;
            return val;
        }


        public static BigInteger RoundSqrt(this BigInteger n)
        {
            if (n == 0)
                return 0;
            if (n > 0)
            {
                var root = n.FloorSqrt();
                return BigInteger.Abs(root * root - n) < BigInteger.Abs(BigInteger.Pow(root + 1, 2) - n) ? root : root + 1;
            }
            else
                throw new Exception("square root of negative number");
        }

        public static BigInteger CeillingSqrt(this BigInteger n)
        {
            if (n == 0)
                return 0;
            if (n > 0)
            {
                var root = n.FloorSqrt();
                return root * root == n ? root : root + 1;
            }
            else
                throw new Exception("square root of negative number");
        }

        public static BigInteger FloorNroot(this BigInteger n, int k)
        {
            if (n == 0)
                return 0;
            if (n > 0)
            {
                if (n < uint.MaxValue)
                    return (BigInteger)Math.Pow((uint)n, 1.0 / k);
                BigInteger x = BigInteger.One << (int)Math.Ceiling(n.GetBitLength() / (double)k);
                BigInteger x1 = n;
                while (x < x1)
                {
                    x1 = x;
                    x = ((k - 1) * x + n / BigInteger.Pow(x, k - 1)) / k;
                }
                return x1;
            }
            else
                throw new Exception("input number is negative");
        }

        public static BigInteger CeillingNroot(this BigInteger n, int k)
        {
            if (n == 0)
                return 0;
            if (n > 0)
            {
                if (n < uint.MaxValue)
                    return (BigInteger)Math.Ceiling(Math.Pow((uint)n, 1.0 / k));
                BigInteger x = BigInteger.One << (int)Math.Ceiling(n.GetBitLength() / (double)k);
                BigInteger x1 = n;
                while (x < x1)
                {
                    x1 = x;
                    x = ((k - 1) * x + n / BigInteger.Pow(x, k - 1)) / k;
                }
                return BigInteger.Pow(x1, k) == n ? x1 : x1 + 1;
            }
            else
                throw new Exception("square root of negative number");
        }

        public static bool IsSqrt(this long n)
        {
            var r = n % 64;
            if (r != 0 && r != 1 && r != 4 && r != 9 && r != 16 && r != 25 && r != 36 && r != 49
                && r != 17 && r != 33 && r != 41 && r != 57)
                return false;
            var t = n % 45045;
            r = t % 63;
            if (r != 0 && r != 1 && r != 4 && r != 7 && r != 9 && r != 16 && r != 18 && r != 22 && r != 25 && r != 28 && r != 36 && r != 37 && r != 43 && r != 46 && r != 49 && r != 58)
                return false;
            r = t % 65;
            if (r != 0 && r != 1 && r != 4 && r != 9 && r != 10 && r != 14 && r != 16 && r != 25 && r != 26 && r != 29 && r != 30 && r != 35 && r != 36 && r != 39 && r != 40 && r != 49 && r != 51 && r != 55 && r != 56 && r != 61 && r != 64)
                return false;
            r = t % 11;
            if (r == 2 || r == 6 || r == 7 || r == 8 || r == 10)
                return false;
            return (long)Math.Pow((long)Math.Sqrt(n), 2) == n;
        }

        public static bool IsSqrt(this BigInteger n)
        {
            var r = n % 64;
            if (r != 0 && r != 1 && r != 4 && r != 9 && r != 16 && r != 25 && r != 36 && r != 49
                && r != 17 && r != 33 && r != 41 && r != 57)
                return false;
            var t = n % 45045;
            r = t % 63;
            if (r != 0 && r != 1 && r != 4 && r != 7 && r != 9 && r != 16 && r != 18 && r != 22 && r != 25 && r != 28 && r != 36 && r != 37 && r != 43 && r != 46 && r != 49 && r != 58)
                return false;
            r = t % 65;
            if (r != 0 && r != 1 && r != 4 && r != 9 && r != 10 && r != 14 && r != 16 && r != 25 && r != 26 && r != 29 && r != 30 && r != 35 && r != 36 && r != 39 && r != 40 && r != 49 && r != 51 && r != 55 && r != 56 && r != 61 && r != 64)
                return false;
            r = t % 11;
            if (r == 2 || r == 6 || r == 7 || r == 8 || r == 10)
                return false;
            return BigInteger.Pow(n.FloorSqrt(), 2) == n;
        }

        public static bool IsSqrt(this BigInteger n, ref BigInteger root)
        {
            var r = (int)(n % 64);
            if (r != 0 && r != 1 && r != 4 && r != 9 && r != 16 && r != 25 && r != 36 && r != 49
                && r != 17 && r != 33 && r != 41 && r != 49 && r != 57)
                return false;
            var t = (int)(n % 45045);
            r = t % 63;
            if (r != 0 && r != 1 && r != 4 && r != 7 && r != 9 && r != 16 && r != 18 && r != 22 && r != 25 && r != 28 && r != 36 && r != 37 && r != 43 && r != 46 && r != 49 && r != 58)
                return false;
            r = t % 65;
            if (r != 0 && r != 1 && r != 4 && r != 9 && r != 10 && r != 14 && r != 16 && r != 25 && r != 26 && r != 29 && r != 30 && r != 35 && r != 36 && r != 39 && r != 40 && r != 49 && r != 51 && r != 55 && r != 56 && r != 61 && r != 64)
                return false;
            r = t % 11;
            if (r == 2 || r == 6 || r == 7 || r == 8 || r == 10)
                return false;
            root = n.FloorSqrt();
            return BigInteger.Pow(root, 2) == n;
        }

        public static bool IsSqrt(this long n, ref long root)
        {
            var r = n % 64;
            if (r != 0 && r != 1 && r != 4 && r != 9 && r != 16 && r != 25 && r != 36 && r != 49
                && r != 17 && r != 33 && r != 41 && r != 57)
                return false;
            var t = n % 45045;
            r = t % 63;
            if (r != 0 && r != 1 && r != 4 && r != 7 && r != 9 && r != 16 && r != 18 && r != 22 && r != 25 && r != 28 && r != 36 && r != 37 && r != 43 && r != 46 && r != 49 && r != 58)
                return false;
            r = t % 65;
            if (r != 0 && r != 1 && r != 4 && r != 9 && r != 10 && r != 14 && r != 16 && r != 25 && r != 26 && r != 29 && r != 30 && r != 35 && r != 36 && r != 39 && r != 40 && r != 49 && r != 51 && r != 55 && r != 56 && r != 61 && r != 64)
                return false;
            r = t % 11;
            if (r == 2 || r == 6 || r == 7 || r == 8 || r == 10)
                return false;
            root = (long)Math.Round(Math.Sqrt(n));
            return root * root == n;
        }

        public static bool IsSqrt(this ulong n, ref ulong root)
        {
            var r = n % 64;
            if (r != 0 && r != 1 && r != 4 && r != 9 && r != 16 && r != 25 && r != 36 && r != 49
                && r != 17 && r != 33 && r != 41 && r != 57)
                return false;
            var t = n % 45045;
            r = t % 63;
            if (r != 0 && r != 1 && r != 4 && r != 7 && r != 9 && r != 16 && r != 18 && r != 22 && r != 25 && r != 28 && r != 36 && r != 37 && r != 43 && r != 46 && r != 49 && r != 58)
                return false;
            r = t % 65;
            if (r != 0 && r != 1 && r != 4 && r != 9 && r != 10 && r != 14 && r != 16 && r != 25 && r != 26 && r != 29 && r != 30 && r != 35 && r != 36 && r != 39 && r != 40 && r != 49 && r != 51 && r != 55 && r != 56 && r != 61 && r != 64)
                return false;
            r = t % 11;
            if (r == 2 || r == 6 || r == 7 || r == 8 || r == 10)
                return false;
            root = (ulong)Math.Round(Math.Sqrt(n));
            return root * root == n;
        }

        public static bool IsSqrt(this UInt128 n, ref UInt128 root)
        {
            var r = n % 64;
            if (r != 0 && r != 1 && r != 4 && r != 9 && r != 16 && r != 25 && r != 36 && r != 49
                && r != 17 && r != 33 && r != 41 && r != 57)
                return false;
            var t = n % 45045;
            r = t % 63;
            if (r != 0 && r != 1 && r != 4 && r != 7 && r != 9 && r != 16 && r != 18 && r != 22 && r != 25 && r != 28 && r != 36 && r != 37 && r != 43 && r != 46 && r != 49 && r != 58)
                return false;
            r = t % 65;
            if (r != 0 && r != 1 && r != 4 && r != 9 && r != 10 && r != 14 && r != 16 && r != 25 && r != 26 && r != 29 && r != 30 && r != 35 && r != 36 && r != 39 && r != 40 && r != 49 && r != 51 && r != 55 && r != 56 && r != 61 && r != 64)
                return false;
            r = t % 11;
            if (r == 2 || r == 6 || r == 7 || r == 8 || r == 10)
                return false;
            root = n.FloorSqrt();
            return root * root == n;
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

        public static bool IsPowerOfNumber(this BigInteger n)
        {
            if (n == 1)
                return true;
            if (n.IsPowerOfTwo)
                return n != 2;
            var lgn = Math.Ceiling(BigInteger.Log(n, 2));
            for (int b = 2; b <= lgn; b++)
            {
                int lowa = 1;
                var higha = 1 << (int)(lgn / b + 1);
                while (lowa < higha - 1)
                {
                    var mida = (lowa + higha) >> 1;
                    var ab = BigInteger.Pow(mida, b);
                    if (ab > n)
                        higha = mida;
                    else if (ab < n)
                        lowa = mida;
                    else
                        return true;
                }
            }
            return false;
        }

        public static double ForwardFiniteDifference(this Func<double, double> function, int n, int k, double x_0, double h, int order)
        {
            if (k < 0 & k > n - order)
                throw new Exception();
            if (order == 0)
                return function(x_0);
            if (order == 1)
                return function(x_0 + (k + 1) * h) - function(x_0 + k * h);
            if (n - order >= 0)
                return ForwardFiniteDifference(function, n, k + 1, x_0, h, order - 1) - ForwardFiniteDifference(function, n, k, x_0, h, order - 1);
            throw new Exception();
        }
    }
}
