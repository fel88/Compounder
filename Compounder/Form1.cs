using FxEngine.Shaders;
using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SharpFont;
using System.IO;
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
        bool first = true;
        Shader shader;
        public static byte[] ReadResourceRaw(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fr1 = assembly.GetManifestResourceNames().First(z => z.Contains(resourceName));

            using (Stream stream = assembly.GetManifestResourceStream(fr1))
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }
        public CameraViewManager ViewManager;
        Camera camera1 = new Camera() { IsOrtho = true };
        private EventWrapperGlControl evwrapper;
        GLControl glControl;
        private void InitFonts()
        {
            // FreeType
            // --------
            Library ft = new Library();

            // All functions return a value different than 0 whenever an error occurred
            //if (FT_Init_FreeType(&ft))
            {
                //   std::cout << "ERROR::FREETYPE: Could not init FreeType Library" << std::endl;
                //  return -1;
            }

            // find path to font
            var fontBytes = ReadResourceRaw("OCRAEXT.TTF");

            // load font as face
            Face face = new Face(ft, fontBytes, 0);
            face.SetPixelSizes(0, 48);
            // set size to load glyphs as
            //FT_Set_Pixel_Sizes(face, 0, 48);

            // disable byte-alignment restriction
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

            // load first 128 characters of ASCII set
            for (char c = (char)0; c < 128; c++)
            {
                face.LoadChar(c, LoadFlags.Render, LoadTarget.Normal);
                //   face.Glyph.RenderGlyph(RenderMode.Normal);
                // Load character glyph 
                //if (FT_Load_Char(face, c, FT_LOAD_RENDER))
                {
                    // std::cout << "ERROR::FREETYTPE: Failed to load Glyph" << std::endl;
                    // continue;
                }
                // generate texture
                int texture;
                GL.GenTextures(1, out texture);
                GL.BindTexture(TextureTarget.Texture2D, texture);
                GL.TexImage2D(
                    TextureTarget.Texture2D,
                    0,
                  PixelInternalFormat.R8,
                      face.Glyph.Bitmap.Width,
                     face.Glyph.Bitmap.Rows,
                    0,
                          OpenTK.Graphics.OpenGL.PixelFormat.Red,
                    PixelType.UnsignedByte,
                      face.Glyph.Bitmap.Buffer
                );

                // set texture options
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

                // now store character for later use
                Character character = new Character()
                {
                    TextureID = texture,
                    Size = new Vec2i(face.Glyph.Bitmap.Width, face.Glyph.Bitmap.Rows),
                    Bearing = new Vec2i(face.Glyph.BitmapLeft, face.Glyph.BitmapTop),
                    Advance = (face.Glyph.Advance.X.ToInt32())
                };
                Characters.Add(c, character);
            }
            GL.BindTexture(TextureTarget.Texture2D, 0);

            // destroy FreeType once we're finished
            //     FT_Done_Face(face);
            //    FT_Done_FreeType(ft);

        }

        private void Gl_Paint(object sender, PaintEventArgs e)
        {
            //if (!loaded)
            //  return;
            if (!glControl.Context.IsCurrent)
            {
                glControl.MakeCurrent();
            }
            if (first)
            {
                
                // build and compile our shader zprogram
                // ------------------------------------                
                //lightingShader = new Shader("2.2.basic_lighting.vs", "2.2.basic_lighting.fs");
                shader = new Shader("text.vs", "text.fs");

             
                Matrix4 projection = Matrix4.CreateOrthographicOffCenter(0, glControl.Width, 0, glControl.Height, 0, 1);

                shader.use();
                // glUniformMatrix4fv(glGetUniformLocation(shader.ID, "projection"), 1, false, glm::value_ptr(projection));

                shader.setMat4("projection", projection);
                InitFonts();
                GL.UseProgram(0);
                first = false;

                // configure VAO/VBO for texture quads
                // -----------------------------------
                GL.GenVertexArrays(1, out VAO);
                GL.GenBuffers(1, out VBO);
                GL.BindVertexArray(VAO);
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
                GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 6 * 4, 0, BufferUsageHint.DynamicDraw);
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                GL.BindVertexArray(0);



            }
            Redraw();
            glControl.SwapBuffers();
        }

        void Redraw()
        {

            ViewManager.Update();

            GL.ClearColor(Color.Gray);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Viewport(0, 0, glControl.Width, glControl.Height);
            var o2 = Matrix4.CreateOrthographic(glControl.Width, glControl.Height, -1000, 1000);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref o2);

            Matrix4 modelview2 = Matrix4.LookAt(0, 0, 70, 0, 0, 0, 0, 1, 0);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview2);
            
            GL.Disable(EnableCap.DepthTest);

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

            GL.Disable(EnableCap.DepthTest);
            //GL.Viewport(0, 0, glControl.Width, glControl.Height);

            Matrix4 projection = Matrix4.CreateOrthographicOffCenter(0, glControl.Width, 0, glControl.Height, -1000, 1000);
            
            shader.use();


            shader.setMat4("projection", projection);
            GL.UseProgram(0);
            //   GL.ClearColor(0.2f, 0.3f, 0.3f, 1f);
            //   GL.Clear(ClearBufferMask.ColorBufferBit);
            
             GL.Enable(EnableCap.CullFace);
              GL.Enable(EnableCap.Blend);
              GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

              
             
            RenderText(shader, "This is sample text", -0, -0, 1.0f, new Vector3(0.5f, 0.8f, 0.2f));
            RenderText(shader, "(C) LearnOpenGL.com", -0, 50, 0.5f, new Vector3(0.3f, 0.7f, 0.9f));
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.Blend);

        }
        struct Vec2i
        {
            public Vec2i(int x, int y)
            {
                X = x;
                Y = y;
            }

            public int X;
            public int Y;
        }
        /// Holds all state information relevant to a character as loaded using FreeType
        struct Character
        {
            public int TextureID; // ID handle of the glyph texture
            public Vec2i Size;      // Size of glyph
            public Vec2i Bearing;   // Offset from baseline to left/top of glyph
            public int Advance;   // Horizontal offset to advance to next glyph
        };
        Dictionary<char, Character> Characters = new Dictionary<char, Character>();
        int VAO, VBO;

        // render line of text
        // -------------------
        void RenderText(Shader shader, string text, float x, float y, float scale, Vector3 color)
        {
            // activate corresponding render state	
            shader.use();
            //glUniform3f(glGetUniformLocation(shader.ID, "textColor"), color.x, color.y, color.z);
            shader.setVec3("textColor", color);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindVertexArray(VAO);

            // iterate through all characters
            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];
                Character ch = Characters[c];

                float xpos = x + ch.Bearing.X * scale;
                float ypos = y - (ch.Size.Y - ch.Bearing.Y) * scale;

                float w = ch.Size.X * scale;
                float h = ch.Size.Y * scale;
                // update VBO for each character
                float[,] vertices = new float[6, 4]{
                    {xpos,     ypos + h,   0.0f, 0.0f },
            { xpos,     ypos,       0.0f, 1.0f },
            { xpos + w, ypos,       1.0f, 1.0f },

            { xpos,     ypos + h,   0.0f, 0.0f },
            {xpos + w, ypos, 1.0f, 1.0f },
            {xpos +w, ypos + h,   1.0f, 0.0f}
                };

                // render glyph texture over quad
                GL.BindTexture(TextureTarget.Texture2D, ch.TextureID);
                // update content of VBO memory
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
                GL.BufferSubData(BufferTarget.ArrayBuffer, 0, 6 * 4 * sizeof(float), vertices);// be sure to use glBufferSubData and not glBufferData

                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                // render quad
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
                // now advance cursors for next glyph (note that advance is number of 1/64 pixels)
                x += (ch.Advance) * scale; // bitshift by 6 to get value in pixels (2^6 = 64 (divide amount of 1/64th pixels by 64 to get amount of pixels))
            }
            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.UseProgram(0);

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

            settings.Flags = OpenTK.Windowing.Common.ContextFlags.ForwardCompatible | OpenTK.Windowing.Common.ContextFlags.Debug;
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