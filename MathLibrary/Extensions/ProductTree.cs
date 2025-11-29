using System.Buffers;
using System.Numerics;

namespace MathLibrary.Extensions
{
    public static partial class Extensions
    {
        public static BigInteger Product(this int[] source)
        {
            ArgumentNullException.ThrowIfNull(source);
            int n = source.Length;
            if (n == 0) return BigInteger.One;

            var poolUL = ArrayPool<ulong>.Shared;
            ulong[] ucur = poolUL.Rent(n);
            ulong[] unext = poolUL.Rent((n + 1) >> 1);

            for (int i = 0; i < n; i++) ucur[i] = (ulong)source[i];

            int ulen = n;
            bool overflow = false;

            while (ulen > 1 && !overflow)
            {
                int nextLen = (ulen + 1) >> 1;

                int i = 0, k = 0;
                for (; i + 1 < ulen; i += 2, k++)
                {
                    ulong a = ucur[i], b = ucur[i + 1];
                    if (a != 0 && b > ulong.MaxValue / a) { overflow = true; break; }
                    unext[k] = a * b;
                }

                if (overflow) break;

                if ((ulen & 1) == 1)
                    unext[nextLen - 1] = ucur[ulen - 1];

                (ucur, unext) = (unext, ucur);
                ulen = nextLen;
            }

            if (!overflow && ulen == 1)
            {
                ulong val = ucur[0];
                poolUL.Return(ucur, clearArray: true);
                poolUL.Return(unext, clearArray: true);
                return new BigInteger(val);
            }

            var poolBI = ArrayPool<BigInteger>.Shared;
            BigInteger[] bcur = poolBI.Rent(ulen);
            BigInteger[] bnext = poolBI.Rent((ulen + 1) >> 1);

            try
            {
                int nextLen = (ulen + 1) >> 1;
                int i = 0, k = 0;

                for (; i + 1 < ulen; i += 2, k++)
                {
                    ulong a = ucur[i], b = ucur[i + 1];
                    if (a != 0 && b <= ulong.MaxValue / a)
                        bcur[k] = new BigInteger(a * b);
                    else
                        bcur[k] = new BigInteger(a) * b;
                }
                if ((ulen & 1) == 1)
                    bcur[nextLen - 1] = new BigInteger(ucur[ulen - 1]);

                poolUL.Return(ucur, clearArray: true);
                poolUL.Return(unext, clearArray: true);

                int blen = nextLen;
                //const int PAR_THRESHOLD = 1 << 5;

                while (blen > 1)
                {
                    int nn = (blen + 1) >> 1;
                    int pairs = blen >> 1;

                    //if (blen >= PAR_THRESHOLD)
                    //{
                    //    Parallel.For(0, pairs, t =>
                    //    {
                    //        int j = t << 1;
                    //        bnext[t] = bcur[j] * bcur[j + 1];
                    //    });
                    //}
                    //else
                    //{
                    for (int t = 0, j = 0; t < pairs; t++, j += 2)
                        bnext[t] = bcur[j] * bcur[j + 1];
                    //}

                    if ((blen & 1) == 1)
                        bnext[nn - 1] = bcur[blen - 1];

                    (bcur, bnext) = (bnext, bcur);
                    blen = nn;
                }

                return bcur[0];
            }
            finally
            {
                poolBI.Return(bcur, clearArray: true);
                poolBI.Return(bnext, clearArray: true);
            }
        }

    }
}
