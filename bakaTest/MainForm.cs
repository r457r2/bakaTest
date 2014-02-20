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
    public partial class VisualisationForm : Form
    {
        public VisualisationForm()
        {
            SurfaceRenderer renderer = new SurfaceRenderer(new Body(new Surface(5, 10.5, 0.1, 1200, 0.04, 0.01, 1.57, x => (5 * Math.Sin(x / 2)))));

            this.Size = new Size(1280, 720);
            renderer.Size = new Size(1280, 720);

            this.Controls.Add(renderer);
            InitializeComponent();

            // TODO: handle resize
        }
    }
}
