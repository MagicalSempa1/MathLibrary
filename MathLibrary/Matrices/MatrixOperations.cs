using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Matrices
{
    public partial struct MatrixNxN<T>
        where T : INumber<T>
    {
        #region Add
        public static MatrixNxN<T> operator +(MatrixNxN<T> left, MatrixNxN<T> right) => Add(left, right);

        public static MatrixNxN<T> operator +(T left, MatrixNxN<T> right) => Add(left, right);

        public static MatrixNxN<T> operator +(MatrixNxN<T> left, T right) => Add(left, right);

        public static MatrixNxN<T> Add(MatrixNxN<T> left, MatrixNxN<T> right)
        {
            var result = new MatrixNxN<T>(left.N);
            for (int i = 0; i < result.N; i++)
                for (int j = 0; j < result.N; j++)
                    result[i, j] = left[i, j] + right[i, j];
            return result;
        }

        public static MatrixNxN<T> Add(T left, MatrixNxN<T> right)
        {
            var result = new MatrixNxN<T>(right.N);
            for (int i = 0; i < result.N; i++)
                result[i, i] = left + right[i, i];
            return result;
        }

        public static MatrixNxN<T> Add(MatrixNxN<T> left, T right)
        {
            var result = new MatrixNxN<T>(left.N);
            for (int i = 0; i < result.N; i++)
                result[i, i] = left[i, i] + right;
            return result;
        }
        #endregion

        #region Sub
        public static MatrixNxN<T> operator -(MatrixNxN<T> left, MatrixNxN<T> right) => Subtract(left, right);

        public static MatrixNxN<T> operator -(T left, MatrixNxN<T> right) => Subtract(left, right);

        public static MatrixNxN<T> operator -(MatrixNxN<T> left, T right) => Subtract(left, right);

        public static MatrixNxN<T> Subtract(MatrixNxN<T> left, MatrixNxN<T> right)
        {
            var result = new MatrixNxN<T>(left.N);
            for (int i = 0; i < result.N; i++)
                for (int j = 0; j < result.N; j++)
                    result[i, j] = left[i, j] - right[i, j];
            return result;
        }

        public static MatrixNxN<T> Subtract(T left, MatrixNxN<T> right)
        {
            var result = new MatrixNxN<T>(right.N);
            for (int i = 0; i < result.N; i++)
                result[i, i] = left - right[i, i];
            return result;
        }

        public static MatrixNxN<T> Subtract(MatrixNxN<T> left, T right)
        {
            var result = new MatrixNxN<T>(left.N);
            for (int i = 0; i < result.N; i++)
                result[i, i] = left[i, i] - right;
            return result;
        }
        #endregion

        #region Mul
        public static MatrixNxN<T> operator *(MatrixNxN<T> left, MatrixNxN<T> right) => Multiply(left, right);

        public static MatrixNxN<T> operator *(T left, MatrixNxN<T> right) => Multiply(left, right);

        public static MatrixNxN<T> operator *(MatrixNxN<T> left, T right) => Multiply(left, right);

        public static MatrixNxN<T> Multiply(MatrixNxN<T> left, MatrixNxN<T> right)
        {
            var result = new MatrixNxN<T>(left.N);
            for (int i = 0; i < result.N; i++)
                for (int j = 0; j < result.N; j++)
                    for (int k = 0; k < result.N; k++)
                        result[i, j] += left[i, k] * right[k, j];
            return result;
        }

        public static MatrixNxN<T> SchoolbookMultiply(MatrixNxN<T> left, MatrixNxN<T> right)
        {
            var result = new MatrixNxN<T>(left.N);
            for (int i = 0; i < result.N; i++)
                for (int j = 0; j < result.N; j++)
                    for (int k = 0; k < result.N; k++)
                        result[i, j] += left[i, k] * right[k, j];
            return result;
        }

        public static MatrixNxN<T> ParallelSchoolbookMultiply(MatrixNxN<T> left, MatrixNxN<T> right)
        {
            var result = new MatrixNxN<T>(left.N);
            Parallel.For(0, result.N, i =>
            {
                for (int j = 0; j < result.N; j++)
                    for (int k = 0; k < result.N; k++)
                        result[i, j] += left[i, k] * right[k, j];
            });
            return result;
        }

        public static MatrixNxN<T> StrassenMultiply(MatrixNxN<T> left, MatrixNxN<T> right)
        {
            var result = new MatrixNxN<T>(left.N);
            (MatrixNxN<T> a11, MatrixNxN<T> a12, MatrixNxN<T> a21, MatrixNxN<T> a22) leftblocks = left.GetBlocks();
            (MatrixNxN<T> b11, MatrixNxN<T> b12, MatrixNxN<T> b21, MatrixNxN<T> b22) rightblocks = right.GetBlocks();
            var S1 = leftblocks.a21 + leftblocks.a22;
            var S2 = S1 - leftblocks.a11;
            var S3 = leftblocks.a11 - leftblocks.a21;
            return result;
        }

        public static MatrixNxN<T> Multiply(T left, MatrixNxN<T> right)
        {
            var result = new MatrixNxN<T>(right.N);
            for (int i = 0; i < result.N; i++)
                for (int j = 0; j < result.N; j++)
                        result[i, j] += left * right[i, j];
            return result;
        }

        public static MatrixNxN<T> Multiply(MatrixNxN<T> left, T right)
        {
            var result = new MatrixNxN<T>(left.N);
            for (int i = 0; i < result.N; i++)
                for (int j = 0; j < result.N; j++)
                    result[i, j] += left[i, j] * right;
            return result;
        }
        #endregion

        #region Pow
        public static MatrixNxN<T> operator ^(MatrixNxN<T> matrix, int n) => Pow(matrix, n);

        public static MatrixNxN<T> Pow(MatrixNxN<T> matrix, int n)
        {
            if (n == 0)
                return Identity(matrix.N);
            if (n == 1)
                return matrix;
            MatrixNxN<T> result = Identity(matrix.N);
            var pow = matrix;
            while (n > 0)
            {
                if (n % 2 == 1)
                    result *= pow;
                pow *= pow;
                n = n >> 1;
            }
            return result;
        }
        #endregion

        public static MatrixNxN<T> Transpose(MatrixNxN<T> matrix)
        {
            var result = new MatrixNxN<T>(matrix.N);
            for (int i = 0; i < result.N; i++)
                for (int j = 0; j < result.N; j++)
                    result[i, j] = matrix[i, j];
            return result;
        }
    }
}
