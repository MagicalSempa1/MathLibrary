using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Extensions
{
    public static partial class Extensions
    {
        //public static int[] Remainders(this BigInteger n, int[] m)
        //{
        //    if (m == null) throw new ArgumentNullException(nameof(m));
        //    int k = m.Length;
        //    if (k == 0) return Array.Empty<int>();

        //    // листья product tree
        //    var leaves = new BigInteger[k];
        //    for (int i = 0; i < k; i++)
        //        leaves[i] = m[i]; // m[i] > 0 по договорённости

        //    // product tree: levels[0] — листья; levels[^1][0] — произведение всех модулей
        //    var levels = new List<BigInteger[]>
        //    {
        //        leaves
        //    };

        //    while (levels[^1].Length > 1)
        //    {
        //        var prev = levels[^1];
        //        int len = prev.Length;
        //        int nextLen = (len + 1) / 2;

        //        var next = new BigInteger[nextLen];
        //        for (int j = 0; j < nextLen; j++)
        //        {
        //            int l = (j << 1);
        //            int r = l + 1;
        //            next[j] = (r < len) ? prev[l] * prev[r] : prev[l];
        //        }
        //        levels.Add(next);
        //    }

        //    // remainder tree
        //    var rems = new BigInteger[levels.Count][];
        //    rems[^1] = [n % levels[^1][0]]; // n > 0, модуль > 0 -> остаток в диапазоне [0, m)

        //    for (int level = levels.Count - 2; level >= 0; level--)
        //    {
        //        var prods = levels[level];
        //        var parents = rems[level + 1];
        //        var cur = new BigInteger[prods.Length];

        //        for (int j = 0; j < prods.Length; j++)
        //        {
        //            // родитель для узла j — j/2 на уровне выше
        //            cur[j] = parents[j >> 1] % prods[j];
        //        }
        //        rems[level] = cur;
        //    }

        //    // листья — искомые n mod m[i]
        //    var result = new int[k];
        //    for (int i = 0; i < k; i++)
        //        result[i] = (int)rems[0][i]; // безопасно: 0 <= r < m[i] <= int.MaxValue

        //    return result;
        //}

        public static int[] Remainders1(this BigInteger n, int[] m)
        {
            ArgumentNullException.ThrowIfNull(m);
            var result = new int[m.Length];

            for (int i = 0; i < m.Length; i++)
                result[i] = (int)(n % m[i]);

            return result;
        }

        public static int[] Remainders2(this BigInteger n, int[] m)
        {
            ArgumentNullException.ThrowIfNull(m);
            int k = m.Length;
            if (k == 0) return Array.Empty<int>();

            // --- Product tree: levels[0] — листья; levels[^1][0] — общий продукт ---
            var levels = new List<BigInteger[]>(capacity: 32);

            var leaves = new BigInteger[k];
            for (int i = 0; i < k; i++)
            {
                int mi = m[i];
                if (mi <= 0) throw new ArgumentException("Все модули должны быть положительными.", nameof(m));
                leaves[i] = mi; // BigInteger <- int
            }
            levels.Add(leaves);

            while (levels[^1].Length > 1)
            {
                var prev = levels[^1];
                int len = prev.Length;
                int nextLen = (len + 1) >> 1;

                var next = new BigInteger[nextLen];
                for (int j = 0; j < nextLen; j++)
                {
                    int l = j << 1;
                    int r = l + 1;
                    next[j] = (r < len) ? prev[l] * prev[r] : prev[l];
                }
                levels.Add(next);
            }

            // Корень: один остаток n mod (произведение всех модулей)
            var remLevels = new BigInteger[levels.Count][];
            remLevels[^1] = [n % levels[^1][0]];

            // --- Remainder tree: спускаем остатки вниз по уровням ---
            for (int level = levels.Count - 2; level >= 0; level--)
            {
                var prods = levels[level];
                var parents = remLevels[level + 1];

                int len = prods.Length;
                var cur = new BigInteger[len];
                for (int j = 0; j < len; j++)
                {
                    // Родитель для j — это j/2 на уровне выше
                    cur[j] = parents[j >> 1] % prods[j];
                }
                remLevels[level] = cur;
            }

            // Листья remainder-дерева — искомые n mod m[i]
            var res = new int[k];
            var leafRem = remLevels[0];
            for (int i = 0; i < k; i++)
                res[i] = (int)leafRem[i]; // 0 <= r < m[i] <= int.MaxValue

            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static int[] Remainders3(this BigInteger n, int[] m)
        {
            ArgumentNullException.ThrowIfNull(m);
            int k = m.Length;
            if (k == 0) return Array.Empty<int>();

            // Размер дерева: ближайшая степень двойки >= k
            int N = 1;
            while (N < k) N <<= 1;

            int baseIdx = N;
            int treeSize = 2 * N;

            // products[1] — корень; leaves — в [N .. N+N-1]
            var products = new BigInteger[treeSize];
            // Инициализируем листья: заполняем реальные модули, «хвост» — единицами
            for (int i = 0; i < N; i++)
                products[baseIdx + i] = (i < k) ? (BigInteger)m[i] : BigInteger.One;

            // Строим product tree снизу вверх
            for (int i = N - 1; i >= 1; --i)
            {
                int left = i << 1;
                int right = left + 1;
                products[i] = products[left] * products[right];
            }

            // remainder tree в таком же плоском виде
            var rem = new BigInteger[treeSize];
            rem[1] = n % products[1];

            // Спускаем остатки: rem[left] = rem[parent] % product[left], и аналогично для right
            for (int i = 1; i < N; ++i)
            {
                var parentRem = rem[i];
                int left = i << 1;
                int right = left + 1;

                rem[left] = parentRem % products[left];
                rem[right] = parentRem % products[right];
            }

            // Считываем ответы с листьев
            var res = new int[k];
            for (int i = 0; i < k; i++)
                res[i] = (int)rem[baseIdx + i]; // 0 <= r < m[i] <= int.MaxValue

            return res;
        }
    }
}
