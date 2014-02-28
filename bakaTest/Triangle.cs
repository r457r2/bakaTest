using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace bakaTest
{
    class Triangle
    {
        public Matrix a;
        public Matrix b;
        public Matrix c;
        public Matrix inner;

        private Color color;

        public Color getColor
        {
            get { return color; }
        }

        public Triangle()
        {
            a = new Matrix(1, 4);
            b = new Matrix(1, 4);
            c = new Matrix(1, 4);

            color = Color.FromArgb(0xff, 0xff, 0xff);
        }

        public Triangle(Matrix pa, Matrix pb, Matrix pc)
        {
            a = pa;
            b = pb;
            c = pc;

            colorFromNormal();
            findInnerPoint();
        }

        public Triangle(Matrix pa, Matrix pb, Matrix pc, Color col)
        {
            a = pa;
            b = pb;
            c = pc;

            color = col;
            findInnerPoint();
        }

        private int invertIfNegative(int value)
        {
            if (value < 0)
                return 0; //return 255 + value;
            return value;
        }

        public void findInnerPoint()
        {
            Matrix mid = a - b;

            mid = 0.5 * mid;
            mid = a - mid;

            inner = c - mid;
            inner = 0.5 * inner;
            inner = c - inner;
        }

        public void colorFromNormal()
        {
            Matrix v1 = a - b;
            Matrix v2 = a - c;

            double x = v1[0][1] * v2[0][2] - v1[0][2] * v2[0][1];
            double y = v1[0][2] * v2[0][0] - v1[0][0] * v2[0][2];
            double z = v1[0][0] * v2[0][1] - v1[0][1] * v2[0][0];
            double length = Math.Sqrt(x * x + y * y + z * z);

            int r = (int)(x / length * 255);
            int g = (int)(y / length * 255);
            int bl = (int)(z / length * 255);

            r = invertIfNegative(r);
            g = invertIfNegative(g);
            bl = invertIfNegative(bl);

            color = Color.FromArgb(r, g, bl);
        }

        public static Triangle operator*(Triangle t, Matrix m)
        {
            return new Triangle(t.a * m, t.b * m, t.c * m, t.color);
        }
    }
}
