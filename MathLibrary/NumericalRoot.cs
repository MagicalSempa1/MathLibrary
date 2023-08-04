using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary
{
    public static class NumericalRoot
    {
        public static double HalfDivisionMethod(Func<double, double> function, double a, double b, double epsilon)
        {
            if (function(a) == 0)
                return a;
            if (function(b) == 0)
                return b;
            while (b - a > epsilon)
            {
                var t = a + (b - a) / 2;
                if (Math.Sign(function(a)) != Math.Sign(function(t)))
                    b = t;
                else
                    a = t;
                Console.WriteLine((a + b) / 2);
            }
            return (a + b) / 2;
        }

        public static double ChordMethod(Func<double, double> function, double x0, double x1, double epsilon)
        {
            if (function(x0) == 0)
                return x0;
            if (function(x1) == 0)
                return x1;
            double x_next = 0;
            double tmp;
            do
            {
                tmp = x_next;
                x_next = x1 - function(x1) * (x0 - x1) / (function(x0) - function(x1));
                x0 = x1;
                x1 = tmp;
                Console.WriteLine(x_next);
            } while (Math.Abs(x_next - x1) > epsilon);

            return x_next;
        }

        public static double NewtonMethod(Func<double, double> function, Func<double, double> derivative, double x0, double epsilon)
        {
            if (function(x0) == 0)
                return x0;
            double x = x0;
            do
            {
                x0 = x;
                x = x - function(x) / derivative(x);
                Console.WriteLine(x);
            } while (Math.Abs(x - x0) > epsilon);
            return x;
        }

        public static double SimpleIterationMethod(Func<double, double> function, Func<double, double> lambda, double x0, double epsilon)
        {
            if (function(x0) == 0)
                return x0;
            double x = x0;
            do
            {
                x0 = x;
                x = x - lambda(x) * function(x);
                Console.WriteLine(x);
            } while (Math.Abs(x - x0) > epsilon);
            return x;
        }

        public static double GetPoint(Func<double, double> function, double x0, int n)
        {
            double x = x0;
            for (int i = 0; i < n; i++)
            {
                x = function(x);
            }
            return x;
        }
    }
}
