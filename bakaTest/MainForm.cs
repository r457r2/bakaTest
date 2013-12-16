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
            SurfaceRenderer renderer = new SurfaceRenderer(new Surface(10, 2, 20, 100, 1, 40, Math.PI/4, x => Math.Sin(x)));

            this.Size = new Size(1280, 720);
            renderer.Size = new Size(1280, 720);

            this.Controls.Add(renderer);
            InitializeComponent();

            // TODO: handle resize
        }
    }
}
