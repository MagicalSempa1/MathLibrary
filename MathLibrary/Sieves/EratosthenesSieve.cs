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
        public static int[] EratosthenesSieve(int limit)
        {
            if (limit < 2)
                return [];

            const int segmentSize = 65536;
            var primes = new List<int>();

            int sqrtLimit = (int)Math.Sqrt(limit);
            var isPrime = new BitArray(sqrtLimit + 1, true);

            isPrime[0] = isPrime[1] = false;
            for (int p = 2; p * p <= sqrtLimit; p++)
                if (isPrime[p])
                    for (int i = p * p; i <= sqrtLimit; i += p)
                        isPrime[i] = false;

            var basePrimes = new List<int>();
            for (int p = 2; p <= sqrtLimit; p++)
                if (isPrime[p])
                    basePrimes.Add(p);
            primes.AddRange(basePrimes);

            var segment = new BitArray(segmentSize);
            long low = sqrtLimit + 1;
            long high = low + segmentSize - 1;

            while (low <= limit)
            {
                if (high > limit)
                    high = limit;

                segment.SetAll(true);

                for (int i = 0; i < basePrimes.Count; i++)
                {
                    int p = basePrimes[i];
                    long start = (long)Math.Floor((double)low / p) * p;
                    if (start < low)
                        start += p;

                    for (long j = start; j <= high; j += p)
                        segment[(int)(j - low)] = false;
                }

                for (int i = 0; i < (int)(high - low + 1); i++)
                    if (segment[i])
                        primes.Add((int)(low + i));

                low += segmentSize;
                high += segmentSize;
            }

            return [.. primes];
        }
    }
}
