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
            SurfaceRenderer renderer = new SurfaceRenderer(new Surface(10, 1.5, 0.1, 1210, 0.05, 0.01, 1.4, x => 2*Math.Sin(x)*Math.Exp(-x/20)));

            this.Size = new Size(1280, 720);
            renderer.Size = new Size(1280, 720);

            this.Controls.Add(renderer);
            InitializeComponent();

            // TODO: handle resize
        }
    }
}
