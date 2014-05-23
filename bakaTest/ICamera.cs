using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace bakaTest
{
    interface ICamera
    {
        Matrix4 getMatrix();
        Vector3 getPosition();

        void rotLeft(float phi);
        void rotDown(float phi);

        void moveForward(float shift);
        void moveRight(float shift);
        void moveUp(float shift);

        void onMouseRightPressed(float shift);

        void update();
    }
}
