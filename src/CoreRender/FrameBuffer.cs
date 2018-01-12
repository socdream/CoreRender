using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.IO;

namespace Simulador
{
    public class FrameBuffer
    {
        public int Id { get; private set; }
        public int ColorTex { get; private set; }
        public int DepthTex { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public FrameBuffer(int width, int height)
        {
            Width = width;
            Height = height;
            Id = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.FramebufferExt, Id);

            // texture for color attachment
            ColorTex = GL.GenTexture();

            GL.Enable(EnableCap.Texture2D);
            GL.Disable(EnableCap.ColorMaterial);
            GL.BindTexture(TextureTarget.Texture2D, ColorTex);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);

            // set a size for the texture but not any initial data
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, width, height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);

            GL.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.Texture2D, ColorTex, 0);

            // Set up a depth attachment
            DepthTex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, DepthTex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, width, height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.DepthAttachmentExt, TextureTarget.Texture2D, DepthTex, 0);

            var status = GL.CheckFramebufferStatus(FramebufferTarget.FramebufferExt);

            if (status != FramebufferErrorCode.FramebufferCompleteExt)
                throw new Exception("Error creating framebuffer");

            GL.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);
        }

        public void Activate()
        {
            GL.BindFramebuffer(FramebufferTarget.FramebufferExt, Id);

            GL.ClearColor(Color.MidnightBlue);
            GL.Enable(EnableCap.DepthTest);

            //int program = ShaderManager.CreateProgram(ShaderManager.ColorVertexShader, ShaderManager.ColorFragmentShader);

            //GL.UseProgram(program);
        }

        public void Draw()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Begin(PrimitiveType.Triangles);

            DrawTriangles();

            GL.End();
        }

        private void DrawTriangles()
        {
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
        }

        public void Export(string path)
        {
            var data = new byte[Width * Height * 3];
            GL.ReadPixels(0, 0, Width, Height, PixelFormat.Bgr, PixelType.UnsignedByte, data);

            CoreImaging.BMP.BmpImage.Export(data, Width, Height, path);
        }
    }
}
