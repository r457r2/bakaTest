using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bakaTest
{
    class Line
    {
        private Matrix begin;
        private Matrix end;

        public Matrix Begin
        {
            get { return begin; }
            set { begin = value; }
        }

        public Matrix End
        {
            get { return end; }
            set { end = value; }
        }

        public Line()
        {
            begin = new Matrix(1, 4);
            end   = new Matrix(1, 4);
        }

        public Line(double x1, double y1, double z1, double x2, double y2, double z2)
        {
            begin = new Matrix(1, 4);
            begin[0][0] = x1;
            begin[0][0] = y1;
            begin[0][0] = z1;
            begin[0][0] = 1;

            end = new Matrix(1, 4);
            end[0][0] = x2;
            end[0][0] = y2;
            end[0][0] = z2;
            end[0][0] = 1;
        }
    }
}
