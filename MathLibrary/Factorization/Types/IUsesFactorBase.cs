using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Factorization.Types
{
    public interface IUsesFactorBase
    {
        void Bind(ReadOnlySpan<int> fb);
    }
}
