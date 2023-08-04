using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary
{
    public static partial class Sieves
    {
        public static int[] EratosthenesSieve(int limit, Predicate<int>? predicate = null)
        {
            if (limit < 2)
                return Array.Empty<int>();

            var isComposite = new BitArray(limit + 1);
            List<int> primes = new List<int>();

            for (int i = 2; i <= limit; i++)
            {
                if (!isComposite[i])
                {
                    if (predicate == null || predicate(i))
                        primes.Add(i);

                    for (int j = i << 1; j <= limit; j += i)
                        isComposite[j] = true;
                }
            }

            return primes.ToArray();
        }
    }
}
