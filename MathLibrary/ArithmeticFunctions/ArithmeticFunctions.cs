using System.Collections;
using System.Numerics;
using MathLibrary.Factorization;

namespace MathLibrary
{
    public static partial class ArithmeticFunctions
    {
        public static BigInteger EulerTotientFunction(BigInteger n)
        {
            var primes = Factorization.Factorization.EnumerateMethod(n);
            BigInteger result = 1;
            foreach (var prime in primes)
            {
                int k = primes.Length - primes.Where(i => i != prime).Count();
                result *= BigInteger.Pow(prime, k) - BigInteger.Pow(prime, k - 1);
            }
            return result;
        }

        public static BigInteger Order(BigInteger a, BigInteger n)
        {
            if (a == 1)
                return 1;
            BigInteger k = 1;
            BigInteger m = a;
            do
            {
                m = m * a % n;
                k++;
            } while (m != 1);
            return k;
        }

        public static bool IsPrimitiveRoot(BigInteger g, BigInteger n) => Order(g, n) == EulerTotientFunction(n);

        public static BigInteger DivisorFunction(BigInteger n, int x)
        {
            var primes = Factorization.Factorization.EnumerateMethod(n);
            BigInteger result = 1;
            if (x != 0)
            {
                foreach (var prime in primes)
                {
                    int k = primes.Length - primes.Where(i => i != prime).Count();
                    result *= BigInteger.Pow(prime, (k + 1) * x) - 1;
                    result /= BigInteger.Pow(prime, x) - 1;
                }
            }
            else
            {
                foreach (var prime in primes)
                {
                    int k = primes.Length - primes.Where(i => i != prime).Count();
                    result *= k + 1;
                }
            }
            return result;
        }

        public static BigInteger SmallOmegaFunction(BigInteger n)
        {
            if (n == 1)
                return 0;
            BigInteger result = 0;
            var sqrt = n.FloorSqrt();
            if (n % 2 == 0)
            {
                result++;
                while (n % 2 == 0)
                    n >>= 1;
            }
            for (BigInteger i = 3; i <= sqrt; i += 2)
            {
                if (n % i == 0)
                {
                    result++;
                    while (n % i == 0)
                        n /= i;
                }
            }
            if (n != 1)
                result++;
            return result;
        }

        public static BigInteger OmegaFunction(BigInteger n)
        {
            if (n == 1)
                return 0;
            BigInteger result = 0;
            var sqrt = n.FloorSqrt();
            while (n % 2 == 0)
            {
                result++;
                n >>= 1;
            }
            for (BigInteger i = 3; i <= sqrt; i += 2)
            {
                while (n % i == 0)
                {
                    result++;
                    n /= i;
                }
            }
            if (n != 1)
                result++;
            return result;
        }

        public static int MobiusFunction(BigInteger n)
        {
            if (n == 1)
                return 1;
            if (!n.WithoutSquares())
                return 0;
            return SmallOmegaFunction(n) % 2 == 1 ? -1 : 1;
        }

        public static int LiouvilleFunction(BigInteger n)
        {
            if (n == 1)
                return 1;
            return OmegaFunction(n) % 2 == 1 ? -1 : 1;
        }

        public static BigInteger MertensFunction(BigInteger n)
        {
            BigInteger result = 0;
            for (BigInteger k = 1; k <= n; k++)
                result += MobiusFunction(k);
            return result;
        }

        public static long MertensFunction(long n)
        {
            int result = 1;
            var primes = Sieves.AtkinSieve(n);
            var mu = new int[n + 1];
            for (int i = 0; i < primes.Length; i++)
            {

            }
            return result;
        }

        public static int PrimeCountingFunction(int n)
        {
            int result = 0;
            var sieve = new BitArray(n + 1);
            for (int x2 = 1, dx2 = 3; x2 < n; x2 += dx2, dx2 += 2)
                for (int y2 = 1, dy2 = 3, m; y2 < n; y2 += dy2, dy2 += 2)
                {
                    m = (x2 << 2) + y2;
                    if (m <= n && (m % 12 == 1 || m % 12 == 5))
                        sieve[m] ^= true;
                    m -= x2;
                    if (m <= n && m % 12 == 7)
                        sieve[m] ^= true;
                    if (x2 > y2)
                    {
                        m -= y2 << 1;
                        if (m <= n && m % 12 == 11)
                            sieve[m] ^= true;
                    }
                }
            int r = 5;
            for (int r2 = r * r, dr2 = (r << 1) + 1; r2 < n; ++r, r2 += dr2, dr2 += 2)
                if (sieve[r])
                    for (int mr2 = r2; mr2 < n; mr2 += r2)
                        sieve[mr2] = false;
            if (n >= 2)
                sieve[2] = true;
            if (n >= 3)
                sieve[3] = true;
            for (int i = 0; i < sieve.Length; i++)
            {
                if (sieve[i])
                    result++;
            }
            return result;
        }

        public static BigInteger InverseElement(this BigInteger a, BigInteger m)
        {
            (BigInteger u, BigInteger w) = (1, 0);
            BigInteger b = a;
            while (m != 0)
            {
                BigInteger r;
                var q = BigInteger.DivRem(b, m, out r);
                b = m;
                m = r;
                (u, w) = (w, u - q * w);
            }
            return u;
        }

        public static BigInteger InverseElement(this BigInteger a, BigInteger n, BigInteger p) => BigInteger.ModPow(a, p - 1 - n % p, p);

        public static BigInteger[] InverseElements(this ulong m)
        {
            BigInteger[] elements = new BigInteger[m - 1];
            elements[0] = 1;
            for (ulong i = 1; i < m - 1; ++i)
                elements[i] = (m - m / i * elements[m % i] % m) % m;
            return elements;
        }

        public static int LegendreSymbol(BigInteger a, BigInteger p)
        {
            if (a == 1 || p == 2)
                return 1;
            if (a % p == 0)
                return 0;
            if (a % p == 2)
            {
                var t = p % 8;
                return t == 1 || t == 7 ? 1 : -1;
            }
            if (a % p == 3)
            {
                if (p % 12 == 1 || p % 12 == 11)
                    return 1;
                if (p % 12 == 5 || p % 12 == 7)
                    return -1;
            }
            if (a % p == p - 1)
                return (p - 1 >> 1).IsEven ? 1 : -1;
            return BigInteger.ModPow(a, p - 1 >> 1, p) == 1 ? 1 : -1;
        }

        public static int KroneckerSymbol(BigInteger a, BigInteger n)
        {
            BigInteger t = 0;
            int k = 1;
            if (n == 0)
            {
                if (BigInteger.Abs(a) == 1)
                    return 1;
                return 0;
            }
            if (n % 2 == 0)
            {
                if (a % 2 == 0)
                    return 0;
                do
                {
                    t++;
                    n >>= 1;
                } while (n % 2 == 0);
                if (t % 2 == 0)
                    k = 1;
                else
                {
                    int r = (int)(a % 8);
                    if (r == 1 | r == 7)
                        k = 1;
                    else
                        k = -1;
                }
                if (n < 0)
                {
                    n = -n;
                    if (a < 0)
                        k = -k;
                }
            }
            do
            {
                if (a == 0)
                {
                    if (n > 1)
                        return 0;
                    if (n == 1)
                        return k;
                }
                t = 0;
                while (a % 2 == 0)
                {
                    t++;
                    a /= 2;
                }
                if (t % 2 == 1)
                {
                    int r = (int)(a % 8);
                    if (r != 1 | r != 7)
                        k = -k;
                }
                k = ((a - 1) * (n - 1) >> 2) % 2 == 0 ? k : -k;
                BigInteger m = BigInteger.Abs(a);
                a = n % m;
                n = m;
            }
            while (true);
        }

        public static int CubicResidueSymbol(BigInteger a, BigInteger p)
        {
            if (p == 2 || p == 3)
                return 1;
            if (a % p == 0)
                return 0;
            if (p % 3 == 2)
                return 1;
            a %= p;
            return BigInteger.ModPow(a, (p - 1) / 3, p) == 1 ? 1 : -1;
        }

        public static BigInteger ModFactorial(BigInteger n, BigInteger m)
        {
            if (n >= m)
                return 0;
            BigInteger r = n % m;
            for (BigInteger i = 1; i < n - 1; i++)
            {
                r = r * (n - i) % m;
                if (r == 0)
                    return 0;
            }
            return r;
        }

        public static BigInteger ModBinomial(BigInteger n, BigInteger k, BigInteger m)
        {
            if (k == 0 || n == k)
                return 1;
            if (n < k)
                return 0;
            BigInteger result = 1;
            BigInteger d = 1;
            for (BigInteger i = 0; i < k; i++)
            {
                result *= n - i;
                d *= i + 1;
            }
            return result / d % m;
        }

        public static BigInteger SolveSystemOfCongruencesCRTMethod(params (BigInteger a, BigInteger m)[] system)
        {
            BigInteger M = 1, Mi, result = 0;
            for (int i = 0; i < system.Length; i++)
            {
                M *= system[i].m;
            }
            for (int i = 0; i < system.Length; i++)
            {
                Mi = M / system[i].m;
                result += system[i].a * Mi * Mi.InverseElement(system[i].m) % M;
            }
            return result % M;
        }

        public static BigInteger SolveSystemOfCongruencesCRTMethod(BigInteger[] a, BigInteger[] m)
        {
            BigInteger M = 1, Mi, result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                M *= m[i];
            }
            for (int i = 0; i < a.Length; i++)
            {
                Mi = M / m[i];
                result += a[i] * Mi * Mi.InverseElement(m[i]) % M;
            }
            return result % M;
        }

        public static BigInteger SolveSystemOfCongruencesSievingMethod(params (BigInteger a, BigInteger m)[] system)
        {
            BigInteger result = system[0].a, t = system[0].m;
            for (int i = 1; i < system.Length; i++)
            {
                while (result % system[i].m != system[i].a)
                    result += t;
                t *= system[i].m;
            }
            return result;
        }

        public static BigInteger SolveSystemOfCongruencesSievingMethod(BigInteger[] a, BigInteger[] m)
        {
            BigInteger result = a[0], t = m[0];
            for (int i = 1; i < a.Length; i++)
            {
                while (result % m[i] != a[i])
                    result += t;
                t *= m[i];
            }
            return result;
        }

        public static BigInteger SolveSystemOfCongruencesGarnerMethod(params (BigInteger a, BigInteger m)[] system)
        {
            BigInteger result = 0, m = 1;
            BigInteger[] x = new BigInteger[system.Length];
            for (int i = 0; i < system.Length; ++i)
            {
                x[i] = system[i].a;
                for (int j = 0; j < i; ++j)
                {
                    x[i] = system[j].m.InverseElement(system[i].m) * (x[i] - x[j]);

                    x[i] = x[i] % system[i].m;
                    if (x[i] < 0)
                        x[i] += system[i].m;
                }
                result += m * x[i];
                m *= system[i].m;
            }
            return result % m;
        }

        public static BigInteger SolveSystemOfCongruencesGarnerMethod(BigInteger[] a, BigInteger[] m)
        {
            BigInteger result = 0, n = 1;
            BigInteger[] x = new BigInteger[a.Length];
            for (int i = 0; i < a.Length; ++i)
            {
                x[i] = a[i];
                for (int j = 0; j < i; ++j)
                {
                    x[i] = m[j].InverseElement(m[i]) * (x[i] - x[j]);

                    x[i] = x[i] % m[i];
                    if (x[i] < 0)
                        x[i] += m[i];
                }
                result += n * x[i];
                n *= m[i];
            }
            return result % n;
        }

        public static (BigInteger a, BigInteger b) CornacchiasAlgorithm(BigInteger d, BigInteger p)
        {
            var t = p - d;
            (BigInteger a, BigInteger b) r = TonelliShanksMethod(t, p);
            r.b = BigInteger.Min(r.a, r.b);
            if (r.a == 0)
                return (0, 0);
            r.a = p;
            var sqrtp = p.FloorSqrt();
            while (r.b > sqrtp)
                r = (r.b, r.a % r.b);
            t = p - BigInteger.Pow(r.b, 2);
            if (t % d != 0)
                return (0, 0);
            if (!(t / d).IsSqrt())
                return (0, 0);
            return (r.b, (t / d).FloorSqrt());
        }

        public static (BigInteger, BigInteger) PocklingtonMethod(BigInteger a, BigInteger p)
        {
            if (LegendreSymbol(a, p) == -1)
                return (0, 0);
            if (p == 2)
                return (a % p, a % p);
            if (p % 4 == 3)
            {
                if (p == 3)
                    return (a, a);
                BigInteger m = p - 3 >> 2;
                BigInteger root = BigInteger.ModPow(a, m + 1, p);
                return (root, p - root);
            }
            if (p % 8 == 5)
            {
                BigInteger m = p - 5 >> 3;
                if (BigInteger.ModPow(a, m << 1 + 1, p) == 1)
                {
                    var t = BigInteger.ModPow(a, m + 1, p);
                    return (t, p - t);
                }
                BigInteger y = BigInteger.ModPow(a << 2, m + 1, p);
                if (y % 2 == 0)
                    return (y >> 1, p - (y >> 1));
                return (p + y >> 1, p - y >> 1);
            }

            BigInteger d;
            for (d = 2; d < p; d++)
            {
                if (LegendreSymbol(d, p) == -1)
                    break;
            }
            BigInteger T = p - 1;
            int s = 0;
            do
            {
                T >>= 1;
                s++;
            } while (T % 2 == 0);
            var A = BigInteger.ModPow(a, T, p);
            var D = BigInteger.ModPow(d, T, p);
            BigInteger mi = 0;
            for (int i = 0; i < s; i++)
            {
                if (BigInteger.ModPow(A * D, mi * (1 << s - 1 - i), p) == p - 1)
                    mi += 1 << i;
            }
            var x = BigInteger.ModPow(a, T + 1 >> 1, p) * BigInteger.ModPow(D, mi >> 1, p) % p;
            return (x, p - x);
        }

        public static (BigInteger, BigInteger) TonelliShanksMethod(BigInteger a, BigInteger p)
        {
            if (LegendreSymbol(a, p) != 1)
                return (0, 0);
            if (p == 2)
                return (a % p, a % p);
            BigInteger Q = p - 1;
            BigInteger S = 0;
            do
            {
                Q >>= 1;
                S++;
            } while (Q % 2 == 0);
            BigInteger z = 2;
            while (LegendreSymbol(z, p) != -1)
                z++;
            BigInteger M = S;
            BigInteger c = BigInteger.ModPow(z, Q, p);
            BigInteger t = BigInteger.ModPow(a, Q, p);
            BigInteger R = BigInteger.ModPow(a, Q + 1 >> 1, p);
            while (true)
            {
                if (t == 0)
                    return (0, 0);
                if (t == 1)
                    return (R, p - R);
                int i = 1;
                while (BigInteger.ModPow(t, 1 << i, p) != 1)
                    i++;

                BigInteger b = BigInteger.ModPow(c, 1 << (int)(M - i - 1), p);
                M = i;
                c = BigInteger.ModPow(b, 2, p);
                t = t * c % p;
                R = R * b % p;
            }
        }

        public static (BigInteger, BigInteger) CipollaMethod(BigInteger a, BigInteger p)
        {
            if (LegendreSymbol(a, p) != 1)
                return (0, 0);
            BigInteger t = 0;
            BigInteger f = BigInteger.Pow(t, 2) - a + p;
            while (LegendreSymbol(f, p) != -1)
            {
                f += 2 * t + 1;
                t++;
            }
            BigInteger omega = f;
            (BigInteger, BigInteger) r = (1, 0);
            (BigInteger, BigInteger) s = (t, 1);
            BigInteger nn = p + 1 >> 1;
            while (nn > 0)
            {
                if ((nn & 1) == 1)
                    r = ((r.Item1 * s.Item1 + r.Item2 * s.Item2 * omega) % p, (r.Item1 * s.Item2 + s.Item1 * r.Item2) % p);
                s = ((BigInteger.ModPow(s.Item1, 2, p) + BigInteger.ModPow(s.Item2, 2, p) * omega) % p, 2 * s.Item1 * s.Item2 % p);
                nn >>= 1;
            }
            if (r.Item2 != 0)
                return (0, 0);
            if (BigInteger.ModPow(r.Item1, 2, p) != a)
                return (0, 0);
            return (r.Item1, p - r.Item1);
        }

        public static BigInteger DescreteLogEnumerateMethod(BigInteger b, BigInteger h, BigInteger p)
        {
            if (b == 0 || h == 0)
                return b == h ? 1 : -1;
            if (h == 1)
                return 0;
            if (b == 1)
                return -1;
            BigInteger result = b;
            for (int i = 1; i < p - 1; i++)
            {
                if (result == h)
                    return i;
                result = result * b % p;
            }
            return -1;
        }
    }

}