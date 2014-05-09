using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace bakaTest
{
    class BoundCamera : ICamera
    {
        private Vector3 worldUp = new Vector3(0, 1, 0);

        Matrix4 mtx;
        bool modified = true;
        Vector3 target, sphereRelativePos;
        Vector3 targetMove = new Vector3(0, 0, 0), camMove = new Vector3(0, 0, 0);

        // sphereRelativePos contains spherical coordinates
        // x: psi; y: theta; z: radius
        // targetMove contains only shifts (not really move vector)
        // x: right/left, y: up/down, z: forward/backward
        // camMove similarly contains shifts in spherical coordinates

        public Matrix4 getMatrix()
        {
            if (modified)
                calcMtx();
            return mtx;
        }

        private void calcMtx()
        {
            // move camera around target (apply shifts)
            float old_theta = sphereRelativePos.Y;

            Vector3.Add(ref sphereRelativePos, ref camMove, out sphereRelativePos);
            sphereRelativePos.X = (float)Math.IEEERemainder(sphereRelativePos.X, 2 * Math.PI);
            sphereRelativePos.Y = (float)Math.IEEERemainder(sphereRelativePos.Y, 2 * Math.PI);

            if (sphereRelativePos.Z < 1.0f)
                sphereRelativePos.Z = 1.0f;

            // pretty ugly solution that allows to remove limits on camera's vertical rotation
            // firstly, check if angle between lookat and worldup is in critical interval
            // (i.e. vectors are close to be parallel)
            // if so, flip worldup
            // next, adjust values, so actual 'rendered' angle is always outside of critical interval
            if (old_theta > 0.01f && sphereRelativePos.Y < 0.01f
                || old_theta < -0.01f && sphereRelativePos.Y > -0.01f
                || old_theta > -3.13f && sphereRelativePos.Y < -3.13f
                || old_theta < 3.13f && sphereRelativePos.Y > 3.13f)
            {
                worldUp = Vector3.Multiply(worldUp, -1);
            }

            if (sphereRelativePos.Y < 0.01f && sphereRelativePos.Y > -0.01f)
                sphereRelativePos.Y = 0.011f * Math.Sign(camMove.Y);
            else if (sphereRelativePos.Y > 3.13f || sphereRelativePos.Y < -3.13f)
                sphereRelativePos.Y = -3.129f * Math.Sign(camMove.Y);

            // calculate cartesian coordinates of camera (around origin)
            Vector3 cameraPos = new Vector3((float)(Math.Sin(sphereRelativePos.Y) * Math.Cos(sphereRelativePos.X)),
                (float)Math.Cos(sphereRelativePos.Y),
                (float)(Math.Sin(sphereRelativePos.Y) * Math.Sin(sphereRelativePos.X)));
            Vector3.Multiply(ref cameraPos, sphereRelativePos.Z, out cameraPos);

            // move target, using camera look direction (apply shifts)
            if (targetMove.Length != 0)
            {
                Vector3 lookAt = Vector3.Multiply(cameraPos, -1.0f);
                lookAt.Normalize();

                // shift forward
                Vector3 move = Vector3.Multiply(lookAt, targetMove.Z);

                Vector3 right = Vector3.Cross(lookAt, worldUp);
                Vector3 up = Vector3.Cross(right, lookAt);

                // shift right
                Vector3.Multiply(ref right, targetMove.X, out right);
                Vector3.Add(ref move, ref right, out move);

                // shift up
                Vector3.Multiply(ref up, targetMove.Y, out up);
                Vector3.Add(ref move, ref up, out move);

                Vector3.Add(ref target, ref move, out target);
            }

            Vector3.Add(ref cameraPos, ref target, out cameraPos);
            mtx = Matrix4.LookAt(cameraPos, target, worldUp);

            modified = false;
        }

        public BoundCamera(Vector3 target, float horizontalAngle, float verticalAngle, float radius)
        {
            this.target = target;
            sphereRelativePos = new Vector3(horizontalAngle, verticalAngle, radius);
            calcMtx();
        }

        public void rotLeft(float phi)
        {
            camMove.X -= phi;
            modified = true;
        }

        public void rotDown(float theta)
        {
            camMove.Y += theta;
            modified = true;
        }

        public void onMouseRightPressed(float shift)
        {
            camMove.Z -= shift;
            modified = true;
        }

        public void moveForward(float shift)
        {
            targetMove.Z += shift;
            modified = true;
        }

        public void moveRight(float shift)
        {
            targetMove.X += shift;
            modified = true;
        }

        public void moveUp(float shift)
        {
            targetMove.Y += shift;
            modified = true;
        }

        private void decreaseMove(ref Vector3 move)
        {
            // inertion of camera
            if (move.Length != 0)
            {
                move = Vector3.Add(move, Vector3.Multiply(move, -0.3f));
                modified = true;
            }

            if (move.Length < 0.0001 && move.Length != 0)
            {
                move.X = move.Y = move.Z = 0;
                modified = true;
            }  
        }

        public void update()
        {
            decreaseMove(ref camMove);
            decreaseMove(ref targetMove);
        }
    }
}
