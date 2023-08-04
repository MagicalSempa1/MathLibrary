using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary
{
    public static class NumericalDifferentiation
    {
        public static double ForwardDiff(this Func<double, double> func, double x_0, double h) => (func(x_0 + h) - func(x_0)) / h;

        public static double ForwardDiff(this Func<double, double> func, double x_0)
        {
            double t = 1E-8 * x_0;
            double x1 = x_0 + t;
            double dx = x1 - x_0;
            return (func(x1) - func(x_0)) / dx;
        }

        public static double BackwardDiff(this Func<double, double> func, double x_0, double h) => (func(x_0) - func(x_0 - h)) / h;

        public static double BackwardDiff(this Func<double, double> func, double x_0)
        {
            double t = 1E-8 * x_0;
            double x1 = x_0 - t;
            double dx = x1 - x_0;
            return (func(x_0) - func(x1)) / dx;
        }

        public static double CentralDiff(this Func<double, double> func, double x_0, double h) => (func(x_0 + h) - func(x_0 - h)) / (2 * h);

        public static double CentralDiff(this Func<double, double> func, double x_0)
        {
            double t = 1E-8 * x_0;
            double x1 = x_0 + t;
            double dx = x1 - x_0;
            x_0 = x1 - t;
            return (func(x1) - func(x_0)) / (2 * dx);
        }

        public static double[] EulerMethod(Func<double, double, double> function, double a, double b, double h, double y0)
        {
            int n = (int)Math.Ceiling(Math.Abs(b - a) / h) + 1;
            var Y = new double[n];
            Y[0] = y0;
            double x = a;
            for (int i = 1; i < Y.Length; i++)
            {
                Y[i] = Y[i - 1] + h * function(x, Y[i - 1]);
                x += h;
            }
            return Y;
        }

        public static double[] ModyfiedEulerMethod(Func<double, double, double> function, double a, double b, double h, double y0)
        {
            int n = (int)Math.Ceiling(Math.Abs(b - a) / h) + 1;
            var Y = new double[n];
            Y[0] = y0;
            double x = a;
            for (int i = 1; i < Y.Length; i++)
            {
                Y[i] = Y[i - 1] + h * function(x, Y[i - 1]);
                Y[i] = Y[i - 1] + h * (function(x, Y[i - 1]) + function(x + h, Y[i])) / 2;
                x += h;
            }
            return Y;
        }

        public static double[] RungeKuttaMethod(Func<double, double, double> function, double a, double b, int n, double y0)
        {
            // y' = f(x, y)
            // y(a) = y0
            // O(h^4)
            double h = (b - a) / n;
            var Y = new double[n + 1];
            Y[0] = y0;
            double x = a;
            for (int i = 1; i < Y.Length; i++)
            {
                var k1 = function(x, Y[i - 1]);
                var k2 = function(x + h / 2, Y[i - 1] + h * k1 / 2);
                var k3 = function(x + h / 2, Y[i - 1] + h * k2 / 2);
                var k4 = function(x + h, Y[i - 1] + h * k3);
                Y[i] = Y[i - 1] + h * (k1 + 2 * k2 + 2 * k3 + k4) / 6;
                x += h;
            }
            return Y;
        }

        public static double[] LeftRungeKuttaMethod(Func<double, double, double> function, double a, double b, int n, double y0)
        {
            double h = (b - a) / n;
            var Y = new double[n + 1];
            Y[^1] = y0;
            double x = b;
            for (int i = Y.Length - 2; i >= 0; i--)
            {
                var k1 = function(x, Y[i + 1]);
                var k2 = function(x - h / 2, Y[i + 1] - h * k1 / 2);
                var k3 = function(x - h / 2, Y[i + 1] - h * k2 / 2);
                var k4 = function(x - h, Y[i + 1] - h * k3);
                Y[i] = Y[i + 1] - h * (k1 + 2 * k2 + 2 * k3 + k4) / 6;
                x -= h;
            }
            return Y;
        }

        public static double[] DiffMethod(Func<double, double> p, Func<double, double> q, Func<double, double> f, double a0, double b0, double A, double a1, double b1, double B, double a, double b, int n)
        {
            // y'' = p(x) * y' + q(x) * y = f(x)
            // a0 * y(a) + b0 * y'(a) = A
            // a1 * y(b) + b1 *y'(b) = B
            double h = (b - a) / n;
            double[] x = new double[n + 1];
            double[] y = new double[n + 1];
            x[0] = a;
            for (int i = 0; i < x.Length - 1; i++)
                x[i] = a + i * h;
            x[^1] = b;
            double[] c = new double[n + 1];
            double[] d = new double[n + 1];
            double[] m = new double[n + 1];
            double[] k = new double[n + 1];
            double[] F = new double[n + 1];
            c[0] = b0 / (a0 * h - b0);
            d[0] = A * h / b0;
            for (int i = 1; i < n; i++)
            {
                m[i] = (2 * h * h * q(x[i]) - 4) / (2 + h * p(x[i]));
                k[i] = (2 - h * p(x[i])) / (2 + h * p(x[i]));
                F[i] = 2 * f(x[i]) / (2 + h * p(x[i]));
                d[i] = F[i] * h * h - k[i] * c[i - 1] * d[i - 1];
                c[i] = 1 / (m[i] - k[i] * c[i - 1]);
            }
            y[^1] = (B * h + b1 * c[^2] * d[^2]) / (a1 * h + b1 * (c[^2] + 1));
            for (int i = n - 1; i >= 0; i--)
                y[i] = c[i] * (d[i] - y[i + 1]);
            return y;

        }

        public static double[] RungeKuttaMethod(Func<double, double> p, Func<double, double> q, Func<double, double> f, double a0, double b0, double A, double a1, double b1, double B, double a, double b, int n)
        {
            double h = (b - a) / n;
            double[] x = new double[n + 1];
            double[] y = new double[n + 1];
            x[0] = a;
            for (int i = 0; i < x.Length - 1; i++)
                x[i] = a + i * h;
            x[^1] = b;
            if (b0 != 0)
            {
                Func<double, double, double> f1 = (x, y) => -(y * y) - p(x) * y + q(x);
                var y1 = RungeKuttaMethod(f1, a, b, n, -a0 / b0);
                Func<double, double, double> f2 = (x, y) => -y * (y1[(int)Math.Round((x - a) / h)] + p(x)) + f(x);
                var y2 = RungeKuttaMethod(f2, a, b, n, A / b0);
                Func<double, double, double> f3 = (x, y) => y1[(int)Math.Round((x - a) / h)] * y + y2[(int)Math.Round((x - a) / h)];
                y = LeftRungeKuttaMethod(f3, b, a, n, (B - b1 * y2[^1]) / (a1 + b1 * y1[^1]));
            }
            else if (a0 != 0)
            {
                Func<double, double, double> f1 = (x, y) => -y * y * q(x) + p(x) * y + 1;
                var y1 = RungeKuttaMethod(f1, a, b, n, -b0 / a0);
                Func<double, double, double> f2 = (x, y) => -y1[(int)Math.Round((x - a) / h - 1)] * (y * q(x) + f(x));
                var y2 = RungeKuttaMethod(f2, a, b, n, A / a0);
                Func<double, double, double> f3 = (x, y) => (y - y2[(int)Math.Round((x - a) / h) - 1]) / y1[(int)Math.Round((x - a) / h - 1)];
                y = LeftRungeKuttaMethod(f3, b, a, n, (B * y1[^1] + b1 * y2[^1]) / (b1 + a1 * y1[^1]));
            }
            return y;
        }

    }
}
