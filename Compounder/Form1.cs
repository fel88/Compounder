using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows.Forms;
using System.Windows.Media.Media3D;

namespace Compounder
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            Form = this;
            InitializeComponent();
            InitGl();
        }

        public CameraViewManager ViewManager;
        Camera camera1 = new Camera() { IsOrtho = true };
        private EventWrapperGlControl evwrapper;
        GLControl glControl;
        private void Gl_Paint(object sender, PaintEventArgs e)
        {
            //if (!loaded)
            //  return;
            if (!glControl.Context.IsCurrent)
            {
                glControl.MakeCurrent();
            }
            
            Redraw();
            glControl.SwapBuffers();
        }

        void Redraw()
        {
            ViewManager.Update();

            GL.ClearColor(Color.LightGray);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                     
            GL.Viewport(0, 0, glControl.Width, glControl.Height);
            var o2 = Matrix4.CreateOrthographic(glControl.Width, glControl.Height, 1, 1000);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref o2);

            Matrix4 modelview2 = Matrix4.LookAt(0, 0, 70, 0, 0, 0, 0, 1, 0);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview2);

            GL.Enable(EnableCap.DepthTest);            

            DrawBackground();
            
            GL.PushMatrix();
            GL.Translate(camera1.viewport[2] / 2 - 50, -camera1.viewport[3] / 2 + 50, 0);
            GL.Scale(0.5, 0.5, 0.5);

            var mtr = camera1.ViewMatrix;
            var q = mtr.ExtractRotation();
            var mtr3 = Matrix4d.CreateFromQuaternion(q);
            GL.MultMatrix(ref mtr3);
            GL.LineWidth(2);
            GL.Color3(Color.Red);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(100, 0, 0);
            GL.End();

            GL.Color3(Color.Green);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 100, 0);
            GL.End();

            GL.Color3(Color.Blue);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 0, 100);
            GL.End();
            GL.PopMatrix();
           
            camera1.Setup(glControl);
            
            if (drawAxes)
            {
                DrawAxes();
            }

            GL.Enable(EnableCap.Light0);

            GL.ShadeModel(ShadingModel.Smooth);
          //  foreach (var item in Helpers)
            {
            //    item.Draw(null);
            }

            //if (pickEnabled)
              //  PickUpdate();

           
        }

        private void DrawAxes()
        {
            GL.LineWidth(2);
            GL.Color3(Color.Red);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(100, 0, 0);
            GL.End();

            GL.Color3(Color.Green);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 100, 0);
            GL.End();

            GL.Color3(Color.Blue);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 0, 100);
            GL.End();
        }

        private void DrawBackground()
        {
            float zz = -500;

            GL.Begin(PrimitiveType.Quads);
            GL.Color3(Color.LightBlue);
            GL.Vertex3(-glControl.Width / 2, -glControl.Height / 2, zz);
            GL.Vertex3(glControl.Width / 2, -glControl.Height / 2, zz);
            GL.Color3(Color.AliceBlue);
            GL.Vertex3(glControl.Width / 2, glControl.Height / 2, zz);
            GL.Vertex3(-glControl.Width / 2, glControl.Height, zz);
            GL.End();

        }

        bool drawAxes = true;
        private void InitGl()
        {
            var def = GLControlSettings.Default;
            GLControlSettings settings = new GLControlSettings();
            settings.StencilBits = 0;
            settings.DepthBits = 24;
            settings.NumberOfSamples = 8;
            
            settings.Profile = OpenTK.Windowing.Common.ContextProfile.Compatability;

            glControl = new GLControl(settings);
            
            evwrapper = new EventWrapperGlControl(glControl);

            glControl.Paint += Gl_Paint;
            ViewManager = new DefaultCameraViewManager();
            ViewManager.Attach(evwrapper, camera1);

            Controls.Add(glControl);
            glControl.Dock = DockStyle.Fill;
            
            _timer = new System.Timers.Timer(10.0); // in milliseconds - you might have to play with this value to throttle your framerate depending on how involved your update and render code is
            _timer.Elapsed += TimerElapsed;
            _timer.Start();
        }

        System.Timers.Timer _timer;

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            //UpdateModel(); // this is where you'd do whatever you need to do to update your model per frame
                           // Invalidate will cause the Paint event on your GLControl to fire
            glControl.Invalidate(); // _glControl is obviously a private reference to the GLControl
        }
        public static Form1 Form;
        RibbonMenu menu;

        private void Form1_Load(object sender, EventArgs e)
        {
            menu = new RibbonMenu();
            Controls.Add(menu);
            menu.AutoSize = true;
            menu.Dock = DockStyle.Top;

            var mf = new MessageFilter();
            System.Windows.Forms.Application.AddMessageFilter(mf);

        }

        void DrawItem()
        {
            
        }
    }
}