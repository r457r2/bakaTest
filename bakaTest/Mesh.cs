using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using System.IO;

namespace bakaTest
{
    class Mesh
    {
        public enum ColoringMethod
        {
            Grayscale,
            Fullcolor
        }

        public struct Vector4b
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

        public static uint restartIndex = 0xffffffff;
        private int primitiveCount = 0, elementOffset = 0;
        private int vao;
        private PrimitiveType primitiveType;

        private static void invertIfNegative(ref Vector3 v)
        {
            if (v.X < 0)
                v.X = - v.X;
            if (v.Y < 0)
                v.Y = - v.Y;
            if (v.Z < 0)
                v.Z = - v.Z;
        }

        private static void calcFullcolor(ref Vector3 v_in, ref Vector4b v_out)
        {
            invertIfNegative(ref v_in);
            v_out.V1 = (byte)(127.0f * v_in.X);
            v_out.V2 = (byte)(127.0f * v_in.Y);
            v_out.V3 = (byte)(127.0f * v_in.Z);
            v_out.V4 = 0x7f;
        }

        private static void calcGrayscale(ref Vector3 v_in, ref Vector4b v_out)
        {
            invertIfNegative(ref v_in);
            v_out.V1 = (byte)(127.0f * v_in.Z);
            v_out.V2 = (byte)(127.0f * v_in.Z);
            v_out.V3 = (byte)(127.0f * v_in.Z);
            v_out.V4 = 0x7f;
        }

        private static int arrangeData(Vector3 [] positions, Vector4b [] colors, uint [] elements)
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
            int vao = GL.GenVertexArray();
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

            return vao;
        }

        private Mesh(int offset, PrimitiveType type)
        {
            elementOffset = offset;
            primitiveType = type;
        }

        public Mesh(Vector3[] positions, Vector4b[] colors, uint[] elements)
        {
            vao = arrangeData(positions, colors, elements);

            positions = null;
            colors = null;
            elements = null;
            GC.Collect();
        }

        public void render()
        {
            GL.BindVertexArray(vao);
            GL.DrawElements(primitiveType, primitiveCount, DrawElementsType.UnsignedInt, elementOffset * 4);
            GL.BindVertexArray(0);
        }

        private static Tuple<Vector3[], Vector4b[], uint[]> ParseZArray(ZArrayDescriptor desc, ColoringMethod coloring)
        {
            Vector3[,] vertexPositions = new Vector3[desc.width, desc.height];
            Vector3[,] normals = new Vector3[desc.width, desc.height];

            int z_max = 0, z_min = 0;
            for (int i = 0; i < desc.width; ++i)
                for (int j = 0; j < desc.height; ++j)
                {
                    if (desc.array[i, j] > z_max)
                        z_max = (int)desc.array[i, j];
                    if (desc.array[i, j] < z_min)
                        z_min = (int)desc.array[i, j];
                }

            int zCenterShift = (z_max + z_min) / 2;
            int xCenterShift = desc.width / 2;
            int yCenterShift = desc.height / 2;

            // this cycle to be optimized (?)
            for (int i = 0; i < desc.width - 1; ++i)
            {
                for (int j = 0; j < desc.height - 1; ++j)
                {
                    int x = i - xCenterShift;
                    int y = yCenterShift - j;

                    vertexPositions[i + 1, j + 1] = new Vector3(x + 1, y + 1, desc.array[i + 1, j + 1] - zCenterShift);
                    vertexPositions[i + 1, j] = new Vector3(x + 1, y, desc.array[i + 1, j] - zCenterShift);
                    vertexPositions[i, j] = new Vector3(x, y, desc.array[i, j] - zCenterShift);

                    vertexPositions[i, j + 1] = new Vector3(x, y + 1, desc.array[i, j + 1] - zCenterShift);
                    vertexPositions[i + 1, j + 1] = new Vector3(x + 1, y + 1, desc.array[i + 1, j + 1] - zCenterShift);
                    vertexPositions[i, j] = new Vector3(x, y, desc.array[i, j] - zCenterShift);

                    Vector3 norm1 = Vector3.Cross(
                        vertexPositions[i + 1, j + 1] - vertexPositions[i, j],
                        vertexPositions[i + 1, j] - vertexPositions[i, j]).Normalized();

                    Vector3 norm2 = Vector3.Cross(
                        vertexPositions[i, j + 1] - vertexPositions[i, j],
                        vertexPositions[i + 1, j + 1] - vertexPositions[i, j]).Normalized();

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
                    switch (coloring)
                    {
                        case ColoringMethod.Fullcolor:
                            calcFullcolor(ref normals[i, j], ref colors[ptr++]);
                            break;
                        case ColoringMethod.Grayscale:
                            calcGrayscale(ref normals[i, j], ref colors[ptr++]);
                            break;
                    }
                }
            }

            normals = null;
            GC.Collect();

            // data arrangement
            Vector3[] positions = new Vector3[desc.width * desc.height];
            uint[] elements = new uint[2 * (desc.width - 1) * (desc.height + 1)];

            ptr = 0;
            for (int i = 0; i < desc.width; ++i)
                for (int j = 0; j < desc.height; ++j)
                    positions[ptr++] = vertexPositions[i, j];

            vertexPositions = null;
            GC.Collect();

            ptr = 0;
            for (uint i = 0; i < desc.width - 1; ++i)
            {
                uint j;
                for (j = 0; j < desc.height; ++j)
                {
                    elements[ptr++] = (uint)(i * desc.height + j);
                    elements[ptr++] = (uint)((i + 1) * desc.height + j);
                }
                //elements[ptr++] = restartIndex;
                elements[ptr++] = (uint)((i + 1) * desc.height + j - 1);
                elements[ptr++] = (uint)((i + 1) * desc.height);
            }

            return new Tuple<Vector3[], Vector4b[], uint[]>(positions, colors, elements);
        }

        public static Mesh FromZArray(ZArrayDescriptor desc, ColoringMethod coloring)
        {
            Tuple<Vector3[], Vector4b[], uint[]> meshData = ParseZArray(desc, coloring);
            Mesh rv = new Mesh(meshData.Item1, meshData.Item2, meshData.Item3);
            rv.primitiveCount = meshData.Item3.Length;
            rv.primitiveType = PrimitiveType.TriangleStrip;
            return rv;
        }

        public static void ZArrayToObject(ZArrayDescriptor desc, ColoringMethod coloring, string path)
        {
            Tuple<Vector3[], Vector4b[], uint[]> meshData = ParseZArray(desc, coloring);

            StreamWriter sw = new StreamWriter(path);
            sw.WriteLine("o surface");

            // write verteces
            for (int i = 0; i < meshData.Item1.Length; ++i)
                sw.WriteLine("v " + meshData.Item1[i].X 
                    + " " + meshData.Item1[i].Y 
                    + " " + meshData.Item1[i].Z);

            // write normals
            for (int i = 0; i < meshData.Item2.Length; ++i)
                sw.WriteLine("vn " + ((float) meshData.Item2[i].V1) / 127.0f
                    + " " + ((float) meshData.Item2[i].V2) / 127.0f
                    + " " + ((float) meshData.Item2[i].V3) / 127.0f);

            // write indices
            for (int i = 0; i < desc.width - 1; ++i)
                for (int j = 0; j < desc.height - 1; ++j)
                {
                    int v1 = 1 + (i * desc.height + j);
                    int v2 = v1 + 1;           // = 1 + (i * desc.height + j + 1);
                    int v3 = v2 + desc.height; // = 1 + ((i + 1) * desc.height + j + 1);
                    int v4 = v1 + desc.height; // = 1 + ((i + 1) * desc.height + j);

                    sw.WriteLine("f " + v1 + "//" + v1 + " "
                        + v2 + "//" + v2 + " "
                        + v3 + "//" + v3 + " "
                        + v4 + "//" + v4 + " ");
                }

            sw.WriteLine("");
            sw.Close();
        }

        private static int ParseIndex(int idx, int vcnt)
        {
            if (idx < 0)
                return vcnt + idx;
            else
                return idx - 1;
        }

        private static void AddNormal(List<Vector3> vertices, List<Vector3> normals, int i1, int i2, int i3)
        {
            Vector3 normal = Vector3.Cross(vertices[i1] - vertices[i2], vertices[i3] - vertices[i2]).Normalized();
            normals[i1] += normal;
            normals[i2] += normal;
            normals[i3] += normal;
        }

        public static List<Mesh> FromObject(string path)
        {
            uint lineno = 1;
            StreamReader sr = new StreamReader(path);
            PrimitiveType ptype = PrimitiveType.Triangles;

            List<Mesh> meshes = new List<Mesh>();
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3>  normals = new List<Vector3>();
            List<uint>    elements = new List<uint>();
            int offset = 0, primitiveCnt = 0;
            Mesh mesh = new Mesh(0, ptype);

            for (string current_line = sr.ReadLine();
                current_line != null;
                current_line = sr.ReadLine(), lineno++)
            {
                string[] split = current_line.Split(null);
                if (split.Length == 1 && split[0] == "")
                    continue;

                // get rid of empty strings (caused by repeated delimiters):
                // v\x20\x20\x200.1234 converts to: {"v", "", "", "0.1234"}
                int notempty = 0;
                for (int i = 0; i < split.Length; ++i)
                    if (split[i] != "")
                        ++notempty;

                string[] tok = split;
                if (notempty != split.Length)
                {
                    tok = new string[notempty];
                    for (int i = 0, j = 0; i < split.Length; ++i)
                        if (split[i] != "")
                            tok[j++] = split[i];
                }

                switch (tok[0])
                {
                    case "v":
                        if (tok.Length < 4)
                            throw new Exception("Parsing error at line " + lineno + ": expected at least 4 tokens");

                        vertices.Add(new Vector3(float.Parse(tok[1], System.Globalization.CultureInfo.InvariantCulture),
                            float.Parse(tok[2], System.Globalization.CultureInfo.InvariantCulture),
                            float.Parse(tok[3], System.Globalization.CultureInfo.InvariantCulture)));
                        normals.Add(new Vector3());
                        break;
                    case "f":
                        if (tok.Length < 4)
                            throw new Exception("Parsing error at line " + lineno + ": expected at least 4 tokens");

                        int i1 = ParseIndex(int.Parse(tok[1].Split('/')[0]), vertices.Count);
                        int i2 = ParseIndex(int.Parse(tok[2].Split('/')[0]), vertices.Count);
                        int i3 = ParseIndex(int.Parse(tok[3].Split('/')[0]), vertices.Count);
                        int i4 = tok.Length >= 5 ? int.Parse(tok[4].Split('/')[0]) : 0;

                        AddNormal(vertices, normals, i1, i2, i3);
                        elements.Add((uint)i1);
                        elements.Add((uint)i2);
                        elements.Add((uint)i3);
                        offset += 3;
                        primitiveCnt += 3;

                        if (i4 != 0)
                        {
                            i4 = ParseIndex(i4, vertices.Count);
                            elements.Add((uint)i1);
                            elements.Add((uint)i3);
                            elements.Add((uint)i4);
                            AddNormal(vertices, normals, i1, i3, i4);
                            offset += 3;
                            primitiveCnt += 3;
                        }
                        break;
                    case "o":
                    case "g":
                        if (mesh != null && primitiveCnt != 0) //wrong
                        {
                            mesh.primitiveCount = primitiveCnt;
                            primitiveCnt = 0;
                            meshes.Add(mesh);
                        }
                        mesh = new Mesh(offset, ptype);
                        break;
                }
            }

            if (meshes.Count == 0 && primitiveCnt == 0)
                return null;

            if (primitiveCnt != 0)
            {
                mesh.primitiveCount = primitiveCnt;
                meshes.Add(mesh);
            }

            // normals
            Vector4b[] colors = new Vector4b[normals.Count];
            for (int i = 0; i < normals.Count; ++i)
            {
                Vector3 n = normals[i].Normalized();
                calcFullcolor(ref n, ref colors[i]);
            }

            // arrange
            Vector3[] positions = new Vector3[vertices.Count];
            for (int i = 0; i < vertices.Count; ++i)
                positions[i] = vertices[i];

            uint[] elems = new uint[elements.Count];
            for (int i = 0; i < elements.Count; ++i)
                elems[i] = elements[i];

            int vao = arrangeData(positions, colors, elems);
            foreach(Mesh m in meshes)
                m.vao = vao;

            return meshes;
        }
    }
}
