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
    public class ZArrayDescriptor
    {
        public int[,] array;
        public int width;
        public int height;
    }

    public partial class VisualisationForm : Form
    {
        private SurfaceRenderer renderer;
        
        public VisualisationForm(ZArrayDescriptor array)
        {
            renderer = new SurfaceRenderer(new Body(array));
            this.Size = new Size(1280, 720);
            renderer.Size = new Size(1280, 720);

            this.Controls.Add(renderer);
            InitializeComponent();
        }
        
        public VisualisationForm()
        {
            renderer = new SurfaceRenderer(new Body(new Surface(500, .015, 0.1, 500, 0.09, 0.01, 1.57, x => (5 * Math.Sin(x / 2)))));

            this.Size = new Size(1280, 720);
            renderer.Size = new Size(1280, 720);

            this.Controls.Add(renderer);
            InitializeComponent();
        }

        protected override void OnResize(EventArgs ev)
        {
            renderer.Size = this.Size;
        }
    }
}
