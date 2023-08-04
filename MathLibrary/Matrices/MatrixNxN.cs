using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Matrices
{
    public partial struct MatrixNxN<T>
        where T : INumber<T>
    {
        public int N { get => _values.Length; }

        public T this[int i, int j]
        {
            get => _values[i][j];
            set => _values[i][j] = value;
        }

        private T[][] _values;

        public static MatrixNxN<T> Identity(int n)
        {
            var result = new MatrixNxN<T>(n);
            for (int i = 0; i < result.N; i++)
                result[i, i] = T.One;
            return result;
        }

        public void SwapRows(int row1index, int row2index)
        {
            var temp = _values[row1index];
            _values[row1index] = _values[row2index];
            _values[row2index] = temp;
        }

        public (MatrixNxN<T>, MatrixNxN<T>, MatrixNxN<T>, MatrixNxN<T>) GetBlocks()
        {
            int blocklen = N % 2 == 0 ? N >> 1 : (N >> 1) + 1;
            var block11 = new MatrixNxN<T>(blocklen);
            var block12 = new MatrixNxN<T>(blocklen);
            var block21 = new MatrixNxN<T>(blocklen);
            var block22 = new MatrixNxN<T>(blocklen);
            for (int i = 0; i < blocklen; i++)
            {
                for (int j = 0; j < blocklen; j++)
                    block11[i, j] = this[i, j];
                for (int j = blocklen; j < N; j++)
                    block12[i, j - blocklen] = this[i, j];
            }
            for (int i = blocklen; i < N; i++)
            {
                for (int j = 0; j < blocklen; j++)
                    block21[i - blocklen, j] = this[i, j];
                for (int j = blocklen; j < N; j++)
                    block22[i - blocklen, j - blocklen] = this[i, j];
            }
            return (block11, block12, block21, block22);
        }

        public override string? ToString()
        {
            if (N == 0) return null;
            if (N == 1) return $"|{this[0, 0]}|";
            var sb = new StringBuilder();
            int maxlenofvalue = 1;
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N - 1; j++)
                {
                    if (this[i, j].ToString().Length > maxlenofvalue)
                        maxlenofvalue = this[i, j].ToString().Length;
                    sb.Append($"{this[i, j]}, ");
                }
                sb.Append($"{this[i, N - 1]}\n");
            }
            return sb.ToString();
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj == null)
                throw new ArgumentNullException();
            if (obj is MatrixNxN<T> matrixNxN)
            {
                if (N != matrixNxN.N)
                    return false;
            }
            return base.Equals(obj);
        }

        public MatrixNxN(int n)
        {
            _values = new T[n][];
            for (int i = 0; i < _values.Length; i++)
                _values[i] = new T[n];
        }

        public MatrixNxN(T[][] values) =>
            _values = values;
    }
}