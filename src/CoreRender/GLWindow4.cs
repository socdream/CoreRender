using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK;

namespace CoreRender
{
    public class GLWindow4 : GameWindow
    {
        public event EventHandler<FrameEventArgs> DrawScene;
        public bool DepthTesting
        {
            set
            {
                if (value)
                    GL.Enable(EnableCap.DepthTest);
                else
                    GL.Disable(EnableCap.DepthTest);
            }
        }

        public void EnableBlend(bool enable)
        {
            if(enable)
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);//BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            }
            else
            {
                GL.Disable(EnableCap.Blend);
            }
        }

        /// <summary>
        /// Enables writing on the depth buffer
        /// </summary>
        public bool DepthWrite
        {
            set
            {
                GL.DepthMask(value);
            }
        }

        //public bool StencilWrite
        //{
        //    set
        //    {
        //        GL.StencilFunc(StencilFunction.Always, 1, 0xff);
        //        GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
        //        GL.StencilMask(0xff);
        //    }
        //}

        public GLWindow4() : base()
        {
            
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            GL.ClearColor(Color.MidnightBlue);
            GL.Enable(EnableCap.DepthTest);

            // transparency
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);//BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            // antialiasing
            GL.Enable(EnableCap.PolygonSmooth);

            GL.DepthFunc(DepthFunction.Lequal);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            DrawScene?.Invoke(this, e);

            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(ClientRectangle);
        }
        
        public int LoadGeometry(float[] vertices)
        {
            int buffer = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, buffer);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * 4, vertices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            return buffer;
        }
    }
}
