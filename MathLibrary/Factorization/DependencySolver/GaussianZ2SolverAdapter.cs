using MathLibrary.LinearAlgebraZ2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Factorization.DependencySolver
{
    public sealed class GaussianZ2SolverAdapter : IDependencySolver
    {
        private DenseMatrixZ2 A;
        private DenseMatrixZ2 Reduced;

        public void BuildDependencyVector(int[] pivotOfRow, int freeCol, bool[] z)
        {
            Reduced.BuildDependencyVector(pivotOfRow, freeCol, z);
        }

        public void FromParityColumns(IReadOnlyList<ushort[]> exps)
        {
            A = DenseMatrixZ2.FromParityColumns(exps);
        }

        public (bool[] pivotFlags, int[] pivotOfRow) Solve()
        {
            var result = Z2Solver.GaussSolve(A);
            Reduced = result.reduced;
            return (result.pivotFlags, result.pivotOfRow);
        }
    }
}
