using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bakaTest
{
    class Body
    {
        private List<Triangle> triangles = null;

        public int TrianglesNumber
        {
            get { return triangles.Count; }
        }

        public Triangle this[int idx]
        {
            get { return triangles[idx]; }
        }

        public void AddTriangle(Triangle t)
        {
            triangles.Add(t);
        }

        public Body()
        {
            triangles = new List<Triangle>();
        }

        public Body(Surface s)
        {
            triangles = new List<Triangle>();

            for (int i = 0; i < s.CurvesNumber - 1; ++i)
            {
                for (int j = 0; j < s.StepsNumber - 1; ++j)
                {
                    triangles.Add(new Triangle(s[i][j+1], s[i][j], s[i+1][j]));
                    triangles.Add(new Triangle(s[i+1][j], s[i+1][j + 1], s[i][j+1]));
                }
            }
        }
    }
}
