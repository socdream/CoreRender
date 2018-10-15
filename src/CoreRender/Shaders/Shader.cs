using CoreMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreRender.Shaders
{
    public abstract class Shader
    {
        public int Program { get; set; }
        public int FragmentShader { get; set; }
        public int VertexShader { get; set; }
        public string FragmentSource { get; set; }
        public string VertexSource { get; set; }

        private Uniform _viewMat = new Uniform()
        {
            Name = "viewMat",
            Value = new float[] { }.IdentityMatrix()
        };
        public float[] ViewMatrix
        {
            get
            {
                return (float[])_viewMat.Value;
            }
            set
            {
                _viewMat.Value = value;

                ShaderManager.SetUniform(this, _viewMat);
            }
        }
        private Uniform _projMat = new Uniform()
        {
            Name = "projMat",
            Value = new float[] { }.IdentityMatrix()
        };
        public float[] ProjectionMatrix
        {
            get
            {
                return (float[])_projMat.Value;
            }
            set
            {
                _projMat.Value = value;

                ShaderManager.SetUniform(this, _projMat);
            }
        }
        private Uniform _modelMat = new Uniform()
        {
            Name = "modelMat",
            Value = new float[] { }.IdentityMatrix()
        };
        public float[] ModelMatrix
        {
            get
            {
                return (float[])_modelMat.Value;
            }
            set
            {
                _modelMat.Value = value;

                ShaderManager.SetUniform(this, _modelMat);
            }
        }

        protected void SetTexture(int texture, int uniformLocation)
        {
            OpenTK.Graphics.OpenGL4.GL.ActiveTexture(OpenTK.Graphics.OpenGL4.TextureUnit.Texture0);
            OpenTK.Graphics.OpenGL4.GL.BindTexture(OpenTK.Graphics.OpenGL4.TextureTarget.Texture2D, texture);
            
            OpenTK.Graphics.OpenGL4.GL.Uniform1(uniformLocation, 0);
        }
    }
}
