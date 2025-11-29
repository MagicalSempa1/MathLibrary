using MathLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Factorization.Types
{
    public sealed record QSOptions(int B, int Safety, int BlockLen, int? DegreeOfParallelism)
    {
        public int LogScale { get; set; } = 256;
        public int LogSlack { get; set; } = 48;
        public QSLPOpt EnableLargePrimes { get; set; }
        public double LargePrimeBoundMultiplier { get; set; } = 10.0;
        public QSType Type { get; set; }
        public int BlocksPerPolynomial { get; set; } = 2;
        public int AMaxBits { get; set; } = 30;

        public static QSOptions Build(BigInteger n, QSType type)
        {
            var digits = n.DecimalDigits();
            int B;
            int safety;
            int Bl = 0;
            int Bpp;
            int Amb;

            switch (type)
            {
                case QSType.SPQS:
                    (B, safety, Bl) = digits switch
                    {
                        <= 8 => (340, 10, 128),
                        <= 9 => (400, 10, 256),
                        <= 10 => (700, 10, 256),
                        <= 11 => (800, 10, 256),
                        <= 12 => (900, 10, 256),
                        <= 13 => (950, 10, 512),
                        <= 14 => (1200, 10, 512),
                        <= 15 => (1500, 10, 512),
                        <= 16 => (1700, 10, 512),
                        <= 17 => (2200, 10, 1024),
                        <= 18 => (2800, 10, 1024),
                        <= 19 => (3400, 10, 1024),
                        <= 20 => (5000, 10, 2048),
                        <= 21 => (5500, 10, 4096),
                        <= 22 => (6000, 10, 4096),
                        <= 23 => (9000, 10, 8192),
                        <= 25 => (10500, 10, 16384),
                        <= 26 => (12500, 10, 32768),
                        <= 27 => (15500, 10, 32768),
                        <= 28 => (17000, 10, 32768),
                        <= 29 => (21000, 10, 65536),
                        <= 30 => (32000, 10, 65536),
                        <= 32 => (36000, 10, 65536),
                        <= 33 => (50000, 10, 65536),
                        <= 34 => (55000, 10, 65536),
                        _ => (70000, 30, 65536)
                    };
                    return new QSOptions(B, safety, Bl, null) { EnableLargePrimes = QSLPOpt.NoLP };
                case QSType.MPQS:
                    (B, safety, Bl, Bpp, Amb) = digits switch
                    {
                        <= 8 => (280, 10, 256, 26, 20),
                        <= 9 => (360, 10, 512, 26, 20),
                        <= 10 => (380, 10, 512, 27, 20),
                        <= 11 => (420, 10, 512, 3, 20), // now
                        <= 13 => (540, 10, 512, 3, 30),
                        <= 15 => (900, 10, 512, 3, 32),
                        <= 17 => (1500, 10, 1024, 3, 32),
                        <= 19 => (2500, 10, 2048, 3, 32),
                        <= 21 => (4000, 10, 8192, 2, 32),
                        <= 25 => (4000, 10, 16384, 2, 32),
                        _ => (70000, 30, 65536, 3, 20)
                    };
                    return new QSOptions(B, safety, Bl, null) { Type = QSType.MPQS, BlocksPerPolynomial = Bpp, AMaxBits = Amb };
                default:
                    B = 0;
                    safety = 0;
                    Bl = 0;
                    break;
            }
            return new QSOptions(B, safety, Bl, null);
        }

        public static QSOptions Build(BigInteger n, QSType type, int b, int bL, int bLC, int aMX)
        {
            return new QSOptions(b, 30, bL, null) { Type = QSType.MPQS, BlocksPerPolynomial = bLC, AMaxBits = aMX };
        }
    }
}
