﻿using System;
using System.Collections.Generic;

namespace bakaTest
{
    // Surface class
    // An ordered set of 3d-points
    // Surface consists of curves
    // Each curve is a shifted (by z) copy of original curve and consists of points
    // Original curve is a 2d function's graph (in plane xOy, so z coordinate is fixed)
    public class Surface
    {
        private int ncurves, nsteps;
        private List<List<Matrix>> curves = null;

        public int CurvesNumber
        {
            get { return ncurves; }
        }

        public int StepsNumber
        {
            get { return nsteps; }
        }

        public List<Matrix> this[int curve]
        {
            get { return curves[curve]; }
        }

        public void AddCurve()
        {
            curves.Add(new List<Matrix>());
        }

        public Surface(int _ncurves, int _nsteps)
        {
            ncurves = _ncurves;
            nsteps = _nsteps;
            curves = new List<List<Matrix>>();
        }

        public Surface(int _ncurves, double zshift, double zinitial, int _nsteps, double xstep, double xinitial, double angle, Func<double, double> func)
        {
            ncurves = _ncurves;
            nsteps = _nsteps;
            curves = new List<List<Matrix>>();

            // calculating initial curve
            List<Matrix> curve = new List<Matrix>();
            double x = xinitial;
            for (int i = 0; i < nsteps; ++i, x += xstep)
            {
                Matrix m = new Matrix(1, 4);
                m[0][0] = x;
                m[0][1] = func(x);
                m[0][2] = zinitial;
                m[0][3] = 1;
                curve.Add(m);
            }
            curves.Add(curve);

            // calculating copies
            double xshift = (zshift / Math.Sin(angle)) * Math.Cos(angle);
            for (int i = 1; i < ncurves; ++i)
            {
                curve = new List<Matrix>();
                for (int j = 0; j < nsteps; ++j)
                {
                    Matrix m = new Matrix(1, 4);
                    m[0][0] = curves[i - 1][j][0][0] + xshift;
                    m[0][1] = curves[i - 1][j][0][1];
                    m[0][2] = curves[i - 1][j][0][2] + zshift;
                    m[0][3] = 1;
                    curve.Add(m);
                }
                curves.Add(curve);
            }
        }
    }
}