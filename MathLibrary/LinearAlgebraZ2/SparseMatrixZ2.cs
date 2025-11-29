using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.LinearAlgebraZ2
{
    public sealed class SparseMatrixZ2
    {
        /// <summary>Количество строк.</summary>
        public int Rows { get; }

        /// <summary>Количество столбцов.</summary>
        public int Cols { get; }

        /// <summary>
        /// Указатели начала строк в массиве ColIdx.
        /// Длина = Rows + 1, элементы неубывающи.
        /// </summary>
        public int[] RowPtr { get; }

        /// <summary>
        /// Индексы столбцов для всех единиц, подряд по строкам.
        /// Строка i — это диапазон [RowPtr[i], RowPtr[i+1]) в ColIdx.
        /// </summary>
        public int[] ColIdx { get; }

        /// <summary>Общее количество единичных элементов.</summary>
        public int Nnz => ColIdx.Length;

        internal SparseMatrixZ2(int rows, int cols, int[] rowPtr, int[] colIdx)
        {
            Rows = rows;
            Cols = cols;
            RowPtr = rowPtr;
            ColIdx = colIdx;
        }

        // -------------------------------------------------------------
        //  FromParityColumns
        // -------------------------------------------------------------

        /// <summary>
        /// Создаёт матрицу из набора «столбцов чётности» exps (аналогично DenseMatrixZ2.FromParityColumns).
        /// A будет размера m × r, где:
        ///   r = exps.Count,
        ///   m = exps[0].Length.
        /// В матрице A[i,j] = exps[j][i] & 1.
        /// </summary>
        public static SparseMatrixZ2 FromParityColumns(IReadOnlyList<ushort[]> exps)
        {
            if (exps == null || exps.Count == 0)
                throw new ArgumentException("Exps is empty", nameof(exps));

            int r = exps.Count;       // число столбцов
            int m = exps[0].Length;   // число строк

            for (int j = 1; j < r; j++)
                if (exps[j].Length != m)
                    throw new ArgumentException("All columns must have same length", nameof(exps));

            // 1-й проход: считаем, сколько единиц в каждой строке.
            var rowCounts = new int[m];

            for (int j = 0; j < r; j++)
            {
                var col = exps[j];
                for (int i = 0; i < m; i++)
                {
                    if ((col[i] & 1) != 0)
                        rowCounts[i]++;
                }
            }

            // Строим RowPtr по префиксным суммам.
            var rowPtr = new int[m + 1];
            int nnz = 0;
            for (int i = 0; i < m; i++)
            {
                rowPtr[i] = nnz;
                nnz += rowCounts[i];
            }
            rowPtr[m] = nnz;

            // 2-й проход: заполняем ColIdx.
            var colIdx = new int[nnz];
            var next = new int[m];          // текущая позиция записи по каждой строке
            Array.Copy(rowPtr, next, m);    // начальные позиции — это rowPtr

            for (int j = 0; j < r; j++)
            {
                var col = exps[j];
                for (int i = 0; i < m; i++)
                {
                    if ((col[i] & 1) == 0) continue;

                    int pos = next[i]++;
                    colIdx[pos] = j;        // в строке i есть единица в столбце j
                }
            }

            return new SparseMatrixZ2(m, r, rowPtr, colIdx);
        }

        // -------------------------------------------------------------
        //  Умножение на блочный вектор: y = A * x
        // -------------------------------------------------------------

        /// <summary>
        /// Вычисляет y = A * x над GF(2).
        /// A: (Rows × Cols), this
        /// x: (Cols × k), плотная матрица-блок векторов
        /// y: (Rows × k), плотная матрица-блок векторов
        /// </summary>
        public DenseMatrixZ2 Multiply(DenseMatrixZ2 x)
        {
            ArgumentNullException.ThrowIfNull(x);
            if (x.Rows != Cols)
                throw new ArgumentException("Inner dimensions must match: A.Cols == x.Rows");

            int k = x.Cols;                     // размер блока (кол-во векторов)
            var y = new DenseMatrixZ2(Rows, k); // по умолчанию заполнена нулями

            for (int i = 0; i < Rows; i++)
            {
                var yRow = y.GetRowSpan(i);     // строка результата y_i (длина = WLen по k)
                yRow.Clear();                   // ctor уже нули, но на всякий случай

                for (int idx = RowPtr[i]; idx < RowPtr[i + 1]; idx++)
                {
                    int j = ColIdx[idx];        // столбец, где A[i,j] = 1
                    var xRow = x.GetRowSpanRO(j); // строка x_j (это вектор, который надо прибавить)

                    // y_i ^= x_j (по словам ulong)
                    MatrixZ2Calculator.XorRows(yRow, xRow, yRow);
                }
            }

            return y;
        }

        // -------------------------------------------------------------
        //  Умножение на транспонированную матрицу: y = A^T * x
        // -------------------------------------------------------------

        /// <summary>
        /// Вычисляет y = A^T * x над GF(2).
        /// Удобно для блочного Ланцоша: часто нужны и A * v, и A^T * v.
        /// A:  (Rows × Cols)
        /// x:  (Rows × k)
        /// y:  (Cols × k)
        /// </summary>
        public DenseMatrixZ2 MultiplyTranspose(DenseMatrixZ2 x)
        {
            ArgumentNullException.ThrowIfNull(x);
            if (x.Rows != Rows)
                throw new ArgumentException("Inner dimensions must match: A.Rows == x.Rows");

            int k = x.Cols;
            var y = new DenseMatrixZ2(Cols, k);

            for (int i = 0; i < Rows; i++)
            {
                var xRow = x.GetRowSpanRO(i); // строка x_i

                for (int idx = RowPtr[i]; idx < RowPtr[i + 1]; idx++)
                {
                    int j = ColIdx[idx];      // A[i,j] = 1 ⇒ в строку j результата прилетает x_i
                    var yRow = y.GetRowSpan(j);

                    // y_j ^= x_i
                    MatrixZ2Calculator.XorRows(yRow, xRow, yRow);
                }
            }

            return y;
        }
    }
}
