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
        public static int[] LinearEratosthenesSieve(int limit, Predicate<int>? predicate = null)
        {
            if (limit < 2)
                return [];

            var isComposite = new BitArray(limit + 1);
            var primes = new List<int>();
            int sqrt = (int)Math.Sqrt(limit);
            for (int i = 2; i <= sqrt; i++)
            {
                if (!isComposite[i])
                {
                    if (predicate == null || predicate(i))
                        primes.Add(i);

                    for (int j = i * i; j <= limit; j += i)
                        isComposite[j] = true;
                }
            }
            for (int i = Math.Max(2, sqrt + 1); i <= limit; i++)
                if (!isComposite[i] && (predicate == null || predicate(i)))
                    primes.Add(i);
            return [.. primes];
        }
    }
}
