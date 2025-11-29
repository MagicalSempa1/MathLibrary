using MathLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Factorization.Types
{
    public sealed record MpqsOptions(
    int B,
    int Safety,
    int BlockLen,
    int BlocksPerPolynomial,
    int AMaxBits,
    int? DegreeOfParallelism)
    {
        public int LogScale { get; init; } = 256;
        public int LogSlack { get; init; } = 48;

        public QSLPOpt EnableLargePrimes { get; init; } = QSLPOpt.NoLP;

        /// <summary>
        /// Граница для больших простых: LP_bound = FB[^1] * LargePrimeBoundMultiplier.
        /// </summary>
        public double LargePrimeBoundMultiplier { get; init; } = 20.0;

        /// <summary>
        /// Стандартный билд MPQS-опций по количеству десятичных цифр числа n.
        /// Таблица — это твоя старая ветка QSType.MPQS.
        /// </summary>
        public static MpqsOptions Build(BigInteger n, QSLPOpt lpOpt = QSLPOpt.NoLP)
        {
            var digits = n.DecimalDigits();

            int B;
            int safety;
            int blockLen;
            int bpp;
            int aMaxBits;

            (B, safety, blockLen, bpp, aMaxBits) = digits switch
            {
                <= 8 => (280, 10, 256, 26, 20),
                <= 9 => (360, 10, 512, 26, 20),
                <= 10 => (380, 10, 512, 27, 20),
                <= 11 => (420, 10, 512, 3, 20),  // как у тебя "now"
                <= 13 => (540, 10, 512, 3, 30),
                <= 15 => (900, 10, 512, 3, 32),
                <= 17 => (1500, 10, 1024, 3, 32),
                <= 19 => (2500, 10, 2048, 3, 32),
                <= 21 => (4000, 10, 8192, 2, 32),
                <= 25 => (4000, 10, 16384, 2, 32),
                _ => (70000, 30, 65536, 3, 20)
            };

            var opt = new MpqsOptions(B, safety, blockLen, bpp, aMaxBits, null)
            {
                EnableLargePrimes = lpOpt,
                LargePrimeBoundMultiplier = 20.0
            };

            return opt;
        }

        /// <summary>
        /// Ручной билд MPQS-опций (например, из бенчмарка).
        /// Safety здесь зашит константой 30, но можно вынести параметром.
        /// </summary>
        public static MpqsOptions BuildCustom(
            BigInteger n,
            int b,
            int blockLen,
            int blocksPerPoly,
            int aMaxBits,
            QSLPOpt lpOpt = QSLPOpt.NoLP,
            int safety = 30)
        {
            var opt = new MpqsOptions(b, safety, blockLen, blocksPerPoly, aMaxBits, null)
            {
                EnableLargePrimes = lpOpt,
                LargePrimeBoundMultiplier = 20.0
            };

            return opt;
        }
    }
}
