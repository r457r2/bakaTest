using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace bakaTest
{
    class Mesh
    {
        public static  uint restartIndex = 0xffffffff;
        private int vertexCount;
        private int vao;

        struct Vector4b
        {
            private byte v1, v2, v3, v4;

            public Vector4b(byte v1, byte v2, byte v3)
            {
                this.v1 = v1;
                this.v2 = v2;
                this.v3 = v3;
                this.v4 = 0xff;
            }

            public byte V1
            {
                get { return v1; }
                set { v1 = value; }
            }

            public byte V2
            {
                get { return v2; }
                set { v2 = value; }
            }

            public byte V3
            {
                get { return v3; }
                set { v3 = value; }
            }

            public byte V4
            {
                get { return v4; }
                set { v4 = value; }
            }

            public override string ToString()
            {
                string s = "(" + v1 + ", " + v2 + ", " + v3 + ", " + v4 + ")";
                return s;
            }
        }

        private void invertIfNegative(ref Vector3 v)
        {
            if (v.X < 0)
                v.X = - v.X;
            if (v.Y < 0)
                v.Y = - v.Y;
            if (v.Z < 0)
                v.Z = - v.Z;
        }

        private void arrangeData(Vector3 [] positions, Vector4b [] colors, uint [] elements)
        {
            // send vertex positions
            int vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer,
                new IntPtr(positions.Length * Vector3.SizeInBytes),
                positions, BufferUsageHint.StaticDraw);

            // send vertex colors
            int cbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, cbo);
            GL.BufferData(BufferTarget.ArrayBuffer,
                new IntPtr(colors.Length * 4),
                colors, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // send index data
            int ibo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
            GL.BufferData(BufferTarget.ElementArrayBuffer,
                new IntPtr(elements.Length * 4),
                elements, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            // create and setup vertex array object
            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, cbo);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Byte, true, 0, 0);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
            GL.BindVertexArray(0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        
        public Mesh(ZArrayDescriptor desc)
        {
            Vector3[,] vertexPositions = new Vector3[desc.width, desc.height];
            Vector3[,] normals = new Vector3[desc.width, desc.height];

            // this cycle to be optimized (?)
            for (int i = 0; i < desc.width - 1; ++i)
            {
                for (int j = 0; j < desc.height - 1; ++j)
                {
                    vertexPositions[i+1, j+1] = new Vector3(i+1, j+1, desc.array[i+1, j+1]);
                    vertexPositions[i+1, j] = new Vector3(i+1, j, desc.array[i+1, j]);
                    vertexPositions[i, j] = new Vector3(i, j, desc.array[i, j]);

                    vertexPositions[i, j+1] = new Vector3(i, j+1, desc.array[i, j+1]);
                    vertexPositions[i+1, j+1] = new Vector3(i+1, j+1, desc.array[i+1, j+1]);
                    vertexPositions[i, j] = new Vector3(i, j, desc.array[i, j]);

                    Vector3 norm1 = Vector3.Cross(
                        Vector3.Subtract(vertexPositions[i+1, j+1], vertexPositions[i, j]),
                        Vector3.Subtract(vertexPositions[i + 1, j], vertexPositions[i, j]));

                    Vector3 norm2 = Vector3.Cross(
                        Vector3.Subtract(vertexPositions[i, j + 1], vertexPositions[i, j]),
                        Vector3.Subtract(vertexPositions[i+1, j+1], vertexPositions[i, j]));

                    norm1.Normalize();
                    norm2.Normalize();

                    Vector3.Add(ref normals[i, j], ref norm1, out normals[i, j]);
                    Vector3.Add(ref normals[i + 1, j], ref norm1, out normals[i + 1, j]);
                    Vector3.Add(ref normals[i + 1, j + 1], ref norm1, out normals[i + 1, j + 1]);
                    Vector3.Add(ref normals[i, j], ref norm2, out normals[i, j]);
                    Vector3.Add(ref normals[i, j + 1], ref norm2, out normals[i, j + 1]);
                    Vector3.Add(ref normals[i + 1, j + 1], ref norm2, out normals[i + 1, j + 1]);
                }
            }

            // vertexes color calculated from average vertex normal
            Vector4b[] colors = new Vector4b[desc.width * desc.height];
            uint ptr = 0;
            for (int i = 0; i < desc.width; ++i)
            {
                for (int j = 0; j < desc.height; ++j)
                {
                    Vector3.Multiply(ref normals[i, j], 0.166666666f, out normals[i, j]);
                    invertIfNegative(ref normals[i, j]);
                    
                    colors[ptr].V1 = (byte)(127.0f * normals[i, j].X);
                    colors[ptr].V2 = (byte)(127.0f * normals[i, j].Y);
                    colors[ptr].V3 = (byte)(127.0f * normals[i, j].Z);

                    /*
                    // colour by normal z-value (grayscale)
                    colors[ptr].V1 = (byte)(127.0f * normals[i, j].Z);
                    colors[ptr].V2 = (byte)(127.0f * normals[i, j].Z);
                    colors[ptr].V3 = (byte)(127.0f * normals[i, j].Z);
                    */

                    colors[ptr++].V4 = 0x7f;
                }
            }

            normals = null;
            GC.Collect();

            // data arrangement
            Vector3[] positions = new Vector3[desc.width * desc.height];
            uint[] elements = new uint[2 * desc.width * desc.height + desc.width - 1];
            vertexCount = 2* desc.width * desc.height + desc.width - 1;

            ptr = 0;
            for (int i = 0; i < desc.width; ++i)
                for (int j = 0; j < desc.height; ++j)
                    positions[ptr++] = vertexPositions[i, j];

            vertexPositions = null;
            GC.Collect();

            ptr = 0;
            for (uint i = 0; i < desc.width - 1; ++i)
            {
                for (uint j = 0; j < desc.height; ++j)
                {
                    elements[ptr++] = (uint)(i * desc.height + j);
                    elements[ptr++] = (uint)((i + 1) * desc.height + j);
                }
                elements[ptr++] = restartIndex;
            }

            arrangeData(positions, colors, elements);

            positions = null;
            colors = null;
            elements = null;
            GC.Collect();
        }

        public void render()
        {
            GL.BindVertexArray(vao);
            GL.DrawElements(PrimitiveType.TriangleStrip, vertexCount, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
        }
    }
}
