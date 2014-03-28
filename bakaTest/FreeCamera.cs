using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace bakaTest
{
    class FreeCamera
    {
        private Vector3 worldUp = new Vector3(0, 1, 0);

        private Vector3 lookAt, position;
        private Vector3 move = new Vector3(0, 0, 0);
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

        public FreeCamera(Vector3 look, Vector3 pos)
        {
            lookAt = look;
            position = pos;
        }

        private void calcMtx()
        {
            Vector3.Add(ref position, ref move, out position);
            mtx = Matrix4.LookAt(position, Vector3.Add(position, lookAt), worldUp);
            modified = false;
            //move.X = move.Y = move.Z = 0;
        }

        public void rotLeft(float phi)
        {
            Vector3 up = Vector3.Cross(lookAt, worldUp);
            Vector3.Cross(ref up, ref lookAt, out up);

            Matrix4 rot = Matrix4.CreateFromAxisAngle(up, phi);
            Vector4 l = new Vector4(lookAt, 1.0f);
            Vector4.Transform(ref l, ref rot, out l);

            lookAt.X = l.X;
            lookAt.Y = l.Y;
            lookAt.Z = l.Z;

            modified = true;
        }

        public void rotDown(float phi)
        {
            // check if lookAt is almost parallel to worldUp
            // if they're parallel, some unpleasant effects occures
            float psi = Vector3.CalculateAngle(lookAt, worldUp);
            if (psi < 0.04f || psi > 3.1f)
                return;

            Vector3 right = Vector3.Cross(lookAt, worldUp);
            

            Matrix4 rot = Matrix4.CreateFromAxisAngle(right, phi);
            Vector4 l = new Vector4(lookAt, 1.0f);
            Vector4.Transform(ref l, ref rot, out l);

            lookAt.X = l.X;
            lookAt.Y = l.Y;
            lookAt.Z = l.Z;

            modified = true;
        }

        public void moveForward(float shift)
        {
            move = Vector3.Add(move, Vector3.Multiply(lookAt, shift));
            modified = true;
        }

        public void moveRight(float shift)
        {
            Vector3 right = Vector3.Cross(lookAt, worldUp);
            Vector3.Multiply(ref right, shift, out right);
            Vector3.Add(ref move, ref right, out move);
            
            modified = true;
        }

        public void moveUp(float shift)
        {
            Vector3 up = Vector3.Cross(lookAt, worldUp);
            Vector3.Cross(ref up, ref lookAt, out up);
            Vector3.Multiply(ref up, shift, out up);
            Vector3.Add(ref move, ref up, out move);

            modified = true;
        }

        public void update()
        {
            // inertion of camera
            if (move.Length != 0)
            {
                move = Vector3.Add(move, Vector3.Multiply(move, -0.1f));
                modified = true;
            }

            if (move.Length < 0.0001 && move.Length != 0)
            {
                move.X = move.Y = move.Z = 0;
                modified = true;
            }            
        }
    }
}
