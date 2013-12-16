﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace bakaTest
{
    class SurfaceRenderer : Control
    {
        //for y-correction output
        int ysize = 720;

        // surfaces
        private Surface surface;
        private Surface renderedSurface;

        // axes
        private Line oxAxis = new Line(0, 0, 0, unitVectorLength, 0, 0);
        private Line oyAxis = new Line(0, 0, 0, 0, unitVectorLength, 0);
        private Line ozAxis = new Line(0, 0, 0, 0, 0, unitVectorLength);

        private Line renderedOxAxis = new Line();
        private Line renderedOyAxis = new Line();
        private Line renderedOzAxis = new Line();

        // matrix
        private Matrix rotationMatrix       = new Matrix(4, 4);
        private Matrix shiftMatrix          = new Matrix(4, 4);
        private Matrix scaleMatrix          = new Matrix(4, 4);
        private Matrix transformationMatrix = new Matrix(4, 4);

        // matrices' parameters
        private const double unitVectorLength = 200;
        private double scale = 6;
        private double shiftX = 640, shiftY = 380, shiftZ = 0;
        private double rotationOX = Math.PI / 4, rotationOY = Math.PI / 4, rotationOZ = 0;

        // colors
        private Color bgColor   = Color.FromArgb(0x83, 0x94, 0x96);
        private Color oxColor   = Color.FromArgb(0x00, 0x47, 0xab);
        private Color oyColor   = Color.FromArgb(0xd3, 0x36, 0x82);
        private Color ozColor   = Color.FromArgb(0x7b, 0x3f, 0x00);
        private Color meshColor = Color.FromArgb(0x00, 0x00, 0x00);

        private List<Color> colors;

        // misc
        public bool showMesh = true;
        public bool showColors = false;

        // accessors
        public Surface Surface
        {
            set { surface = value; }
        }


        ///////////////////////////////////////////////////////////
        // Methods
        private void mtxToConsole(Matrix one)
        {
            for (int i = 0; i < one.nstr; i++)
            {
                for (int j = 0; j < one.ncol; j++)
                {
                    Console.Write(one[i][j] + " ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        private Matrix yCorrection(Matrix one)
        {
            one[0][1] = ysize - one[0][1];
            return one;
        }

        public SurfaceRenderer(Surface _surface)
        {
            surface = _surface;
            calculateColors();
            calculateTransformationMatrix();
        }

        // Matrix
        private void calculateRotationMatrix()
        {
            Matrix rotationOxMatrix = new Matrix(4, 4);
            rotationOxMatrix[0][0] = 1;
            rotationOxMatrix[1][1] = Math.Cos(rotationOX);
            rotationOxMatrix[1][2] = Math.Sin(rotationOX);
            rotationOxMatrix[2][1] = - Math.Sin(rotationOX);
            rotationOxMatrix[2][2] = Math.Cos(rotationOX);
            rotationOxMatrix[3][3] = 1;


            Matrix rotationOyMatrix = new Matrix(4, 4);
            rotationOyMatrix[0][0] = Math.Cos(rotationOY);
            rotationOyMatrix[0][2] = -Math.Sin(rotationOY);
            rotationOyMatrix[1][1] = 1;
            rotationOyMatrix[2][0] = Math.Sin(rotationOY);
            rotationOyMatrix[2][2] = Math.Cos(rotationOY);
            rotationOyMatrix[3][3] = 1;


            Matrix rotationOzMatrix = new Matrix(4, 4);
            rotationOzMatrix[0][0] = Math.Cos(rotationOZ);
            rotationOzMatrix[0][1] = Math.Sin(rotationOZ);
            rotationOzMatrix[1][0] = -Math.Sin(rotationOZ);
            rotationOzMatrix[1][1] = Math.Cos(rotationOZ);
            rotationOzMatrix[2][2] = 1;
            rotationOzMatrix[3][3] = 1;

            rotationMatrix = rotationOyMatrix * rotationOxMatrix;
            rotationMatrix = rotationMatrix * rotationOzMatrix;
        }

        private void calculateShiftMatrix()
        {
            shiftMatrix[0][0] = 1;
            shiftMatrix[1][1] = 1;
            shiftMatrix[2][2] = 1;

            shiftMatrix[3][0] = shiftX;
            shiftMatrix[3][1] = shiftY;
            shiftMatrix[3][2] = shiftZ;
            shiftMatrix[3][3] = 1;
        }

        private void calculateScaleMatrix()
        {
            scaleMatrix[0][0] = scale;
            scaleMatrix[1][1] = scale;
            scaleMatrix[2][2] = scale;
            scaleMatrix[3][3] = 1;
        }

        private void calculateTransformationMatrix()
        {
            calculateScaleMatrix();
            calculateShiftMatrix();
            calculateRotationMatrix();

            Matrix projectionOzMatrix = new Matrix(4, 4);
            projectionOzMatrix[0][0] = 1;
            projectionOzMatrix[1][1] = 1;
            projectionOzMatrix[3][3] = 1;


            transformationMatrix = rotationMatrix * projectionOzMatrix * scaleMatrix * shiftMatrix;
        }

        // Colors
        private void calculateColors()
        {
            for (int i = 0; i < surface.StepsNumber - 1; ++i)
            {

            }
            return;
        }

        // Projection
        private void projection()
        {
            // surface
            renderedSurface = new Surface(surface.CurvesNumber, surface.StepsNumber);
            for (int i = 0; i < surface.CurvesNumber; ++i)
            {
                renderedSurface.AddCurve();
                for (int j = 0; j < surface.StepsNumber; ++j)
                    renderedSurface[i].Add(yCorrection(surface[i][j] * transformationMatrix));
                
            }
            // axes
            renderedOxAxis.Begin = yCorrection(oxAxis.Begin * transformationMatrix);
            renderedOyAxis.Begin = yCorrection(oyAxis.Begin * transformationMatrix);
            renderedOzAxis.Begin = yCorrection(ozAxis.Begin * transformationMatrix);

            renderedOxAxis.End = yCorrection(oxAxis.End * transformationMatrix);
            renderedOyAxis.End = yCorrection(oyAxis.End * transformationMatrix);
            renderedOzAxis.End = yCorrection(ozAxis.End * transformationMatrix);
        }

        private Color getColor(int i, int j, int im, int jm)
        {
            int r = 255*i;
            r /= im;
            int g = 255 * j;
            g /= jm;
            int b = 255 * (i + j);
            b /= (im + jm);
            return Color.FromArgb(r, g, b);
        }

        // Drawing
        protected override void OnPaint(PaintEventArgs ev)
        {
            base.OnPaint(ev);

            Graphics g = ev.Graphics;
            projection();

            // background
            Brush bgBrush = new SolidBrush(bgColor);
            g.FillRectangle(bgBrush, 0, 0, this.Width, this.Height);

            // axes
            g.DrawLine(new Pen(oxColor),
                (int)renderedOxAxis.Begin[0][0],
                (int)renderedOxAxis.Begin[0][1],
                (int)renderedOxAxis.End[0][0],
                (int)renderedOxAxis.End[0][1]);

            g.DrawLine(new Pen(oyColor),
                (int)renderedOyAxis.Begin[0][0],
                (int)renderedOyAxis.Begin[0][1],
                (int)renderedOyAxis.End[0][0],
                (int)renderedOyAxis.End[0][1]);

            g.DrawLine(new Pen(ozColor),
                (int)renderedOzAxis.Begin[0][0],
                (int)renderedOzAxis.Begin[0][1],
                (int)renderedOzAxis.End[0][0],
                (int)renderedOzAxis.End[0][1]);

            // surface
            Pen meshPen = new Pen(meshColor);

            // surface: mesh
            if (showMesh)
            {
                // mesh: along curves
                for (int i = 0; i < renderedSurface.CurvesNumber; ++i)
                    for (int j = 0; j < renderedSurface.StepsNumber - 1; ++j)
                    {
                        g.DrawLine(meshPen,
                            (int)renderedSurface[i][j][0][0],
                            (int)renderedSurface[i][j][0][1],
                            (int)renderedSurface[i][j + 1][0][0],
                            (int)renderedSurface[i][j + 1][0][1]);
                    }

                // mesh: across curves 
                for (int i = 0; i < renderedSurface.StepsNumber; ++i)
                    for (int j = 0; j < renderedSurface.CurvesNumber - 1; ++j)
                    {
                        g.DrawLine(meshPen,
                            (int)renderedSurface[j][i][0][0],
                            (int)renderedSurface[j][i][0][1],
                            (int)renderedSurface[j + 1][i][0][0],
                            (int)renderedSurface[j + 1][i][0][1]);
                    }
            }

            // surface: colors
            if (showColors)
            {
                for(int i = 0; i < renderedSurface.CurvesNumber - 1; ++i)
                    for (int j = 0; j < renderedSurface.StepsNumber - 1; ++j)
                    {
                        SolidBrush polyBrush = new SolidBrush(getColor(i, j, renderedSurface.CurvesNumber, renderedSurface.StepsNumber));
                        Point [] p = new Point[4];
                        p[0] = new Point((int) renderedSurface[i][j][0][0], (int) renderedSurface[i][j][0][1]);
                        p[0] = new Point((int) renderedSurface[i][j+1][0][0], (int) renderedSurface[i][j+1][0][1]);
                        p[0] = new Point((int) renderedSurface[i+1][j+1][0][0], (int) renderedSurface[i+1][j+1][0][1]);
                        p[0] = new Point((int) renderedSurface[i+1][j][0][0], (int) renderedSurface[i+1][j][0][1]);
                        g.FillPolygon(polyBrush, p);
                    }
            }
        }

        // User input
        protected override void OnKeyPress(KeyPressEventArgs ev)
        {
            base.OnKeyPress(ev);
            switch (ev.KeyChar)
            {
                case (char) Keys.Up:
                    rotationOX += 0.05;
                    break;

                case (char)Keys.Down:
                    rotationOX -= 0.05;
                    break;

                case (char)Keys.Left:
                    rotationOY += 0.05;
                    break;

                case (char)Keys.Right:
                    rotationOX += 0.05;
                    break;


                case (char)Keys.Space:
                    break;

                case (char)Keys.C:
                    break;


                case (char)Keys.W:
                    shiftY += 5;
                    break;

                case (char)Keys.A:
                    shiftX -= 5;
                    break;

                case (char)Keys.S:
                    shiftY -= 5;
                    break;

                case (char)Keys.D:
                    shiftX += 5;
                    break;


                case (char)Keys.R:
                    scale += 1;
                    break;

                case (char)Keys.F:
                    scale -= 1;
                    break;

                default:
                    return;
            }

            calculateTransformationMatrix();
            this.Refresh();
        }
    }
}