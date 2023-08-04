using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary
{
    public class GaussZ2
    {
        public static BitArray Solve(BitArray[] matrix, BitArray vector)
        {
            int rows = matrix.Length;
            int cols = matrix[0].Length;
            for (int col = 0; col < cols; col++)
            {
                int pivotRow = -1;
                for (int row = col; row < rows; row++)
                {
                    if (matrix[row][col])
                    {
                        pivotRow = row;
                        break;
                    }
                }
                if (pivotRow == -1)
                    throw new ArgumentException("Матрица необратима.");
                SwapRows(matrix, vector, col, pivotRow);
                for (int row = col + 1; row < rows; row++)
                {
                    if (matrix[row][col])
                    {
                        matrix[row] = matrix[row].Xor(matrix[col]);
                        vector[row] ^= vector[col];
                    }
                }
            }
            BitArray solution = new BitArray(vector.Length);
            for (int row = rows - 1; row >= 0; row--)
            {
                bool sum = false;
                for (int col = row + 1; col < cols; col++)
                    sum ^= matrix[row][col] && solution[col];
                solution[row] = vector[row] ^ sum;
            }
            return solution;
        }

        private static void SwapRows(BitArray[] matrix, BitArray vector, int row1, int row2)
        {
            BitArray tempRow = matrix[row1];
            matrix[row1] = matrix[row2];
            matrix[row2] = tempRow;
            bool tempVal = vector[row1];
            vector[row1] = vector[row2];
            vector[row2] = tempVal;
        }
    }
}
