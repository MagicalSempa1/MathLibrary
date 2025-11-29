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
        public static void InsertionSort(T[] array)
        {
            ArgumentNullException.ThrowIfNull(array);

            for (int i = 1; i < array.Length; i++)
            {
                T key = array[i];
                if (array[i - 1] <= key) continue;

                int lo = 0, hi = i;
                while (lo < hi)
                {
                    int mid = (lo + hi) >> 1;
                    if (array[mid] <= key) lo = mid + 1;
                    else hi = mid;
                }

                for (int j = i; j > lo; j--)
                    array[j] = array[j - 1];

                array[lo] = key;
            }
        }
    }
}
