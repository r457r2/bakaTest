﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace bakaTest
{
    class VisualisationWindow : GameWindow
    {
        int shaderProgram;

        Vector2 oldMousePos;
        Mesh mesh;
        ICamera cam;
        PerspectiveProjeciton proj;
        int projLoc, viewLoc;
        
        // antialiasing?
        public VisualisationWindow() : base(1280, 720, new GraphicsMode()) 
        {
            //cam = new FreeCamera(new Vector3(0, 0, 1), new Vector3(0, 0, -5));
            cam = new BoundCamera(new Vector3(250, 250, 0), 0, 1.47f, 1000.0f);
            mesh = new Mesh(ZArrayDescriptor.createPerlin1d(500, 400, 6));
        }

        public VisualisationWindow(ZArrayDescriptor desc, ICamera cam, int width, int height, int fsaa_samples, bool vsync)
            : base(width, height, new GraphicsMode(32, 24, 0, fsaa_samples))
        {
            this.cam = cam;
            mesh = new Mesh(desc);
            if(!vsync)
                this.VSync = VSyncMode.Off;
        }

        private void CreateProgram()
        {
            ShaderWrapper vshader = ShaderWrapper.FromFile(ShaderType.VertexShader, "..\\..\\shaders\\shader.vert");
            ShaderWrapper fshader = ShaderWrapper.FromFile(ShaderType.FragmentShader, "..\\..\\shaders\\shader.frag");

            vshader.Compile();
            fshader.Compile();

            shaderProgram = GL.CreateProgram();
            GL.AttachShader(shaderProgram, vshader.handle);
            GL.AttachShader(shaderProgram, fshader.handle);

            int status;
            GL.LinkProgram(shaderProgram);
            GL.GetProgram(shaderProgram, ProgramParameter.LinkStatus, out status);
            if (status == 0)
                throw new Exception(GL.GetProgramInfoLog(shaderProgram));

            projLoc = GL.GetUniformLocation(shaderProgram, "projection");
            viewLoc = GL.GetUniformLocation(shaderProgram, "view");
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            CreateProgram();

            proj = new PerspectiveProjeciton(3.14159f / 4, 0.01f, 5000.0f, (float)this.Width / this.Height);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.DepthRange(0.0f, 1.0f);
            //GL.Enable(EnableCap.DepthClamp);
            
            //GL.Enable(EnableCap.CullFace);
            //GL.FrontFace(FrontFaceDirection.Ccw);
            //GL.CullFace(CullFaceMode.Back);
        }

        float shift = 5.0f;
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            cam.update();

            if (Mouse[MouseButton.Left])
            {
                cam.rotLeft((oldMousePos.X - Mouse.X) / this.Width * (float) Math.PI);
                cam.rotDown((oldMousePos.Y - Mouse.Y) / this.Height * (float) Math.PI);
            }

            if (Mouse[MouseButton.Right])
                cam.onMouseRightPressed(Math.Sign(Mouse.Y - oldMousePos.Y) 
                    * Vector2.Subtract(oldMousePos, new Vector2(Mouse.X, Mouse.Y)).Length);

            oldMousePos.X = Mouse.X;
            oldMousePos.Y = Mouse.Y;

            shift += 0.1f *shift * (float)Mouse.WheelDelta;
            if (shift < 0.003)
                shift = 0.003f;

            if (Keyboard[Key.W])
                cam.moveForward(shift);
            if (Keyboard[Key.S])
                cam.moveForward(-shift);
            if (Keyboard[Key.A])
                cam.moveRight(-shift);
            if (Keyboard[Key.D])
                cam.moveRight(shift);
            if (Keyboard[Key.Space])
                cam.moveUp(shift);
            if (Keyboard[Key.C])
                cam.moveUp(-shift);

            if (Keyboard[Key.R])
            {
                proj.FieldOfView += 0.01f;
                if (proj.FieldOfView > 3.14f)
                    proj.FieldOfView = 3.14f;
                setProjectionUniform();
            }

            if (Keyboard[Key.F])
            {
                proj.FieldOfView -= 0.01f;
                if (proj.FieldOfView < 0.01f)
                    proj.FieldOfView = 0.01f;
                setProjectionUniform();
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            this.Title = this.RenderFrequency.ToString() + " fps";

            GL.UseProgram(shaderProgram);
            Matrix4 hack = cam.getMatrix();
            GL.UniformMatrix4(viewLoc, false, ref hack);
            mesh.render();
            GL.UseProgram(0);

            SwapBuffers();
        }

        private void setProjectionUniform()
        {
            GL.UseProgram(shaderProgram);
            Matrix4 hack = proj.Matrix;
            GL.UniformMatrix4(projLoc, false, ref hack);
            GL.UseProgram(0);
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, this.Width, this.Height);

            proj.AspectRatio = (float)this.Width / this.Height;
            setProjectionUniform();

            OnUpdateFrame(null);
            OnRenderFrame(null);
        }
    }
}
