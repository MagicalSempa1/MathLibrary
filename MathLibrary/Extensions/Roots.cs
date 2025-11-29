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

        public static BigInteger CeilingSqrt(this BigInteger n)
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
                if (k == 2) return n.FloorSqrt();
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

        public static BigInteger CeilingNroot(this BigInteger n, int k)
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

        private static bool[] rem64 = new bool[] { true, true, false, false, true, false, false, false, false, true, false, false, false, false, false, false, true, true, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, true, false, false, true, false, false, false, false, true, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, true, false, false, false, false, false, false };
        private static bool[] rem63 = new bool[] { true, true, false, false, true, false, false, true, false, true, false, false, false, false, false, false, true, false, true, false, false, false, true, false, false, true, false, false, true, false, false, false, false, false, false, false, true, true, false, false, false, false, false, true, false, false, true, false, false, true, false, false, false, false, false, false, false, false, true, false, false, false, false };
        private static bool[] rem65 = new bool[] { true, true, false, false, true, false, false, false, false, true, true, false, false, false, true, false, true, false, false, false, false, false, false, false, false, true, true, false, false, true, true, false, false, false, false, true, true, false, false, true, true, false, false, false, false, false, false, false, false, true, false, true, false, false, false, true, true, false, false, false, false, true, false, false, true };
        private static bool[] rem11 = new bool[] { true, true, false, true, true, true, false, false, false, true, false };

        public static bool IsSqrt(this BigInteger n)
        {
            var t = (int)(n % 2882880);
            if (!rem64[t & 63])
                return false;
            if (!rem63[t % 63])
                return false;
            if (!rem65[t % 65])
                return false;
            if (!rem11[t % 11])
                return false;
            var sqrt = n.FloorSqrt();
            return sqrt * sqrt == n;
        }



        public static bool IsSqrt(this in BigInteger n, ref BigInteger root)
        {
            var t = (int)(n % 2882880);
            if (!rem64[t & 63])
                return false;
            if (!rem63[t % 63])
                return false;
            if (!rem65[t % 65])
                return false;
            if (!rem11[t % 11])
                return false;
            root = n.FloorSqrt();
            return root * root == n;
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
            root = (long)Math.Sqrt(n);
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
    }
}
