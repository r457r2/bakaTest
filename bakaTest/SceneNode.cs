using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace bakaTest
{
    class SceneNode
    {
        private Mesh mesh;
        private int modelLoc;

        private Quaternion orientation = new Quaternion(1.0f, 0.0f, 0.0f, 0.0f);
        private Vector3 vOffset;

        public SceneNode(Mesh mesh, int modelMatrixLoc)
        {
            this.mesh = mesh;
            modelLoc = modelMatrixLoc;
        }

        public void render()
        {
            Matrix4 hack = getMatrix();
            GL.UniformMatrix4(modelLoc, false, ref hack);
            mesh.render();
        }

        private Matrix4 getMatrix()
        {
            return Matrix4.Identity * Matrix4.CreateTranslation(vOffset) * Matrix4.CreateFromQuaternion(orientation);
        }

        public void rotate(Vector3 axis, float angle)
        {
            axis.Normalize();
            axis = axis * (float)Math.Sin(angle / 2.0f);
            float scalar = (float)Math.Cos(angle / 2.0f);
            rotate(new Quaternion(axis, scalar));
        }

        public void rotate(Quaternion orient)
        {
            orientation *= orient;
            orientation.Normalize();
        }

        public void setOrientation(Quaternion orient)
        {
            orientation = orient;
        }

        public void offset(Vector3 offset)
        {
            vOffset += offset;
        }

        public void setTransition(Vector3 offset)
        {
            vOffset = offset;
        }
    }
}
