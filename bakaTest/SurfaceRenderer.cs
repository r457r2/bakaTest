using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace bakaTest
{
    class SurfaceRenderer : Control
    {
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

        // matrices
        private Matrix rotationMatrix       = new Matrix(4, 4);
        private Matrix shiftMatrix          = new Matrix(4, 4);
        private Matrix scaleMatrix          = new Matrix(4, 4);
        private Matrix transformationMatrix = new Matrix(4, 4);

        // matrices' parameters
        private const double unitVectorLength = 10;
        private double scale = 6;
        private double shiftX, shiftY, shiftZ;
        private double rotationOX, rotationOZ;

        // colors
        private Color bgColor   = Color.FromArgb(0x83, 0x94, 0x96);
        private Color oxColor   = Color.FromArgb(0x2a, 0xa1, 0x98);
        private Color oyColor   = Color.FromArgb(0xd3, 0x36, 0x82);
        private Color ozColor   = Color.FromArgb(0xb5, 0x89, 0x00);
        private Color meshColor = Color.FromArgb(0x85, 0x99, 0x00);

        private List<Color> colors;

        // misc
        public bool showMesh = false;
        public bool showColors = true;

        // accessors
        public Surface Surface
        {
            set { surface = value; }
        }


        ///////////////////////////////////////////////////////////
        // Methods

        public SurfaceRenderer(Surface surface_)
        {
            surface = surface_;
            calculateColors();
            calculateTransformationMatrix();
        }

        // Matrices
        private void calculateRotationMatrix()
        {
        }

        private void calculateShiftMatrix()
        {
        }

        private void calculateScaleMatrix()
        {
            // to r457r: here you should modify necessary fields of matrix
            // using scale variable
            // same to all calculate*Matrix methods
        }

        private void calculateTransformationMatrix()
        {
            // not sure about order of multiplication
            // to r457r: check it
            transformationMatrix = scaleMatrix * shiftMatrix * rotationMatrix;
        }

        // Colors
        private void calculateColors()
        {
            for (int i = 0; i < surface.StepsNumber - 1; ++i)
            {
                return;
            }
        }

        // Projection
        private void project()
        {
            // surface
            renderedSurface = new Surface(surface.CurvesNumber, surface.StepsNumber);
            for (int i = 0; i < surface.CurvesNumber; ++i)
            {
                renderedSurface.AddCurve();
                for (int j = 0; j < surface.StepsNumber; ++j)
                    renderedSurface[i].Add(surface[i][j] * transformationMatrix);
            }

            // axes
            renderedOxAxis.Begin = oxAxis.Begin * transformationMatrix;
            renderedOyAxis.Begin = oyAxis.Begin * transformationMatrix;
            renderedOzAxis.Begin = ozAxis.Begin * transformationMatrix;

            renderedOxAxis.End = oxAxis.End * transformationMatrix;
            renderedOyAxis.End = oyAxis.End * transformationMatrix;
            renderedOzAxis.End = ozAxis.End * transformationMatrix;
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
            project();

            // background
            Brush bgBrush = new SolidBrush(bgColor);
            g.FillRectangle(bgBrush, 0, 0, this.Width, this.Height);

            // axes
            g.DrawLine(new Pen(oxColor),
                (int) renderedOxAxis.Begin[0][0],
                (int) renderedOxAxis.Begin[0][1],
                (int) renderedOxAxis.End[0][0],
                (int) renderedOxAxis.End[0][1]);

            g.DrawLine(new Pen(oxColor),
                (int)renderedOxAxis.Begin[0][0],
                (int)renderedOxAxis.Begin[0][1],
                (int)renderedOxAxis.End[0][0],
                (int)renderedOxAxis.End[0][1]);

            g.DrawLine(new Pen(oxColor),
                (int)renderedOxAxis.Begin[0][0],
                (int)renderedOxAxis.Begin[0][1],
                (int)renderedOxAxis.End[0][0],
                (int)renderedOxAxis.End[0][1]);

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
                    break;

                case (char)Keys.Down:
                    break;

                case (char)Keys.Left:
                    break;

                case (char)Keys.Right:
                    break;


                case (char)Keys.Space:
                    break;

                case (char)Keys.C:
                    break;


                case (char)Keys.W:
                    break;

                case (char)Keys.A:
                    break;

                case (char)Keys.S:
                    break;

                case (char)Keys.D:
                    break;


                case (char)Keys.R:
                    // change scale variable
                    // call calculate*
                    break;

                case (char)Keys.F:
                    break;

                default:
                    return;
            }

            calculateTransformationMatrix();
            this.Refresh();
        }
    }
}
