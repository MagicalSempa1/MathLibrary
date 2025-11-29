using MathLibrary.Extensions;
using System.Numerics;

namespace MathLibrary.Factorization.Types
{
    public sealed record SpqsOptions(int B, int Safety, int BlockLen, int? DegreeOfParallelism)
    {
        public int LogScale { get; init; } = 256;

        public int LogSlack { get; init; } = 48;

        public QSLPOpt EnableLargePrimes { get; init; } = QSLPOpt.NoLP;

        public double LargePrimeBoundMultiplier { get; init; } = 20.0;

        public static SpqsOptions Build(BigInteger n)
        {
            var digits = n.DecimalDigits();
            int B, safety, Bl;

            (B, safety, Bl) = digits switch
            {
                <= 8 => (280, 10, 128),
                <= 9 => (360, 10, 128),
                <= 10 => (380, 10, 256),
                <= 11 => (480, 10, 256),
                <= 12 => (550, 10, 256),
                <= 13 => (800, 10, 256),
                <= 14 => (1000, 10, 512),
                <= 15 => (1400, 10, 512),
                <= 16 => (1600, 10, 1024),
                <= 17 => (2100, 10, 1024),
                <= 18 => (2300, 10, 2048),
                <= 19 => (3200, 10, 2048),
                <= 20 => (4900, 10, 2048),
                <= 21 => (5500, 10, 2048),
                <= 22 => (6700, 10, 2048),
                <= 23 => (7900, 10, 4096),
                <= 24 => (9500, 10, 4096),
                <= 25 => (10000, 10, 4096),
                <= 26 => (14000, 10, 8192),
                <= 27 => (15000, 10, 8192),
                <= 28 => (17000, 10, 8192),
                <= 29 => (21000, 10, 8192),
                <= 30 => (23000, 10, 16384),
                <= 31 => (29000, 10, 16384),
                <= 32 => (32000, 10, 16384),
                <= 33 => (32000, 10, 32768),
                <= 34 => (47000, 10, 32768),
                <= 35 => (50000, 10, 32768),
                <= 36 => (56000, 10, 65536),
                <= 37 => (66000, 10, 65536),
                <= 38 => (76000, 10, 65536),
                <= 39 => (94000, 10, 131072),
                <= 40 => (96000, 10, 131072),
                _ => (140000, 30, 262144)
            };

            return new SpqsOptions(B, safety, Bl, null);
        }
    }
}
