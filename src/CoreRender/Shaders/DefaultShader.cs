using CoreMath;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CoreRender.Shaders
{
    public class DefaultShader : Shader
    {
        private Uniform _foreground = new Uniform()
        {
            Name = "foreground",
            Value = Color.Black
        };
        public Color Foreground
        {
            get
            {
                return (Color)_foreground.Value;
            }
            set
            {
                _foreground.Value = value;

                ShaderManager.SetUniform(this, _foreground);
            }
        }
        
        public DefaultShader()
        {
            VertexSource = @"#version 330
                    layout (location = 0) in vec3 position;

                    uniform mat4 viewMat;
                    uniform mat4 projMat;
                    uniform mat4 modelMat;

                    out vec2 vecUv;

                    void main()
                    {
                        gl_Position = projMat * viewMat * modelMat * vec4(position.x, position.y, position.z, 1.0);
                    }";

            FragmentSource = @"#version 330
                    uniform vec4 foreground = vec4(0, 0, 0, 1);

                    out vec4 color;

                    void main()
                    {
                        color = foreground;
                    }";
        }
    }
}
