using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.LinearAlgebraZ2
{
    public readonly record struct Column(int Index)
    {
        public int W => Index >> 6;
        public int Bit => Index & 63;
        public ulong Mask => 1UL << Bit;
        public override string ToString() => $"col {Index} (w={W}, bit={Bit})";
    }
}
