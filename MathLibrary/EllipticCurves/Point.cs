using MathLibrary.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.EllipticCurves
{
    public struct Point
    {
        public BigInteger X { get; }
        public BigInteger Y { get; }
        public bool IsInfinity { get; }

        public Point(BigInteger x, BigInteger y, bool isinf = false)
        {
            X = x;
            Y = y;
            IsInfinity = isinf;
        }

        public static Point Infinity() => new Point(default, default, true);

        public static Point Add(Point p1, Point p2, BigInteger a, BigInteger n)
        {
            if (p1.IsInfinity) return p2;
            if (p2.IsInfinity) return p1;

            BigInteger m;
            if (p1.X == p2.X)
            {
                if ((p1.Y + p2.Y) % n == 0)
                    return Infinity();
                m = (3 * p1.X * p1.X + a) * (2 * p1.Y).Inverse(n);
            }
            else
                m = (p2.Y - p1.Y) * (p2.X - p1.X).Inverse(n);

            BigInteger x = (m * m - p1.X - p2.X) % n;
            BigInteger y = (m * (p1.X - x) - p1.Y) % n;

            if (x < 0)
                x += n;
            if (y < 0)
                y += n;

            return new Point(x, y);
        }

        public static Point Multiply(Point p, BigInteger k, BigInteger a, BigInteger n)
        {
            Point result = Infinity();
            Point addend = new Point(p.X, p.Y);

            while (k > 0)
            {
                if (k % 2 == 1)
                    result = Add(result, addend, a, n);
                addend = Add(addend, addend, a, n);
                k /= 2;
            }

            return result;
        }

        public override string ToString()
        {
            if (IsInfinity)
                return "Infinity";
            return $"({X}, {Y})";
        }
    }
}
