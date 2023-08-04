using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary
{
    public static class Interpolation
    {
        public static Func<double, double> LinearInterpolation(Func<double, double> function, double x0, double x1) => new((x)
            => function(x0) + (function(x1) - function(x0)) * (x - x0) / (x1 - x0));

        public static Func<double, double> NewtonIntepolation(Func<double, double> function, double x0, double h, int n)
        {
            return (x) =>
            {
                double t = 0;
                for (int i = 0; i < n; i++)
                {
                    double Q = 1;
                    for (int j = 0; j < i; j++)
                    {
                        Q *= (x - j) / (h * (j + 1));
                    }
                    t += function.ForwardFiniteDifference(i, 0, x0, h, i) * Q;
                }
                return t;
            };
        }

        public static Func<double, double> LagrangeIntepolation(Func<double, double> function, params double[] X)
        {
            Func<double, double> l(int j)
            {
                if (j > X.Length)
                    return (x) => 0;
                return (x) =>
                {
                    double t = 1;
                    for (int m = 0; m < X.Length; m++)
                    {
                        if (m != j)
                            t *= (x - X[m]) / (X[j] - X[m]);
                    }
                    return t;
                };
            }
            return (x) =>
            {
                double t = 0;
                for (int i = 0; i < X.Length; i++)
                {
                    t += function(X[i]) * l(i)(x);
                }
                return t;
            };
        }

        public static Func<double, double> LagrangeIntepolation1(Func<double, double> function, params double[] X)
        {
            Func<double, double> l(int j)
            {
                if (j > X.Length)
                    return (x) => 0;
                return (x) =>
                {
                    double t = 1;
                    for (int m = 0; m < X.Length; m++)
                    {
                        if (m != j)
                            t *= (x - X[m]) / (X[j] - X[m]);
                    }
                    return t;
                };
            }
            return (x) =>
            {
                double t = 0;
                for (int i = 0; i < X.Length; i++)
                {
                    t += function(X[i]) * l(i)(x);
                }
                return t;
            };
        }

        public static Func<double, double> NevilleInterpolation(Func<double, double> function, params double[] X)
        {
            Func<double, double> P(int i, int j)
            {
                return (x) =>
                {
                    if (i == j)
                        return function(X[i]);
                    else
                        return ((x - X[j]) * P(i, j - 1)(x) - (x - X[i]) * P(i + 1, j)(x)) / (X[i] - X[j]);
                };
            }
            return P(0, X.Length - 1);
        }

        public static Func<double, double> UniformCubicSpline(Func<double, double> function, params double[] xi)
        {
            var yi = new double[xi.Length];
            for (int i = 0; i < xi.Length; i++)
                yi[i] = function(xi[i]);
            double[][] c;
            var answer = GetCoefficients(xi, yi, out c);
            if (answer != 0)
                throw new Exception();
            return (x) =>
            {
                if (x < xi[0])
                {
                    var h0 = x - xi[0];
                    return c[3][0] + h0 * (c[2][0] + h0 * (c[1][0] + h0 * c[0][0] / 3.0) / 2.0);
                }
                for (int i = 0; i < xi.Length - 1; i++)
                {                  
                    if (x < xi[i + 1])
                    {
                        var hi = x - xi[i];
                        return c[3][i] + hi * (c[2][i] + hi * (c[1][i] + hi * c[0][i] / 3.0) / 2.0);
                    }

                }
                var h = x - xi[^1];
                return c[3][^2] + h * (c[2][^2] + h * (c[1][^2] + h * c[0][^2] / 3.0) / 2.0);
            };
        }

        public static int GetCoefficients(double[] sourceX, double[] sourceY, out double[][] coefs)
        {

            var N = sourceX.Length;
            if (sourceX.LongLength != sourceY.LongLength)
            {
                coefs = null;
                return -1;
            }

            if (sourceX.LongLength <= 3)
            {
                coefs = null;
                return -2;
            }

            long Nx = N - 1;
            double[] dx = new double[Nx];

            double[] b = new double[N];
            double[] alfa = new double[N];
            double[] beta = new double[N];
            double[] gama = new double[N];

            coefs = new double[4][];
            for (int i = 0; i < 4; i++)
                coefs[i] = new double[Nx];

            for (int i = 0; i + 1 <= Nx; i++)
            {
                dx[i] = sourceX[i + 1] - sourceX[i];
                if (dx[i] == 0.0)
                    return -1;
            }

            for (long i = 1; i + 1 <= Nx; i++)
                b[i] = 3.0 * (dx[i] * ((sourceY[i] - sourceY[i - 1]) / dx[i - 1])
                    + dx[i - 1] * ((sourceY[i + 1] - sourceY[i]) / dx[i]));

            b[0] = ((dx[0] + 2.0 * (sourceX[2] - sourceX[0])) * dx[1] * ((sourceY[1] - sourceY[0]) / dx[0]) +
                        Math.Pow(dx[0], 2.0) * ((sourceY[2] - sourceY[1]) / dx[1])) / (sourceX[2] - sourceX[0]);

            b[N - 1] = (Math.Pow(dx[Nx - 1], 2.0) * ((sourceY[N - 2] - sourceY[N - 3]) / dx[Nx - 2]) + (2.0 * (sourceX[N - 1] - sourceX[N - 3])
                + dx[Nx - 1]) * dx[Nx - 2] * ((sourceY[N - 1] - sourceY[N - 2]) / dx[Nx - 1])) / (sourceX[N - 1] - sourceX[N - 3]);

            beta[0] = dx[1];
            gama[0] = sourceX[2] - sourceX[0];
            beta[N - 1] = dx[Nx - 1];
            alfa[N - 1] = (sourceX[N - 1] - sourceX[N - 3]);
            for (long i = 1; i < N - 1; i++)
            {
                beta[i] = 2.0 * (dx[i] + dx[i - 1]);
                gama[i] = dx[i];
                alfa[i] = dx[i - 1];
            }
            double c = 0.0;
            for (int i = 0; i < N - 1; i++)
            {
                c = beta[i];
                b[i] /= c;
                beta[i] /= c;
                gama[i] /= c;

                c = alfa[i + 1];
                b[i + 1] -= c * b[i];
                alfa[i + 1] -= c * beta[i];
                beta[i + 1] -= c * gama[i];
            }

            b[N - 1] /= beta[N - 1];
            beta[N - 1] = 1.0;
            for (int i = N - 2; i >= 0; i--)
            {
                c = gama[i];
                b[i] -= c * b[i + 1];
                gama[i] -= c * beta[i];
            }

            for (long i = 0; i < Nx; i++)
            {
                double dzzdx = (sourceY[i + 1] - sourceY[i]) / Math.Pow(dx[i], 2.0) - b[i] / dx[i];
                double dzdxdx = b[i + 1] / dx[i] - (sourceY[i + 1] - sourceY[i]) / Math.Pow(dx[i], 2.0);
                coefs[0][i] = (dzdxdx - dzzdx) / dx[i];
                coefs[1][i] = (2.0 * dzzdx - dzdxdx);
                coefs[2][i] = b[i];
                coefs[3][i] = sourceY[i];
            }
            return 0;
            // (x, y)
        }
    }
}
