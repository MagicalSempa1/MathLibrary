using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Matrices
{
    public interface IMatrix<T>
        where T : INumber<T>
    {
        public static MatrixNxN<T> Identity(int n)
        {
            var result = new MatrixNxN<T>(n);
            for (int i = 0; i < result.N; i++)
                result[i, i] = T.One;
            return result;
        }
    }
}
