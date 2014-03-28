using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace bakaTest
{
    class PerspectiveProjeciton
    {
        private float fov, znear, zfar, aspect = 0;
        private Matrix4 mtx;
        private Boolean modified = true;

        public Matrix4 Matrix
        {
            get
            {
                if (modified)
                    calcMtx();
                return mtx;
            }
        }

        public float FieldOfView
        {
            get { return fov; }
            set { fov = value; modified = true; }
        }

        public float zNear
        {
            get { return znear; }
            set { znear = value; modified = true; }
        }

        public float zFar
        {
            get { return zfar; }
            set { zfar = value; modified = true; }
        }

        public float AspectRatio
        {
            get { return aspect; }
            set { aspect = value; modified = true; }
        }

        public PerspectiveProjeciton(float fieldOfView,
            float zNear, float zFar, float aspectRatio)
        {
            fov = fieldOfView;
            zfar = zFar;
            znear = zNear;
            aspect = aspectRatio;
        }

        private void calcMtx()
        {
            mtx = Matrix4.CreatePerspectiveFieldOfView(fov, aspect, znear, zfar);
            modified = false;
        }
    }
}
