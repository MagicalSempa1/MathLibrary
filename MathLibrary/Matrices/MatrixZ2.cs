using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Matrices
{
    public class MatrixZ2
    {
        public int N { get => _values.Length; }

        public bool this[int i, int j]
        {
            get => _values[i][j];
            set => _values[i][j] = value;
        }

        private BitArray[] _values;

        public static MatrixZ2 Identity(int n)
        {
            var result = new MatrixZ2(n);
            for (int i = 0; i < result.N; i++)
                result[i, i] = true;
            return result;
        }

        public override string? ToString()
        {
            if (N == 0) return null;
            if (N == 1) return $"|{this[0, 0]}|";
            var sb = new StringBuilder();
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N - 1; j++)
                {
                    sb.Append($"{(this[i, j] ? 1 : 0)}, ");
                }
                sb.Append($"{(this[i, N - 1] ? 1 : 0)}\n");
            }
            return sb.ToString();
        }

        public MatrixZ2(int n)
        {
            _values = new BitArray[n];
            for (int i = 0; i < _values.Length; i++)
                _values[i] = new BitArray(n);
        }

        public MatrixZ2(BitArray[] values) =>
            _values = values;
    }
}
