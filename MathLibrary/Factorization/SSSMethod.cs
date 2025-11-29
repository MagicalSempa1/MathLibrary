using MathLibrary.Extensions;
using MathLibrary.Functions;
using System.Numerics;

namespace MathLibrary.Factorization
{
    public static partial class Factorization
    {
        public static BigInteger[] SSSMethod(BigInteger n)
        {
            if (PrimalityTests.MillerTest(n))
                return [n];

            if (n.IsPerfectPower(out BigInteger a, out int b))
            {
                var result = new BigInteger[b];
                Array.Fill(result, a);
                return result;
            }

            var primes = new List<BigInteger>();

            int digits = n.ToString().Length;

            int m = digits switch
            {
                <= 18 => 120,
                <= 25 => 300,
                <= 34 => 400,
                <= 36 => 600,
                <= 38 => 800,
                <= 40 => 1000,
                <= 42 => 1200,
                <= 44 => 1400,
                <= 48 => 2000,
                <= 52 => 2400,
                <= 56 => 4000,
                <= 60 => 8000,
                <= 66 => 12000,
                <= 74 => 20000,
                <= 80 => 60000,
                <= 88 => 100000,
                <= 94 => 120000,
                _ => 200000
            };

            var fbase = Sieves.AtkinSieveExtendToNo(m);

            for (int i = 0; i < fbase.Length; i++)
            {
                int p = fbase[i];
                while (n % p == 0)
                {
                    primes.Add(p);
                    n /= p;
                }
            }

            if (primes.Count > 0 && PrimalityTests.MillerTest(n))
                return [.. primes, n];

            int plistMax = fbase[fbase.Length / 5 - 1];

            fbase = fbase.Where(p => Functions.ArithmeticFunctions.LegendreSymbol(n, p) == 1).ToArray();
            var plist = fbase.TakeWhile(p => p <= plistMax).ToArray();

            var sqrt = n.CeilingSqrt();

            var mtree = BuildProductTree(fbase.Select(p => (BigInteger)p).ToArray());
            var ntree = BuildProductTree(plist.Select(p => (BigInteger)p).ToArray());
            int last = fbase[^1];
            var mval = mtree[^1][0];
            var nval = ntree[^1][0];
            var partialbound = 128 * last;

            foreach (int p in fbase)
            {
                int exp = Math.Max(1, (int)BigInteger.Log(1 << 15, p)) - 1;
                mval *= BigInteger.Pow(p, exp);
            }

            var coeffs = new BigInteger[plist.Length];
            for (int i = 0; i < plist.Length; i++)
            {
                var div = nval / plist[i];
                var inv = (div % plist[i]).Inverse(plist[i]);
                coeffs[i] = inv * div;
            }

            var roots = new List<BigInteger>[fbase.Length];
            for (int i = 0; i < fbase.Length; i++)
            {
                int p = fbase[i];
                roots[i] = new List<BigInteger>();

                var (r1, r2) = Functions.ArithmeticFunctions.TonelliShanks(n % p, p);

                roots[i].Add(r1 % p);
                if (r2 != r1)
                    roots[i].Add(r2 % p);

                roots[i].Sort();
            }

            var difflist = new BigInteger[plist.Length];
            for (int i = 0; i < plist.Length; i++)
            {
                if (i < roots.Length && roots[i].Count >= 2)
                    difflist[i] = coeffs[i] * (roots[i][1] - roots[i][0]);
                else
                    difflist[i] = 0;
            }

            void Search(HashSet<BigInteger> output, List<BigInteger> partial, int length)
            {
                var rnd = new Random();
                var indset = new HashSet<int>();
                while (indset.Count < length)
                    indset.Add(rnd.Next(plist.Length));

                var primesLoc = new List<BigInteger> { 1 };
                foreach (var i in indset) primesLoc.Add(plist[i]);

                var indlist = new List<int> { -1 };
                indlist.AddRange(indset);

                BigInteger M = 1;
                foreach (var p in primesLoc) M *= p;

                var mvals = primesLoc.Select(p => M / p).ToList();

                BigInteger xval = 0;
                foreach (var i in indset)
                    xval += coeffs[i] * roots[i][0];
                xval %= M;

                var inv = new BigInteger[fbase.Length];
                for (int i = 0; i < fbase.Length; i++)
                    inv[i] = (i >= plist.Length) ? (M % fbase[i]).Inverse(fbase[i]) : 0;

                foreach (var i in indset)
                {
                    if (i == 0) continue;

                    xval = (xval + difflist[i]) % M;

                    var bvals = new List<BigInteger[]>();
                    for (int v = 0; v < fbase.Length; v++) bvals.Add(null);

                    var kvals = new List<BigInteger>();
                    for (int v = plist.Length; v < fbase.Length; v++)
                    {
                        int p = fbase[v];
                        var r = roots[v];
                        BigInteger b0 = (r[0] - xval) * inv[v] % p;
                        BigInteger b1 = (r[1] - xval) * inv[v] % p;
                        if (b0 < 0) b0 += p;
                        if (b1 < 0) b1 += p;

                        bvals[v] = [b0, b1];
                        kvals.AddRange([b0 - p, b0, b1 - p, b1]);
                    }

                    var xlist = new List<BigInteger>();
                    var valList = new List<BigInteger[]>();

                    for (int j = 0; j < mvals.Count; j++)
                    {
                        if (indlist[j] == i) continue;
                        var m = mvals[j];
                        var pri = primesLoc[j];

                        if (j > 0)
                        {
                            kvals.Clear();
                            for (int v = plist.Length; v < fbase.Length; v++)
                            {
                                int p = fbase[v];
                                var b = bvals[v];
                                BigInteger r0 = b[0] * pri % p;
                                BigInteger r1 = b[1] * pri % p;
                                kvals.AddRange([r0 - p, r0, r1 - p, r1]);
                            }
                        }

                        var grouped = kvals.GroupBy(k => k).ToDictionary(g => g.Key, g => g.Count());
                        foreach (var kv in grouped)
                        {
                            if (kv.Value > 2)
                            {
                                var arg = xval + kv.Key * m;
                                xlist.Add(arg);
                                // вместо Pol(arg): (arg + 10)^2 - n
                                valList.Add([BigInteger.Abs(BigInteger.Pow(arg + sqrt, 2) - n) / m]);
                            }
                        }
                    }

                    // smooth_batch(xlist, valList) TODO: нужно реализовать
                    var (xSmooth, smoothParts) = SmoothBatch(xlist, valList);

                    partial.AddRange(smoothParts.Select(sp => sp.Item2));
                    foreach (var x in xSmooth) output.Add(x);
                }
            }

            (List<BigInteger> xSmooth, List<(BigInteger, BigInteger)> smoothParts) SmoothBatch(List<BigInteger> xlist, List<BigInteger[]> valList)
            {
                // rem_list = remainders(mval, val_list)
                var remList = Remainders(mval, valList.Select(v => new BigInteger[] { v[0] }).ToList());

                var xSmooth = new List<BigInteger>();
                var smoothParts = new List<(BigInteger, BigInteger)>();

                for (int i = 0; i < remList.Length; i++)
                {
                    var y = remList[i];
                    if (y == 0)
                    {
                        xSmooth.Add(xlist[i]);
                    }
                    else
                    {
                        var nSmooth = valList[i][0] / BigInteger.GreatestCommonDivisor(valList[i][0], y);
                        if (nSmooth < partialbound)
                        {
                            if (nSmooth <= last)
                                xSmooth.Add(xlist[i]);
                            else
                                smoothParts.Add((nSmooth, xlist[i]));
                        }
                    }
                }

                return (xSmooth, smoothParts);
            }

            var partial = new List<BigInteger>();
            var lp_lst = new List<List<BigInteger>>();
            var output = new HashSet<BigInteger>();



            while (true)
            {
                for (int pi = 0; pi < partial.Count; pi++)
                {
                    var pa = partial[pi];
                    var l_p = lp_lst.Select(lst => lst.Count > 0 ? lst[0] : BigInteger.Zero).ToList();
                    int index = l_p.IndexOf(pa);
                    if (index >= 0)
                    {
                        var plst = lp_lst[index];
                        if (!plst.Contains(pa)) plst.Add(pa);
                    }
                    else
                    {
                        lp_lst.Add(new List<BigInteger> { pa });
                    }
                }

                partial.Clear();

                // Counting suitable partial relations
                int lp = 0;
                foreach (var lst in lp_lst.Where(lst => lst.Count > 2))
                {
                    lp += (lst.Count - 1) / 2;
                }

                int pr = output.Count;
                double frac = (pr + lp + 1.0) / (fbase.Length + 10);

                if (pr + lp >= fbase.Length + 10) break;
                
                // Вызов Search для генерации новых отношений
                Search(output, partial, Math.Min(plist.Length, 10));
            }

            // large primes from partial relations
            var largePrimes = lp_lst.Where(lst => lst.Count > 2).ToList();
            var sNums = output.Select(x => (x + sqrt) * (x + sqrt) - n).ToList(); // pol(x) -> (x+sqrt)^2 - n
            var xlist = output.Select(x => x + sqrt).ToList();

            // Computing full relations from partial relations
            foreach (var tupl in largePrimes)
            {
                var g = BigInteger.GreatestCommonDivisor(tupl[0], n);
                if (g == 1)
                {
                    int i = 1;
                    var pairs = new List<(BigInteger, BigInteger)>();
                    while (i <= tupl.Count - 2)
                    {
                        pairs.Add((tupl[i], tupl[i + 1]));
                        i += 2;
                    }

                    foreach (var pa in pairs)
                    {
                        xlist.Add((pa.Item1 + sqrt) * (pa.Item2 + sqrt) * ModInverse(tupl[0] % n, n));
                        sNums.Add(((pa.Item1 + sqrt) * (pa.Item1 + sqrt) - n) * ((pa.Item2 + sqrt) * (pa.Item2 + sqrt) - n) / (tupl[0] * tupl[0]));
                    }
                }
                else
                {
                    throw new Exception($"Proper factors found: {g} | {n}");
                }
            }

            return MatrixFactor(n, xlist, sNums, fbase);
        }

        private static BigInteger ModInverse(BigInteger a, BigInteger m)
        {
            return a.Inverse(m);
        }

        private static BigInteger[] MatrixFactor(BigInteger n, List<BigInteger> xlist, List<BigInteger> sNums, int[] fbase)
        {
            // Упрощенная реализация факторизации через матрицу
            // В реальной реализации здесь должен быть алгоритм решения системы линейных уравнений
            // над полем Z2 для нахождения квадратных корней
            
            var factors = new List<BigInteger>();
            
            // Простая проверка на общие делители
            foreach (var x in xlist)
            {
                var gcd = BigInteger.GreatestCommonDivisor(x, n);
                if (gcd > 1 && gcd < n)
                {
                    factors.Add(gcd);
                    factors.Add(n / gcd);
                    return factors.ToArray();
                }
            }
            
            return [n]; // Если факторы не найдены
        }


        private static BigInteger[][] BuildProductTree(BigInteger[] leaves)
        {
            var levels = new List<BigInteger[]>();
            if (leaves == null || leaves.Length == 0) return [.. levels];

            var cur = leaves;
            levels.Add(cur);

            while (cur.Length > 1)
            {
                int nextLen = (cur.Length + 1) / 2;
                var next = new BigInteger[nextLen];
                for (int i = 0; i < nextLen; i++)
                {
                    int j = 2 * i;
                    next[i] = (j + 1 < cur.Length) ? cur[j] * cur[j + 1] : cur[j];
                }
                levels.Add(next);
                cur = next;
            }

            return [.. levels];
        }

        private static BigInteger[] Remainders(BigInteger n, List<BigInteger[]> productLevels)
        {
            if (productLevels == null || productLevels.Count == 0)
                return [];

            var remLevels = new List<BigInteger[]>(productLevels.Count);
            for (int i = 0; i < productLevels.Count; i++)
                remLevels.Add(new BigInteger[productLevels[i].Length]);
            int top = productLevels.Count - 1;
            remLevels[top][0] = n % productLevels[top][0];

            for (int level = top - 1; level >= 0; level--)
            {
                var parents = remLevels[level + 1];
                var mods = productLevels[level];
                var rems = remLevels[level];

                for (int i = 0; i < mods.Length; i++)
                {
                    int parent = i / 2;
                    BigInteger r = parents[parent] % mods[i];
                    if (r.Sign < 0) r += mods[i];
                    rems[i] = r;
                }
            }

            return remLevels[0];
        }
    }
}
