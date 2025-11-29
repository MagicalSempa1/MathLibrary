using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace MathLibrary.LinearAlgebraZ2
{
    internal static class MatrixZ2Calculator
    {
        private const int step = 4;

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        internal static void XorFromBit(Span<ulong> t, ReadOnlySpan<ulong> s, int w, int bit)
        {
            Debug.Assert(t.Length == s.Length);
            int len = t.Length;

            int k;
            if (bit == 0)
            {
                k = w;
            }
            else
            {
                ulong headMask = ~((1UL << bit) - 1);
                t[w] ^= s[w] & headMask;
                k = w + 1;
                if (k >= len) return;
            }

            if (Avx2.IsSupported)
            {
                ref ulong tRef = ref MemoryMarshal.GetReference(t);
                ref ulong sRef = ref MemoryMarshal.GetReference(s);

                const int step = 4;
                int remaining = len - k;
                int i = 0;

                int unrolled = remaining & ~((step << 1) - 1);
                for (; i < unrolled; i += step * 2)
                {
                    var vt0 = Vector256.LoadUnsafe(ref tRef, (nuint)(k + i));
                    var vs0 = Vector256.LoadUnsafe(ref sRef, (nuint)(k + i));
                    Avx2.Xor(vt0, vs0).StoreUnsafe(ref tRef, (nuint)(k + i));

                    var vt1 = Vector256.LoadUnsafe(ref tRef, (nuint)(k + i + step));
                    var vs1 = Vector256.LoadUnsafe(ref sRef, (nuint)(k + i + step));
                    Avx2.Xor(vt1, vs1).StoreUnsafe(ref tRef, (nuint)(k + i + step));
                }

                int vec = remaining & ~(step - 1);
                for (; i < vec; i += step)
                {
                    var vt = Vector256.LoadUnsafe(ref tRef, (nuint)(k + i));
                    var vs = Vector256.LoadUnsafe(ref sRef, (nuint)(k + i));
                    Avx2.Xor(vt, vs).StoreUnsafe(ref tRef, (nuint)(k + i));
                }

                for (; i < remaining; i++)
                    Unsafe.Add(ref tRef, k + i) ^= Unsafe.Add(ref sRef, k + i);

                return;
            }

            if (Vector.IsHardwareAccelerated)
            {
                int veclen = Vector<ulong>.Count;
                int limit = len - ((len - k) % veclen);
                for (; k < limit; k += veclen)
                {
                    var v = new Vector<ulong>(t.Slice(k)) ^ new Vector<ulong>(s.Slice(k));
                    v.CopyTo(t.Slice(k));
                }
            }
            for (; k < len; k++) t[k] ^= s[k];
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        internal static void XorRows(ReadOnlySpan<ulong> a, ReadOnlySpan<ulong> b, Span<ulong> dst)
        {
            Debug.Assert(a.Length == b.Length && b.Length == dst.Length);

            int len = a.Length;
            if (len == 0) return;

            if (Avx2.IsSupported)
            {
                ref ulong aRef = ref MemoryMarshal.GetReference(a);
                ref ulong bRef = ref MemoryMarshal.GetReference(b);
                ref ulong dRef = ref MemoryMarshal.GetReference(dst);

                int i = 0;

                int unrolled = len & ~(step * 2 - 1);
                for (; i < unrolled; i += step * 2)
                {
                    var va0 = Vector256.LoadUnsafe(ref aRef, (nuint)i);
                    var vb0 = Vector256.LoadUnsafe(ref bRef, (nuint)i);
                    Avx2.Xor(va0, vb0).StoreUnsafe(ref dRef, (nuint)i);

                    var va1 = Vector256.LoadUnsafe(ref aRef, (nuint)(i + step));
                    var vb1 = Vector256.LoadUnsafe(ref bRef, (nuint)(i + step));
                    Avx2.Xor(va1, vb1).StoreUnsafe(ref dRef, (nuint)(i + step));
                }

                int vec = len & ~(step - 1);
                for (; i < vec; i += step)
                {
                    var va = Vector256.LoadUnsafe(ref aRef, (nuint)i);
                    var vb = Vector256.LoadUnsafe(ref bRef, (nuint)i);
                    Avx2.Xor(va, vb).StoreUnsafe(ref dRef, (nuint)i);
                }

                for (; i < len; i++)
                    Unsafe.Add(ref dRef, i) = Unsafe.Add(ref aRef, i) ^ Unsafe.Add(ref bRef, i);

                return;
            }

            if (Vector.IsHardwareAccelerated)
            {
                int step = Vector<ulong>.Count;
                int i = 0, vec = len - (len % step);
                for (; i < vec; i += step)
                {
                    var va = new Vector<ulong>(a.Slice(i));
                    var vb = new Vector<ulong>(b.Slice(i));
                    (va ^ vb).CopyTo(dst.Slice(i));
                }
                for (; i < len; i++) dst[i] = a[i] ^ b[i];
                return;
            }

            for (int i = 0; i < len; i++) dst[i] = a[i] ^ b[i];
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        internal static ulong[] PackColumns(DenseMatrixZ2 B, int wPacked)
        {
            int rows = B.Rows;
            int cols = B.Cols;
            Debug.Assert(wPacked == ((rows + 63) >> 6));

            var buf = new ulong[cols * wPacked];

            for (int w = 0; w < wPacked; w++)
            {
                int rowStart = w * 64;
                int rowEnd = Math.Min(rowStart + 64, rows);
                int bitBase = 0;

                for (int r = rowStart; r < rowEnd; r++, bitBase++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        if (B.TestBit(r, j))
                            buf[j * wPacked + w] |= 1UL << bitBase;
                    }
                }
            }

            return buf;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        internal static void MulRowByPackedColumns(ReadOnlySpan<ulong> aRow, ulong[] packedCols, int wPacked, Span<ulong> dRow, int cols)
        {
            Debug.Assert(dRow.Length * 64 >= cols);
            for (int j = 0; j < cols; j++)
            {
                var bCol = new ReadOnlySpan<ulong>(packedCols, j * wPacked, wPacked);
                bool bit = DotParity(aRow, bCol);
                if (bit)
                {
                    int wj = j >> 6;
                    int bj = j & 63;
                    dRow[wj] |= 1UL << bj;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        internal static bool DotParity(ReadOnlySpan<ulong> a, ReadOnlySpan<ulong> b)
        {
            Debug.Assert(a.Length == b.Length);
            int len = a.Length;

            if (len == 0) return false;

            if (Avx2.IsSupported)
            {
                ref ulong aRef = ref MemoryMarshal.GetReference(a);
                ref ulong bRef = ref MemoryMarshal.GetReference(b);

                int i = 0;

                var vacc = Vector256<ulong>.Zero;

                int vec = len & ~(step - 1);
                for (; i < vec; i += step)
                {
                    var va = Vector256.LoadUnsafe(ref aRef, (nuint)i);
                    var vb = Vector256.LoadUnsafe(ref bRef, (nuint)i);
                    vacc = Avx2.Xor(vacc, Avx2.And(va, vb));
                }

                Span<ulong> tmp = stackalloc ulong[4];
                ref ulong tmpRef = ref MemoryMarshal.GetReference(tmp);
                vacc.StoreUnsafe(ref tmpRef);
                ulong x = tmp[0] ^ tmp[1] ^ tmp[2] ^ tmp[3];

                for (; i < len; i++)
                    x ^= Unsafe.Add(ref aRef, i) & Unsafe.Add(ref bRef, i);

                return (BitOperations.PopCount(x) & 1) != 0;
            }
            else
            {
                ulong x = 0;
                for (int i = 0; i < len; i++)
                    x ^= a[i] & b[i];
                return (BitOperations.PopCount(x) & 1) != 0;
            }
        }
    }
}
