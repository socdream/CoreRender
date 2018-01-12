using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreRender
{
    public class GLWindow : GameWindow
    {
        public Action Draw { get; set; }

        public GLWindow() : base()
        {
            Draw = DrawTriangles;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            GL.ClearColor(Color.MidnightBlue);
            GL.Enable(EnableCap.DepthTest);

            //int program = ShaderManager.CreateProgram(ShaderManager.ColorVertexShader, ShaderManager.ColorFragmentShader);

            //GL.UseProgram(program);

            /*// Get the location of the attributes that enters in the vertex shader
            var position_attribute = GL.GetAttribLocation(program, "position");

            // Specify how the data for position can be accessed
            GL.VertexAttribPointer(position_attribute, 3, VertexAttribPointerType.Float, false, 0, 0);

            // Enable the attribute
            GL.EnableVertexAttribArray(position_attribute);*/
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (Draw != null)
                Draw();

            GL.End();

            SwapBuffers();
        }

        private Action DrawTriangles = () =>
        {
            GL.Begin(PrimitiveType.Triangles);

            //triangle 1
            GL.Color3(Color.Red);
            GL.Vertex3(0.0f, 0.2f, 0.0f);
            GL.Color3(Color.Yellow);
            GL.Vertex3(-0.2f, -0.2f, 0.0f);
            GL.Color3(Color.Green);
            GL.Vertex3(0.2f, -0.2f, 0.0f);

            //Triangle 2
            GL.Color3(Color.Gold);
            GL.Vertex3(0.1f, 0.2f, -0.1f);
            GL.Color3(Color.Magenta);
            GL.Vertex3(-0.1f, -0.2f, 0.1f);
            GL.Color3(Color.Lime);
            GL.Vertex3(0.3f, -0.2f, 0.1f);
        };

        public int LoadGeometry(float[] vertices)
        {
            int buffer = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, buffer);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * 4, vertices, BufferUsageHint.StaticDraw);
            
            return buffer;
        }
    }
}
