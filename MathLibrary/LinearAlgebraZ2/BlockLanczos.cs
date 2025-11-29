using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.LinearAlgebraZ2
{
    public static partial class Z2Solver
    {
        public static (bool[] pivotFlags, int[] pivotOfRow, SparseMatrixZ2 reduced) BlockLanczosSolve(SparseMatrixZ2 A)
        {
            ArgumentNullException.ThrowIfNull(A);

            int rows = A.Rows;
            int cols = A.Cols;

            const int BlockSize = 64;
            int b = Math.Min(BlockSize, cols);

            var v0 = CreateRandomBlock(cols, b);

            DenseMatrixZ2 ApplySymmetric(DenseMatrixZ2 v)
            {
                var y = A.Multiply(v);
                var z = A.MultiplyTranspose(y);
                return z;
            }

            // 3. Здесь ДОЛЖЕН жить настоящий блочный Ланцош:
            //
            // Блоки:
            //   V0, V1, V2, ...
            //   W0, W1, W2, ... (A-ортогональные блоки)
            //
            // Основные шаги:
            //   - выбор A-invertible подпространства (в терминах Montgomery — S_i, W_i);
            //   - построение блока следующих векторов через S = AᵀA;
            //   - поддержка A-ортогональности блоков;
            //   - накопление информации для восстановления зависимостей.
            //
            // Ядро алгоритма слишком объёмное, чтобы честно реализовать его здесь без ошибок,
            // поэтому ниже — заглушка, которая просто бросает исключение.
            //
            // См.:
            //   P. L. Montgomery, “A Block Lanczos Algorithm for Finding Dependencies over GF(2)” (EUROCRYPT’95).
            //   Michael Peterson, “Parallel Block Lanczos for Solving Large Binary Systems” (thesis).
            //   Реализации в msieve / CADO-NFS (модули lanczos.* / blocklanczos.*).

            throw new NotSupportedException(
                "Полная реализация блочного Ланцоша (Montgomery) слишком объёмна для этого ответа. " +
                "Используйте данный каркас как точку входа: сюда нужно вставить реализацию " +
                "из статьи Montgomery / кода msieve / CADO-NFS, адаптированную под SparseMatrixZ2/DenseMatrixZ2."
            );

            // Ниже показан ожидаемый формат результата, если ты реализуешь алгоритм:

            // var dependencyVectors = new List<bool[]>(); // или DenseMatrixZ2 / SparseMatrixZ2
            //
            // ... тут отработал твой block Lanczos, нашёл k зависимостей x₀, …, x_{k-1} длины cols ...
            //
            // var reduced = BuildSparseFromDependencies(cols, dependencyVectors);
            //
            // // pivotFlags/pivotOfRow можно:
            // //   - либо оставить "пустыми" (false/-1),
            // //   - либо посчитать для reduced, прогнав по нему твой GaussSolve уже как по маленькой матрице зависимостей.
            //
            // var pivotFlags = new bool[cols];
            // var pivotOfRow = new int[reduced.Rows];
            // Array.Fill(pivotOfRow, -1);
            //
            // return (pivotFlags, pivotOfRow, reduced);
        }

        // ----------------------------------------------------------------
        // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ (их можно использовать и в реальном Ланцоше)
        // ----------------------------------------------------------------

        /// <summary>Создаёт случайный блочный вектор размера n × b над GF(2).</summary>
        private static DenseMatrixZ2 CreateRandomBlock(int n, int b)
        {
            var rnd = Random.Shared;
            var block = new DenseMatrixZ2(n, b);

            // Идём по строкам, заполняем случайными ulong’ами.
            for (int i = 0; i < n; i++)
            {
                // внутренний доступ к строкам; см. примечание ниже
                var row = block.GetRowSpan(i);
                for (int w = 0; w < row.Length; w++)
                    row[w] = (ulong)rnd.NextInt64();
            }

            // Можно дополнительно убедиться, что блок не нулевой и имеет приемлемый ранг.

            return block;
        }

        /// <summary>
        /// Строит разреженную матрицу, строки которой — найденные векторы зависимостей
        /// (каждый вектор задан как bool[] длины cols).
        /// </summary>
        private static SparseMatrixZ2 BuildSparseFromDependencies(int cols, IReadOnlyList<bool[]> deps)
        {
            int rows = deps.Count;
            var rowPtr = new int[rows + 1];
            var tmpCols = new List<int>();

            int nnz = 0;
            for (int i = 0; i < rows; i++)
            {
                rowPtr[i] = nnz;
                var v = deps[i];
                if (v.Length != cols)
                    throw new ArgumentException("All dependency vectors must have same length = number of columns");

                for (int c = 0; c < cols; c++)
                {
                    if (v[c])
                    {
                        tmpCols.Add(c);
                        nnz++;
                    }
                }
            }
            rowPtr[rows] = nnz;

            var colIdx = tmpCols.ToArray();
            return new SparseMatrixZ2(rows, cols, rowPtr, colIdx);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static DenseMatrixZ2 ApplyA(SparseMatrixZ2 A, DenseMatrixZ2 V)
        {
            ArgumentNullException.ThrowIfNull(A);
            ArgumentNullException.ThrowIfNull(V);
            if (V.Rows != A.Cols)
                throw new ArgumentException("Inner dimensions must match: A.Cols == V.Rows");

            return A.Multiply(V);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static DenseMatrixZ2 ApplyAt(SparseMatrixZ2 A, DenseMatrixZ2 V)
        {
            ArgumentNullException.ThrowIfNull(A);
            ArgumentNullException.ThrowIfNull(V);
            if (V.Rows != A.Rows)
                throw new ArgumentException("Inner dimensions must match: A.Rows == V.Rows");

            return A.MultiplyTranspose(V);
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        internal static DenseMatrixZ2 ApplySymmetric(SparseMatrixZ2 A, DenseMatrixZ2 V)
        {
            // y = A * V
            var y = ApplyA(A, V);

            // z = A^T * y
            var z = ApplyAt(A, y);

            return z;
        }
        internal static DenseMatrixZ2 CreateRandomBlock(int dimension, int blockSize, Random? rng = null)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(dimension);
            ArgumentOutOfRangeException.ThrowIfNegative(blockSize);

            rng ??= Random.Shared;

            var block = new DenseMatrixZ2(dimension, blockSize);
            int rows = dimension;

            for (int i = 0; i < rows; i++)
            {
                var row = block.GetRowSpan(i);
                for (int w = 0; w < row.Length; w++)
                {
                    row[w] = (ulong)rng.NextInt64();
                }
            }

            // (Опционально) Можно гарантировать, что ни один столбец не нулевой,
            // но для первого приближения этого достаточно.

            return block;
        }
        internal static DenseMatrixZ2 CloneBlock(DenseMatrixZ2 src)
        {
            ArgumentNullException.ThrowIfNull(src);

            var dst = new DenseMatrixZ2(src.Rows, src.Cols);
            int rows = src.Rows;

            for (int i = 0; i < rows; i++)
            {
                var s = src.GetRowSpanRO(i);
                var d = dst.GetRowSpan(i);
                s.CopyTo(d);
            }

            return dst;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        internal static void XorBlocksInPlace(DenseMatrixZ2 dst, DenseMatrixZ2 src)
        {
            ArgumentNullException.ThrowIfNull(dst);
            ArgumentNullException.ThrowIfNull(src);
            if (dst.Rows != src.Rows || dst.Cols != src.Cols)
                throw new ArgumentException("Shapes of dst and src must match.");

            int rows = dst.Rows;
            for (int i = 0; i < rows; i++)
            {
                var d = dst.GetRowSpan(i);
                var s = src.GetRowSpanRO(i);
                MatrixZ2Calculator.XorRows(d, s, d);
            }
        }
        internal static void ComputeGram(DenseMatrixZ2 V, DenseMatrixZ2 AV, ulong[] gram)
        {
            ArgumentNullException.ThrowIfNull(V);
            ArgumentNullException.ThrowIfNull(AV);
            ArgumentNullException.ThrowIfNull(gram);

            if (V.Rows != AV.Rows || V.Cols != AV.Cols)
                throw new ArgumentException("Shapes of V and AV must match.");

            int n = V.Rows;
            int b = V.Cols;
            if (b == 0) return;
            if (b > 64)
                throw new ArgumentOutOfRangeException(nameof(V),
                    "ComputeGram рассчитан на блок шириной ≤ 64 столбцов.");
            if (gram.Length < b)
                throw new ArgumentException("gram length must be ≥ number of block columns.");

            Array.Clear(gram, 0, b);

            if (V.WLen != 1 || AV.WLen != 1)
                throw new InvalidOperationException("ComputeGram ожидает WLen == 1 для V и AV.");

            for (int row = 0; row < n; row++)
            {
                var vRow = V.GetRowSpanRO(row);
                var aRow = AV.GetRowSpanRO(row);

                ulong v = vRow[0];
                ulong u = aRow[0];

                // Для каждого установленного бита p в v:
                // G[p,*] ^= u
                while (v != 0)
                {
                    int p = BitOperations.TrailingZeroCount(v);
                    v &= v - 1;
                    gram[p] ^= u;
                }
            }

            // Обнуляем биты правее b-1
            if (b < 64)
            {
                ulong mask = (1UL << b) - 1;
                for (int i = 0; i < b; i++)
                    gram[i] &= mask;
            }
        }
    }
}
