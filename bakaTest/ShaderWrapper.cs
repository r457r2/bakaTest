using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using OpenTK.Graphics.OpenGL;

namespace bakaTest
{
    class ShaderWrapper
    {
        public int handle;

        public ShaderWrapper(ShaderType type, string source)
        {
            handle = GL.CreateShader(type);
            GL.ShaderSource(handle, source);
        }

        public void Compile()
        {
            GL.CompileShader(handle);

            int status;
            GL.GetShader(handle, ShaderParameter.CompileStatus, out status);
            if (status == 0)
                throw new Exception(GL.GetShaderInfoLog(handle));
        }

        public static ShaderWrapper FromFile(ShaderType type, string path)
        {
            string source = File.ReadAllText(path);
            return new ShaderWrapper(type, source);
        }
    }
}
