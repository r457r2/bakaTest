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

        public void zsort()
        {
            // zsort here
        }

        public Body()
        {
            triangles = new List<Triangle>();
        }

        public Body(ZArrayDescriptor d)
        {
            List<Matrix> convertedPoints = new List<Matrix>();
            for(int y = 0; y < d.height; ++y)
                for (int x = 0; x < d.width; ++x)
                {
                    Matrix p = new Matrix(1, 4);
                    p[0][0] = x;
                    p[0][1] = y;
                    p[0][2] = d.array[x, y];
                    p[0][3] = 1;
                    convertedPoints.Add(p);
                }

            for(int i = 0; i < d.height; ++i)
                for (int j = 0; j < d.width; ++j)
                {
                    triangles.Add(new Triangle(convertedPoints[i * (j+1)], convertedPoints[i * j], convertedPoints[(i+1)*j]));
                    triangles.Add(new Triangle(convertedPoints[(i+1) * j], convertedPoints[(i+1) * (j+1)], convertedPoints[i * (j+1)]));
                }
        }

        // marked for deletion
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
