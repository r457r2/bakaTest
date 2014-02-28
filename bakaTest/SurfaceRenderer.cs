using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace bakaTest
{
    class SurfaceRenderer : UserControl
    {
        // bodies
        private Body body;
        private Body renderedBody;

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
        private const double unitVectorLength = 100;
        private double scale = 6;
        private double shiftX = 640, shiftY = 380, shiftZ = 0;
        private double rotationOX = Math.PI / 4, rotationOY = Math.PI / 4, rotationOZ = 0;

        // colors
        private Color bgColor   = Color.FromArgb(0x83, 0x94, 0x96);
        private Color oxColor   = Color.FromArgb(0x00, 0x47, 0xab);
        private Color oyColor   = Color.FromArgb(0xd3, 0x36, 0x82);
        private Color ozColor   = Color.FromArgb(0x7b, 0x3f, 0x00);
        private Color meshColor = Color.FromArgb(0xff, 0xff, 0xff);
        private Color debugColor= Color.FromArgb(0x00, 0x00, 0x00);

        private List<Color> colors = new List<Color>();

        // misc
        public bool showMesh = false;
        public bool showColors = true;
        public bool showAxes = false;

        // accessors
        public Body Body
        {
            set { body = value; }
        }

        ///////////////////////////////////////////////////////////
        // Methods

        private Matrix toWindow(Matrix one)
        {
            one[0][1] = this.Height - one[0][1];
            return one;
        }

        private Triangle toWindow(Triangle t)
        {
            t.a[0][1] = this.Height - t.a[0][1];
            t.b[0][1] = this.Height - t.b[0][1];
            t.c[0][1] = this.Height - t.c[0][1];
            t.inner[0][1] = this.Height - t.inner[0][1];
            return t;
        }

        public SurfaceRenderer(Body _body)
        {
            body = _body;
            calculateTransformationMatrix();

            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.UpdateStyles();
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


            transformationMatrix = rotationMatrix * scaleMatrix * shiftMatrix;
        }

        // Colors
        private int invertIfNegative(int value)
        {
            if (value < 0)
                return 0; //return 255 + value;
            return value;
        }

        // Projection
        private void projection()
        {
            // body
            renderedBody = new Body();
            for (int i = 0; i < body.TrianglesNumber; ++i)
                renderedBody.AddTriangle(toWindow(body[i] * transformationMatrix));

            // axes
            renderedOxAxis.Begin = toWindow(oxAxis.Begin * transformationMatrix);
            renderedOyAxis.Begin = toWindow(oyAxis.Begin * transformationMatrix);
            renderedOzAxis.Begin = toWindow(ozAxis.Begin * transformationMatrix);

            renderedOxAxis.End = toWindow(oxAxis.End * transformationMatrix);
            renderedOyAxis.End = toWindow(oyAxis.End * transformationMatrix);
            renderedOzAxis.End = toWindow(ozAxis.End * transformationMatrix);
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
            if(showAxes)
            {
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
            }
            
            // body
            Pen meshPen = new Pen(meshColor);
            Pen timePen = new Pen(debugColor);
            renderedBody.zsort();

            for (int i = 0; i < renderedBody.TrianglesNumber; ++i)
            {
                Point[] p = new Point[3];
                p[0] = new Point((int)renderedBody[i].a[0][0], (int)renderedBody[i].a[0][1]);
                p[1] = new Point((int)renderedBody[i].b[0][0], (int)renderedBody[i].b[0][1]);
                p[2] = new Point((int)renderedBody[i].c[0][0], (int)renderedBody[i].c[0][1]);
         
                if (showColors)
                {
                    SolidBrush polyBrush = new SolidBrush(renderedBody[i].getColor);
                    g.FillPolygon(polyBrush, p);
                }

                if (showMesh)
                    g.DrawPolygon(meshPen, p);


                /*g.DrawLine(meshPen, new Point((int)renderedBody[i].inner[0][0], (int)renderedBody[i].inner[0][1]),
                    new Point((int)renderedBody[i].c[0][0], (int)renderedBody[i].c[0][1]));

                g.DrawLine(meshPen, new Point((int)renderedBody[i].inner[0][0], (int)renderedBody[i].inner[0][1]),
                    new Point((int)renderedBody[i].a[0][0], (int)renderedBody[i].a[0][1]));

                g.DrawLine(meshPen, new Point((int)renderedBody[i].inner[0][0], (int)renderedBody[i].inner[0][1]),
                    new Point((int)renderedBody[i].b[0][0], (int)renderedBody[i].b[0][1]));*/

                /*g.DrawLine(debugColor, new Point((int)renderedBody[i].inner[0][0], (int)renderedBody[i].inner[0][1]),
                    new Point((int)renderedBody[i].inner[0][0], (int)renderedBody[i].inner[0][1]-1000));*/
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            double old_scale = scale;
            scale += scale* 0.005 * e.Delta; // the idea is to zoom `equally` in any scale
            if (scale < 0 || scale > 1e5)
                scale = old_scale;
            calculateTransformationMatrix();
            this.Refresh();
        }

        private int lastX, lastY;
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if(e.Button == MouseButtons.Left)
            {
                double dy = e.Y - lastY;
                double dx = e.X - lastX;

                rotationOX += (dy / this.Height) * 3.14;
                rotationOY += (dx / this.Width) * 3.14;
                calculateTransformationMatrix();
                this.Refresh();
            }
            lastX = e.X;
            lastY = e.Y;
        }

        // User input
        protected override void OnKeyPress(KeyPressEventArgs ev)
        {
            base.OnKeyPress(ev);
            switch (ev.KeyChar)
            {
                case (char) Keys.I:
                    rotationOX += 0.05;
                    break;

                case (char)Keys.K:
                    rotationOX -= 0.05;
                    break;

                case (char)Keys.J:
                    rotationOY -= 0.05;
                    break;

                case (char)Keys.L:
                    rotationOY += 0.05;
                    break;


                case (char)Keys.W:
                    shiftY -= 20;
                    break;

                case (char)Keys.A:
                    shiftX += 20;
                    break;

                case (char)Keys.S:
                    shiftY += 20;
                    break;

                case (char)Keys.D:
                    shiftX -= 20;
                    break;


                case (char)Keys.Q:
                    showAxes = !showAxes;
                    break;

                case (char)Keys.R:
                    scale += 0.3;
                    break;

                case (char)Keys.F:
                    scale -= 0.3;
                    if (scale < 0)
                        scale = 0.01;
                    break;

                default:
                    return;
            }

            calculateTransformationMatrix();
            this.Refresh();
        }
    }
}
