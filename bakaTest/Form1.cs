using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace bakaTest
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            SurfaceRenderer renderer = new SurfaceRenderer(new Surface(50, 1.5, 4, 250, 0.4, 4, 0.4, x => Math.Sin(x) + Math.Sin(2*x) + Math.Sin(3*x) + Math.Sin(4*x)));

            this.Size = new Size(1280, 720);
            renderer.Size = new Size(1280, 720);

            this.Controls.Add(renderer);
            InitializeComponent();

            // TODO: handle resize
        }
    }
}
