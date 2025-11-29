using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.LinearAlgebraZ2
{
    public static class LAHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BuildDependencyVector(this DenseMatrixZ2 A, int[] pivotOfRow, int freeCol, bool[] z)
        {
            Array.Clear(z, 0, z.Length);
            z[freeCol] = true;

            var col = new Column(freeCol);
            for (int i = 0; i < A.Rows; i++)
            {
                int p = pivotOfRow[i];
                if (p < 0) continue;
                if (A.TestBit(i, col))
                    z[p] = !z[p];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool[] CreateDependencyVector(this DenseMatrixZ2 A, int[] pivotOfRow, int freeCol)
        {
            var z = new bool[A.Cols];
            BuildDependencyVector(A, pivotOfRow, freeCol, z);
            return z;
        }

        public static double GetNonZeroPercentage(this DenseMatrixZ2 matrix)
        {
            ArgumentNullException.ThrowIfNull(matrix);

            long rows = matrix.Rows;
            long cols = matrix.Cols;
            long total = rows * cols;
            if (total == 0)
                return 0.0;

            long ones = 0;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (matrix.TestBit(i, j))
                        ones++;
                }
            }

            return (double)ones * 100.0 / total;
        }

        public static ulong[] CreateIdentity(int n)
        {
            if (n < 0 || n > 64)
                throw new ArgumentOutOfRangeException(nameof(n), "n must be between 0 and 64.");

            var id = new ulong[n];
            for (int i = 0; i < n; i++)
                id[i] = 1UL << i;

            return id;
        }

        public static void Copy(ulong[] src, ulong[] dst, int n)
        {
            ArgumentNullException.ThrowIfNull(src);
            ArgumentNullException.ThrowIfNull(dst);
            if (n < 0 || n > src.Length || n > dst.Length)
                throw new ArgumentOutOfRangeException(nameof(n));

            Array.Copy(src, dst, n);
        }
        public static void MultiplySquareByRect(
        ulong[] aRows, int n,
        ulong[] bRows,
        ulong[] resultRows)
        {
            ArgumentNullException.ThrowIfNull(aRows);
            ArgumentNullException.ThrowIfNull(bRows);
            ArgumentNullException.ThrowIfNull(resultRows);
            if (n < 0 || n > aRows.Length || n > bRows.Length || n > resultRows.Length)
                throw new ArgumentOutOfRangeException(nameof(n));
            if (n > 64)
                throw new ArgumentOutOfRangeException(nameof(n), "n must be ≤ 64.");

            for (int i = 0; i < n; i++)
            {
                ulong acc = 0;
                ulong row = aRows[i];

                // C[i,*] = XOR по всем j, где A[i,j] = 1, строк B[j,*]
                while (row != 0)
                {
                    int j = BitOperations.TrailingZeroCount(row); // индекс младшего установленного бита
                    row &= row - 1; // сбрасываем этот бит
                    acc ^= bRows[j];
                }

                resultRows[i] = acc;
            }
        }

        public static void MultiplySquareBySquare(
            ulong[] aRows,
            ulong[] bRows,
            int n,
            ulong[] resultRows)
        {
            MultiplySquareByRect(aRows, n, bRows, resultRows);
        }

        public static int Invert(ulong[] aRows, int n, ulong[] invRows)
        {
            ArgumentNullException.ThrowIfNull(aRows);
            ArgumentNullException.ThrowIfNull(invRows);
            if (n < 0 || n > aRows.Length || n > invRows.Length)
                throw new ArgumentOutOfRangeException(nameof(n));
            if (n > 64)
                throw new ArgumentOutOfRangeException(nameof(n), "n must be ≤ 64.");

            // invRows = I_n
            for (int i = 0; i < n; i++)
                invRows[i] = 1UL << i;

            int rank = 0;

            // Гаусс-Жордан по столбцам 0..n-1
            for (int col = 0; col < n; col++)
            {
                // Ищем опорную строку с единицей в этом столбце начиная с row = rank
                int pivotRow = -1;
                for (int row = rank; row < n; row++)
                {
                    if (((aRows[row] >> col) & 1UL) != 0)
                    {
                        pivotRow = row;
                        break;
                    }
                }

                if (pivotRow == -1)
                {
                    // Нет опорного элемента в этом столбце — переходим к следующему столбцу.
                    continue;
                }

                // Меняем местами строки rank и pivotRow и в A, и в inv
                if (pivotRow != rank)
                {
                    (aRows[rank], aRows[pivotRow]) = (aRows[pivotRow], aRows[rank]);
                    (invRows[rank], invRows[pivotRow]) = (invRows[pivotRow], invRows[rank]);
                }

                // Обнуляем этот столбец во всех других строках
                for (int row = 0; row < n; row++)
                {
                    if (row == rank) continue;

                    if (((aRows[row] >> col) & 1UL) != 0)
                    {
                        aRows[row] ^= aRows[rank];
                        invRows[row] ^= invRows[rank];
                    }
                }

                rank++;
            }

            // Если rank == n, то aRows должна быть приведена к единичной,
            // а invRows — это инверсия исходной матрицы.
            return rank;
        }

        public static (ulong[] inverse, int rank) InvertCopy(ulong[] aRows, int n)
        {
            ArgumentNullException.ThrowIfNull(aRows);
            if (n < 0 || n > aRows.Length)
                throw new ArgumentOutOfRangeException(nameof(n));

            var aCopy = new ulong[n];
            Array.Copy(aRows, aCopy, n);

            var inv = new ulong[n];
            int rank = Invert(aCopy, n, inv);

            return (inv, rank);
        }
    }
}
