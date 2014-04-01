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
        private Vector3[] mesh;
        private int vertexCount;
        private int vbo, vao;

        private void invertIfNegative(ref Vector3 v)
        {
            if (v.X < 0)
                v.X = - v.X;
            if (v.Y < 0)
                v.Y = - v.Y;
            if (v.Z < 0)
                v.Z = - v.Z;
        }

        private Vector3 colorFromPositions(ref Vector3 p00, ref Vector3 p01, ref Vector3 p10)
        {
            Vector3 color = Vector3.Cross(Vector3.Subtract(p01, p00), Vector3.Subtract(p10, p00));
            color.Normalize();
            invertIfNegative(ref color);
            return color;
        }

        private void arrangeData()
        {
            vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, new IntPtr(mesh.Length * Vector3.SizeInBytes), mesh, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, Vector3.SizeInBytes * vertexCount);
            GL.BindVertexArray(0);
        }

        public void render()
        {
            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, vertexCount);
            GL.BindVertexArray(0);
        }

        public Mesh(ZArrayDescriptor desc)
        {
            mesh = new Vector3[(desc.height - 1) * (desc.width - 1) * 2 * 3 * 2];
            vertexCount = (desc.height - 1) * (desc.width - 1) * 2 * 3;
            int mesh_p = 0;
            int col_offset = (desc.height - 1) * (desc.width - 1) * 2 * 3;

            for(int i = 0; i < desc.height - 1; ++i)
                for (int j = 0; j < desc.width - 1; ++j)
                {
                    mesh[mesh_p++] = new Vector3(i + 1, j + 1, desc.array[i + 1, j + 1]);
                    mesh[mesh_p++] = new Vector3(i + 1, j, desc.array[i + 1, j]);
                    mesh[mesh_p++] = new Vector3(i, j, desc.array[i, j]);

                    mesh[mesh_p++] = new Vector3(i, j + 1, desc.array[i, j + 1]);
                    mesh[mesh_p++] = new Vector3(i + 1, j + 1, desc.array[i + 1, j + 1]);
                    mesh[mesh_p++] = new Vector3(i, j, desc.array[i, j]);

                    Vector3 color1 = colorFromPositions(ref mesh[mesh_p - 6], ref mesh[mesh_p - 5], ref mesh[mesh_p - 4]);
                    Vector3 color2 = colorFromPositions(ref mesh[mesh_p - 3], ref mesh[mesh_p - 2], ref mesh[mesh_p - 1]);

                    mesh[col_offset + mesh_p - 6] = color1;
                    mesh[col_offset + mesh_p - 5] = color1;
                    mesh[col_offset + mesh_p - 4] = color1;
                    mesh[col_offset + mesh_p - 3] = color2;
                    mesh[col_offset + mesh_p - 2] = color2;
                    mesh[col_offset + mesh_p - 1] = color2;
                }

            Console.WriteLine(vertexCount);
            arrangeData();
            disposeClientData();
        }

        public void disposeClientData()
        {
            mesh = null;
        }
    }
}
