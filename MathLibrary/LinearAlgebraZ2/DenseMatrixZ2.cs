using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;

namespace MathLibrary.LinearAlgebraZ2
{
    public sealed class DenseMatrixZ2
    {
        /// <summary>Линейный буфер данных матрицы (пострично), упакованный по 64 бита на слово.</summary>
        private readonly ulong[] _data;

        /// <summary>
        /// Создаёт матрицу размера <paramref name="rows"/>×<paramref name="cols"/> над GF(2).
        /// Память инициализируется нулями.
        /// </summary>
        public DenseMatrixZ2(int rows, int cols)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(rows);
            ArgumentOutOfRangeException.ThrowIfNegative(cols);
            Rows = rows;
            Cols = cols;
            WLen = (cols + 63) >> 6;
            _data = new ulong[rows * WLen];
        }

        /// <summary>Количество строк матрицы.</summary>
        public int Rows { get; }

        /// <summary>Количество столбцов матрицы.</summary>
        public int Cols { get; }

        /// <summary>Число 64-битных слов в одной строке (длина строки в ulongs).</summary>
        public int WLen { get; }

        /// <summary>Сложение матриц по модулю 2 (эквивалентно XOR). Возвращает новую матрицу.</summary>
        public static DenseMatrixZ2 operator +(DenseMatrixZ2 a, DenseMatrixZ2 b) => Add(a, b);

        /// <summary>Вычитание матриц по модулю 2 (совпадает со сложением в GF(2)). Возвращает новую матрицу.</summary>
        public static DenseMatrixZ2 operator -(DenseMatrixZ2 a, DenseMatrixZ2 b) => Add(a, b);

        /// <summary>Матричное умножение над GF(2). Возвращает новую матрицу.</summary>
        public static DenseMatrixZ2 operator *(DenseMatrixZ2 a, DenseMatrixZ2 b) => Multiply(a, b);

        /// <summary>
        /// Сложение (XOR) двух матриц одинаковой формы. Создаёт и возвращает результат.
        /// </summary>
        public static DenseMatrixZ2 Add(DenseMatrixZ2 a, DenseMatrixZ2 b)
        {
            ArgumentNullException.ThrowIfNull(a);
            ArgumentNullException.ThrowIfNull(b);
            if (a.Rows != b.Rows || a.Cols != b.Cols)
                throw new ArgumentException("Matrices must have same shape.");

            var res = new DenseMatrixZ2(a.Rows, a.Cols);

            for (int i = 0; i < a.Rows; i++)
            {
                var ar = a.RowSpanRO(i);
                var br = b.RowSpanRO(i);
                var dr = res.RowSpan(i);
                MatrixZ2Calculator.XorRows(ar, br, dr);
            }
            return res;
        }

        /// <summary>
        /// Перемножение матриц A×B над GF(2). Внутреннее измерение должно совпадать.
        /// </summary>
        public static DenseMatrixZ2 Multiply(DenseMatrixZ2 A, DenseMatrixZ2 B)
        {
            ArgumentNullException.ThrowIfNull(A);
            ArgumentNullException.ThrowIfNull(B);
            if (A.Cols != B.Rows)
                throw new ArgumentException("Inner dimensions must match: A.Cols == B.Rows");

            int rows = A.Rows;
            int cols = B.Cols;

            int wPacked = (A.Cols + 63) >> 6; // число слов для "внутреннего" измерения
            var packedCols = MatrixZ2Calculator.PackColumns(B, wPacked);

            var result = new DenseMatrixZ2(rows, cols);
            for (int i = 0; i < rows; i++)
            {
                var aRow = A.RowSpanRO(i);
                var dRow = result.RowSpan(i);
                MatrixZ2Calculator.MulRowByPackedColumns(aRow, packedCols, wPacked, dRow, cols);
            }
            return result;
        }

        /// <summary>
        /// Создаёт матрицу из набора «столбцов чётности» <paramref name="exps"/> (массивов показателей по базису).
        /// </summary>
        public static DenseMatrixZ2 FromParityColumns(IReadOnlyList<ushort[]> exps)
        {
            if (exps == null || exps.Count == 0) throw new ArgumentException("Exps is empty");
            int r = exps.Count, m = exps[0].Length;
            for (int j = 1; j < r; j++)
                if (exps[j].Length != m) throw new ArgumentException("All columns must have same length");

            var A = new DenseMatrixZ2(m, r);
            FillFromParityColumns(A, exps);
            return A;
        }

        /// <summary>
        /// Заполняет матрицу <paramref name="A"/> из «столбцов чётности» <paramref name="exps"/>.
        /// </summary>
        public static void FillFromParityColumns(DenseMatrixZ2 A, IReadOnlyList<ushort[]> exps)
        {
            if (exps == null || exps.Count == 0) throw new ArgumentException("Exps is empty");
            int r = exps.Count, m = exps[0].Length;
            if (A.Rows != m || A.Cols != r) throw new ArgumentException("A size must be m×r");

            for (int i = 0; i < m; i++)
            {
                var dst = A.RowSpan(i);
                dst.Clear();

                int j = 0, w = 0;
                while (j + 64 <= r)
                {
                    ulong acc = 0;
                    for (int b = 0; b < 64; b++)
                        acc |= (ulong)(exps[j + b][i] & 1) << b;
                    dst[w++] = acc;
                    j += 64;
                }

                if (j < r)
                {
                    ulong acc = 0; int b = 0;
                    for (; j < r; j++, b++)
                        acc |= (ulong)(exps[j][i] & 1) << b;
                    dst[w] = acc;
                }
            }
        }

        /// <summary>Проверяет, установлен ли бит в позиции (row, col).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TestBit(int row, int col) => TestBit(row, new Column(col));

        /// <summary>Проверяет, установлен ли бит в позиции (row, c.Index) с уже подготовленными w/bit/mask.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TestBit(int row, Column c)
            => (_data[row * WLen + c.W] & c.Mask) != 0;

        /// <summary>
        /// Ищет первую строку на/ниже <paramref name="startRow"/>, где в столбце <paramref name="c"/> стоит 1.
        /// Возвращает индекс строки или -1, если не найдено.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public int FindRowWithBit(Column c, int startRow)
        {
            int w = c.W;
            ulong mask = c.Mask;
            int off = startRow * WLen + w;
            for (int i = startRow; i < Rows; i++, off += WLen)
                if ((_data[off] & mask) != 0) return i;
            return -1;
        }

        /// <summary>
        /// Выполняет XOR: rowDst ^= rowSrc, начиная с бита столбца <paramref name="c"/> (включительно).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void XorRowFromBit(int rowDst, int rowSrc, Column c)
        {
            var t = RowSpan(rowDst);
            var s = RowSpanRO(rowSrc);
            MatrixZ2Calculator.XorFromBit(t, s, c.W, c.Bit);
        }

        /// <summary>
        /// Обнуляет столбец <paramref name="c"/> в строках [<paramref name="startRow"/>, <paramref name="endRow"/>)  
        /// XOR-ами с опорной строкой <paramref name="pivotRow"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void EliminateColumnWithPivot(Column c, int pivotRow, int startRow, int endRow)
        {
            int w = c.W;
            int bit = c.Bit;
            ulong mask = c.Mask;
            var pivot = RowSpanRO(pivotRow);

            int off = startRow * WLen + w;
            for (int i = startRow; i < endRow; i++, off += WLen)
            {
                if ((_data[off] & mask) == 0) continue;
                var row = RowSpan(i);
                MatrixZ2Calculator.XorFromBit(row, pivot, w, bit);
            }
        }

        /// <summary>Меняет местами две строки матрицы.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SwapRows(int a, int b)
        {
            if (a == b) return;
            var ra = RowSpan(a);
            var rb = RowSpan(b);
            int len = ra.Length;

            const int StackThreshold = 256;
            if (len <= StackThreshold)
            {
                Span<ulong> buf = stackalloc ulong[len];
                ra.CopyTo(buf);
                rb.CopyTo(ra);
                buf.CopyTo(rb);
                return;
            }

            var tmp = ArrayPool<ulong>.Shared.Rent(len);
            try
            {
                var t = tmp.AsSpan(0, len);
                ra.CopyTo(t);
                rb.CopyTo(ra);
                t.CopyTo(rb);
            }
            finally
            {
                ArrayPool<ulong>.Shared.Return(tmp, clearArray: false);
            }
        }

        /// <summary>Возвращает строковое представление матрицы: все биты с пробелами и рамкой.</summary>
        public override string ToString()
        {
            var lines = new string[Rows];
            int maxWidth = 0;

            for (int row = 0; row < Rows; row++)
            {
                var sbRow = new StringBuilder(Cols == 0 ? 0 : Cols * 2 - 1);

                int baseIdx = row * WLen;
                int col = 0;

                for (int w = 0; w < WLen; w++)
                {
                    int startCol = w << 6;
                    int bitsInWord = Math.Min(64, Cols - startCol);
                    if (bitsInWord <= 0) break;

                    ulong word = _data[baseIdx + w];

                    for (int b = 0; b < bitsInWord; b++, col++)
                    {
                        sbRow.Append(((word >> b) & 1UL) != 0 ? '1' : '0');
                        if (col + 1 < Cols) sbRow.Append(' ');
                    }
                }

                var line = sbRow.ToString();
                lines[row] = line;
                if (line.Length > maxWidth) maxWidth = line.Length;
            }

            var sb = new StringBuilder(Math.Max(16, (maxWidth + 4) * Math.Max(1, Rows)));
            sb.Append('┌').Append('─', maxWidth + 2).Append('┐').AppendLine();
            for (int i = 0; i < lines.Length; i++)
            {
                var l = lines[i] ?? string.Empty;
                sb.Append('│').Append(' ').Append(l.PadRight(maxWidth)).Append(' ').Append('│').AppendLine();
            }
            sb.Append('└').Append('─', maxWidth + 2).Append('┘');

            return sb.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Span<ulong> GetRowSpan(int i) => RowSpan(i);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ReadOnlySpan<ulong> GetRowSpanRO(int i) => RowSpanRO(i);

        /// <summary>Возвращает изменяемый срез (Span) для строки <paramref name="i"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Span<ulong> RowSpan(int i) => _data.AsSpan(i * WLen, WLen);

        /// <summary>Возвращает только для чтения срез (ReadOnlySpan) для строки <paramref name="i"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ReadOnlySpan<ulong> RowSpanRO(int i) => _data.AsSpan(i * WLen, WLen);
    }
}
