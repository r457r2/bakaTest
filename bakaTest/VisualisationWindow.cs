using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Gwen.Control;

namespace bakaTest
{
    class VisualisationWindow : GameWindow
    {
        int shaderProgram;

        Vector2 oldMousePos;
        SceneNode node;
        ICamera cam;
        PerspectiveProjeciton proj;
        int projLoc, viewLoc, modelLoc;

        Gwen.Input.OpenTK gwenInput;
        Canvas canvas;
        StatusBar statusBar;
        DockBase dock;
        RadioButtonGroup radioCamera;
        Gwen.Renderer.OpenTK renderer;
        Label labelPosX;
        Label labelPosY;
        Label labelPosZ;
        Label labelTips;
        Label labelSpeed;

        private const string boundCameraTip = @"Перемещения точки привязки:
W, A - вперед/назад
S, D - влево/вправо
Space, C - вверх/вниз
Левая кнопка мыши: вращение вокруг точки привязки
Правая кнопка мыши: приближение/отдаление
Колесо мыши: скорость перемещения";
        private const string freeCameraTip = @"Перемещение камеры:
W, A - вперед/назад
S, D - влево/вправо
Space, C - вверх/вниз
Левая кнопка мыши: вращение камеры
Колесо мыши: скорость перемещения";
        private const string nodeTransformTip = @"Вращение объекта:
U, O: вокруг продольной оси
I, K: вокруг поперечной оси
J, L: вокруг вертикальной оси";
        
        // antialiasing?
        public VisualisationWindow() : base(100, 100, new GraphicsMode())
        {
            this.Width = DisplayDevice.Default.Width;
            this.Height = DisplayDevice.Default.Height - 70;
            this.Location = new System.Drawing.Point(0, 0);
            this.VSync = VSyncMode.On;

            cam = new FreeCamera(new Vector3(0, 0, 1), new Vector3(0, 0, -700));
            //cam = new BoundCamera(new Vector3(0, 0, 0), -1.57f, 1.57f, 600.0f);
        }

        public VisualisationWindow(ZArrayDescriptor desc, ICamera cam, int fsaa_samples = 0, bool vsync = true)
            : base(100, 100, new GraphicsMode(32, 24, 0, fsaa_samples))
        {
            this.Width = DisplayDevice.Default.Width;
            this.Height = DisplayDevice.Default.Height - 70;
            this.Location = new System.Drawing.Point(0, 0);

            this.cam = cam;
            if (!vsync)
                this.VSync = VSyncMode.Off;
        }

        public VisualisationWindow(ZArrayDescriptor desc, ICamera cam, int width, int height, int fsaa_samples, bool vsync)
            : base(width, height, new GraphicsMode(32, 24, 0, fsaa_samples))
        {
            this.cam = cam;
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
            modelLoc = GL.GetUniformLocation(shaderProgram, "model");
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            CreateProgram();

            // production
            //Mesh mesh = Mesh.FromZArray(desc, Mesh.ColoringMethod.Fullcolor);

            // testing
            //Mesh mesh = Mesh.FromObject("untitled.obj")[0];
            ZArrayDescriptor zarr = ZArrayDescriptor.createPerlin1d(200, 200, 8);
            Mesh mesh = Mesh.FromZArray(zarr, Mesh.ColoringMethod.Grayscale);
            //Mesh Mesh.ZArrayToObject(zarr, Mesh.ColoringMethod.Fullcolor, "250.obj");

            node = new SceneNode(mesh, modelLoc);

            proj = new PerspectiveProjeciton(3.14159f / 4, 2.0f, 5000.0f, (float)this.Width / this.Height);

            // Fails on hardware not supporting PrimitiveRestart
            //GL.Enable(EnableCap.PrimitiveRestart);
            //GL.PrimitiveRestartIndex(Mesh.restartIndex);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.DepthRange(0.0f, 1.0f);
            GL.Enable(EnableCap.DepthClamp);
            
            // Useful feature ._.
            //GL.Enable(EnableCap.CullFace);
            //GL.FrontFace(FrontFaceDirection.Ccw);
            //GL.CullFace(CullFaceMode.Back);

            setupUi();
        }

        void radioCamera_SelectionChanged(Base control, EventArgs args)
        {
            RadioButtonGroup rg = (RadioButtonGroup)control;
            if (rg.SelectedName == "free")
            {
                cam = new FreeCamera(new Vector3(0, 0, 1), new Vector3(0, 0, -700));
                labelTips.SetText(freeCameraTip + "\n\n" + nodeTransformTip);
                labelTips.SizeToContents();
            }
            else if(rg.SelectedName == "bound")
            {
                cam = new BoundCamera(new Vector3(0, 0, 0), -1.57f, 1.57f, 600.0f);
                labelTips.SetText(boundCameraTip + "\n\n" + nodeTransformTip);
                labelTips.SizeToContents();
            }
        }

        void cameraReset_Clicked(Base control, EventArgs args)
        {
            radioCamera_SelectionChanged(radioCamera, null);
        }

        private void setupUi()
        {
            renderer = new Gwen.Renderer.OpenTK();
            Gwen.Skin.Base skin = new Gwen.Skin.TexturedBase(renderer, "DefaultSkin.png");
            canvas = new Canvas(skin);
            canvas.SetSize(Width, Height);

            gwenInput = new Gwen.Input.OpenTK(this);
            gwenInput.Initialize(canvas);

            Mouse.ButtonDown += Mouse_ButtonDown;
            Mouse.ButtonUp += Mouse_ButtonUp;
            Mouse.Move += Mouse_Move;
            Mouse.WheelChanged += Mouse_Wheel;

            canvas.ShouldDrawBackground = true;
            canvas.BackgroundColor = System.Drawing.Color.FromArgb(122, 150, 170, 170);

            // controls
            radioCamera = new RadioButtonGroup(canvas);
            radioCamera.AutoSizeToContents = true;
            radioCamera.SetText("Тип камеры");
            radioCamera.AddOption("Свободная", "free");
            radioCamera.AddOption("Привязанная", "bound");
            radioCamera.SetSelection(0);
            radioCamera.SelectionChanged += radioCamera_SelectionChanged;

            // coord
            GroupBox posGroup = new GroupBox(canvas);
            posGroup.SetText("Позиция камеры");
            posGroup.SizeToChildren();
            posGroup.SetSize(150, 120);
            Gwen.Align.PlaceDownLeft(posGroup, radioCamera, 45);
            
            labelPosX = new Label(posGroup);
            labelPosY = new Label(posGroup);
            labelPosZ = new Label(posGroup);
            labelPosX.SetText("X: 0.0");
            labelPosY.SetText("Y: 1.0");
            labelPosZ.SetText("Z: 2.0");
            Gwen.Align.PlaceDownLeft(labelPosY, labelPosX, 5);
            Gwen.Align.PlaceDownLeft(labelPosZ, labelPosY, 5);

            // reset button
            Button resetCameraButton = new Gwen.Control.Button(posGroup);
            resetCameraButton.SetText("Сбросить позицию\nкамеры");
            resetCameraButton.Clicked += cameraReset_Clicked;
            resetCameraButton.SizeToContents();
            Gwen.Align.PlaceDownLeft(resetCameraButton, labelPosZ, 5);

            labelSpeed = new Label(canvas);
            Gwen.Align.PlaceDownLeft(labelSpeed, posGroup, 5);

            labelTips = new Label(canvas);
            labelTips.SetText(freeCameraTip + "\n\n" + nodeTransformTip);
            labelTips.SizeToContents();
            Gwen.Align.PlaceDownLeft(labelTips, labelSpeed, 15);
 
            statusBar = new Gwen.Control.StatusBar(canvas);
            statusBar.Dock = Gwen.Pos.Bottom;            
        }

        void Mouse_ButtonDown(object sender, MouseButtonEventArgs args)
        {
            gwenInput.ProcessMouseMessage(args);
        }

        void Mouse_ButtonUp(object sender, MouseButtonEventArgs args)
        {
            gwenInput.ProcessMouseMessage(args);
        }

        void Mouse_Move(object sender, MouseMoveEventArgs args)
        {
            gwenInput.ProcessMouseMessage(args);
        }

        void Mouse_Wheel(object sender, MouseWheelEventArgs args)
        {
            gwenInput.ProcessMouseMessage(args);
        }

        float shift = 2.5f;
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

            if (Keyboard[Key.I])
                node.rotate(new Vector3(1.0f, 0.0f, 0.0f), shift / 70);
            if (Keyboard[Key.K])
                node.rotate(new Vector3(1.0f, 0.0f, 0.0f), -shift / 70);
            if (Keyboard[Key.J])
                node.rotate(new Vector3(0.0f, 1.0f, 0.0f), shift / 70);
            if (Keyboard[Key.L])
                node.rotate(new Vector3(0.0f, 1.0f, 0.0f), -shift / 70);
            if (Keyboard[Key.U])
                node.rotate(new Vector3(0.0f, 0.0f, 1.0f), shift / 70);
            if (Keyboard[Key.O])
                node.rotate(new Vector3(0.0f, 0.0f, 1.0f), -shift / 70);

            if (Keyboard[Key.Escape])
                this.Exit();


            Vector3 cameraPos = cam.getPosition();
            labelPosX.SetText("X: " + cameraPos.X);
            labelPosY.SetText("Y: " + cameraPos.Y);
            labelPosZ.SetText("Z: " + cameraPos.Z);

            labelSpeed.SetText("Скорость перемещения: " + shift);

            if (renderer.TextCacheSize > 50)
                renderer.FlushTextCache();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            this.Title = this.RenderFrequency.ToString() + " fps";

            statusBar.SetText(this.RenderFrequency.ToString());

            GL.UseProgram(shaderProgram);
            Matrix4 hack = cam.getMatrix();
            GL.UniformMatrix4(viewLoc, false, ref hack);
            node.render();
            GL.UseProgram(0);

            canvas.RenderCanvas();

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

            // canvas
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Width, Height, 0, -1, 1);
            canvas.SetSize(Width, Height);
            //dock.SetSize(Width, Height);

            proj.AspectRatio = (float)this.Width / this.Height;
            setProjectionUniform();

            OnUpdateFrame(null);
            OnRenderFrame(null);
        }
    }
}
