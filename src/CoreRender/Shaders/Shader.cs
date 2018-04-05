using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreRender.Shaders
{
    public class Shader
    {
        public int Program { get; set; }
        public int FragmentShader { get; set; }
        public int VertexShader { get; set; }
        public string FragmentSource { get; set; }
        public string VertexSource { get; set; }

        protected void SetTexture(int texture, int uniformLocation)
        {
            OpenTK.Graphics.OpenGL4.GL.ActiveTexture(OpenTK.Graphics.OpenGL4.TextureUnit.Texture0);
            OpenTK.Graphics.OpenGL4.GL.BindTexture(OpenTK.Graphics.OpenGL4.TextureTarget.Texture2D, texture);
            
            OpenTK.Graphics.OpenGL4.GL.Uniform1(uniformLocation, 0);
        }
    }
}
