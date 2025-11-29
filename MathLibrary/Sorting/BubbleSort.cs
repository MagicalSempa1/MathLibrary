using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Sorting
{
    public static partial class Sorting<T> where T : INumber<T>
    {
        public static void BubbleSort(T[] array)
        {
            ArgumentNullException.ThrowIfNull(array);

            int n = array.Length;
            while (n > 1)
            {
                int lastSwap = 0;
                for (int j = 1; j < n; j++)
                {
                    if (array[j - 1] > array[j])
                    {
                        T tmp = array[j - 1];
                        array[j - 1] = array[j];
                        array[j] = tmp;

                        lastSwap = j;
                    }
                }
                n = lastSwap;
            }
        }
    }
}
