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
            for (int i = 0; i < N; i++)
                S += function(a + i * h);
            return h * S;
        }

        public static double RightRectangleMethod(Func<double, double> function, double a, double b, int N)
        {
            double h = (b - a) / N;
            double S = 0;
            for (int i = 1; i <= N; i++)
                S += function(a + i * h);
            return h * S;
        }

        public static double MediumRectangleMethod(Func<double, double> function, double a, double b, int N)
        {
            double h = (b - a) / N;
            double S = 0;
            for (int i = 0; i < N; i++)
                S += function(a + (i + 0.5) * h);
            return h * S;
        }

        public static double TrapezoidalMethod(Func<double, double> function, double a, double b, int N)
        {
            double h = (b - a) / N;
            double S = (function(a) + function(b)) / 2;
            for (int i = 1; i < N; i++)
                S += function(a + i * h);
            return h * S;
        }

        public static double SimpsonMethod(Func<double, double> function, double a, double b, int N)
        {
            if (N % 2 == 1)
                throw new Exception("N должно быть чётным!!!");
            double h = (b - a) / N;
            double S = function(a) + function(b);
            for (int i = 1; i <= (N >> 1) - 1; i++)
                S += 2 * function(a + 2 * i * h) + 4 * function(a + (2 * i - 1) * h);
            return h * (S + 4 * function(b - h)) / 3;
        }

        public static double RombergMethod(Func<double, double> function, double a, double b, int n, int m)
        {
            double h = (b - a) / (1 << n);
            if (m == 0)
            {
                if (n == 0)
                    return (b - a) * (function(a) + function(b)) / 2;
                else
                {
                    double s = 0;
                    for (int k = 1; k <= 1 << (n - 1); k++)
                        s += function(a + (2 * k - 1) * h);
                    return RombergMethod(function, a, b, n - 1, 0) / 2 + h * s;
                }
            }
            return RombergMethod(function, a, b, n, m - 1) + (RombergMethod(function, a, b, n, m - 1) - RombergMethod(function, a, b, n - 1, m - 1)) / (System.Math.Pow(4, m) - 1);
        }
    }

}
