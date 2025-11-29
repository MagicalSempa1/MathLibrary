using System.Numerics;
using System.Runtime.CompilerServices;

namespace MathLibrary.LinearAlgebraZ2
{
    public static partial class Z2Solver
    {
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static (bool[] pivotFlags, int[] pivotOfRow, DenseMatrixZ2 reduced) GaussSolve(DenseMatrixZ2 A)
        {
            int rows = A.Rows, cols = A.Cols;
            var pivotFlags = new bool[cols];
            var pivotOfRow = new int[rows];
            Array.Fill(pivotOfRow, -1);

            int r = 0;
            for (int c = 0; c < cols && r < rows; c++)
            {
                var col = new Column(c);

                int sel = A.FindRowWithBit(col, r);
                if (sel == -1) continue;

                A.SwapRows(r, sel);

                A.EliminateColumnWithPivot(col, r, r + 1, rows);

                A.EliminateColumnWithPivot(col, r, 0, r);

                pivotFlags[c] = true;
                pivotOfRow[r] = c;
                r++;
            }

            return (pivotFlags, pivotOfRow, A);
        }
    }
}
