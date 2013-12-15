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
    public partial class Form1 : Form
    {
        public Form1()
        {
            SurfaceRenderer renderer = new SurfaceRenderer(new Surface(0, 0));
            this.Size = new Size(1280, 720);
            renderer.Size = new Size(1280, 720);

            this.Controls.Add(renderer);
            InitializeComponent();

            // TODO: handle resize
        }
    }
}
