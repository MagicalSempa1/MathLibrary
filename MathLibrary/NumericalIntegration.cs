using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary
{
    public static class NumericalIntegration
    {
        public static double LeftRectangleMethod(Func<double, double> function, double a, double b, int N)
        {
            double h = (b - a) / N;
            double S = 0;
            for (int i = 0; i < N; i++) S += function(a + i * h);
            return h * S;
        }

        public static double RightRectangleMethod(Func<double, double> function, double a, double b, int N)
        {
            double h = (b - a) / N;
            double S = 0;
            for (int i = 1; i <= N; i++) S += function(a + i * h);
            return h * S;
        }

        public static double MediumRectangleMethod(Func<double, double> function, double a, double b, int N)
        {
            double h = (b - a) / N;
            double S = 0;
            for (int i = 0; i < N; i++) S += function(a + (i + 0.5) * h);
            return h * S;
        }

        public static double TrapezoidalMethod(Func<double, double> function, double a, double b, int N)
        {
            double h = (b - a) / N;
            double S = (function(a) + function(b)) / 2;
            for (int i = 1; i < N; i++) S += function(a + i * h);
            return h * S;
        }

        public static double SimpsonMethod(Func<double, double> function, double a, double b, int N)
        {
            if ((N & 1) == 1)
                throw new ArgumentException("N должно быть чётным!!!");
            double h = (b - a) / N;
            double S = function(a) + function(b);
            for (int i = 1; i <= (N >> 1) - 1; i++)
                S += 2 * function(a + 2 * i * h) + 4 * function(a + (2 * i - 1) * h);
            return h * (S + 4 * function(b - h)) / 3;
        }

        public static double RombergMethod(Func<double, double> f, double a, double b, int n, int m)
        {
            if (n < 0) throw new ArgumentOutOfRangeException(nameof(n), "n должно быть ≥ 0");
            if (m < 0) throw new ArgumentOutOfRangeException(nameof(m), "m должно быть ≥ 0");
            if (a == b) return 0.0;

            if (m > n) m = n;

            if (n > 30) throw new ArgumentOutOfRangeException(nameof(n), "n > 30 приведёт к переполнению 1 << (i-1)");

            var prev = new double[m + 1];
            var curr = new double[m + 1];

            double h = b - a;
            double fa = f(a), fb = f(b);
            prev[0] = 0.5 * h * (fa + fb);

            for (int i = 1; i <= n; i++)
            {
                h *= 0.5;
                int numNewPts = 1 << (i - 1);
                double sum = 0.0;

                for (int k = 1; k <= numNewPts; k++)
                    sum += f(a + (2 * k - 1) * h);

                curr[0] = 0.5 * prev[0] + h * sum;

                double pow4 = 1.0;
                int maxJ = Math.Min(i, m);
                for (int j = 1; j <= maxJ; j++)
                {
                    pow4 *= 4.0;
                    curr[j] = curr[j - 1] + (curr[j - 1] - prev[j - 1]) / (pow4 - 1.0);
                }

                (curr, prev) = (prev, curr);
            }

            return prev[m];
        }
    }

}
