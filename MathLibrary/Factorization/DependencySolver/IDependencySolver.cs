using MathLibrary.LinearAlgebraZ2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Factorization.DependencySolver
{
    public interface IDependencySolver
    {
        void FromParityColumns(IReadOnlyList<ushort[]> exps);

        (bool[] pivotFlags, int[] pivotOfRow) Solve();

        void BuildDependencyVector(int[] pivotOfRow, int freeCol, bool[] z);
    }
}
