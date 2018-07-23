using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CoreRender.Shaders
{
    public class TextShader : Shader
    {
        private Uniform _texture = new Uniform()
        {
            Name = "diffuseTexture",
            Value = 0
        };
        public int Texture
        {
            get
            {
                return (int)_texture.Value;
            }
            set
            {
                _texture.Value = value;

                ShaderManager.SetUniform(this, _texture);
                SetTexture(value, _texture.Location);
            }
        }
        private Uniform _offset = new Uniform()
        {
            Name = "offset",
            Value = new float[2]
        };
        public float[] Offset
        {
            get
            {
                return (float[])_offset.Value;
            }
            set
            {
                _offset.Value = value;

                ShaderManager.SetUniform(this, _offset);
            }
        }
        private Uniform _color = new Uniform()
        {
            Name = "fontcolor",
            Value = System.Drawing.Color.White
        };
        public System.Drawing.Color Color
        {
            get
            {
                return (System.Drawing.Color)_color.Value;
            }
            set
            {
                _color.Value = value;

                ShaderManager.SetUniform(this, _color);
            }
        }

        public TextShader()
        {
            VertexSource = @"#version 330
  
                    layout (location = 0) in vec3 position;
                    layout (location = 1) in vec2 uv;

                    uniform mat4 viewMat;
                    uniform mat4 projMat;
                    uniform mat4 modelMat;

                    uniform vec2 offset = vec2(0, 0);

                    out vec2 vecUv;

                    void main()
                    {
                        gl_Position = vec4(position.x + offset.x, position.y + offset.y, position.z, 1.0);
                        vecUv = uv;
                    }";

            FragmentSource = @"#version 330
                    in vec2 vecUv;

                    uniform sampler2D diffuseTexture;
                    uniform vec4 fontcolor = vec4(0, 0, 0, 0);

                    out vec4 color;

                    void main()
                    {
                        //color = vec4(1,0,0,1);
                        color = texture2D(diffuseTexture, vecUv.st);
                        color = vec4(fontcolor.r, fontcolor.g, fontcolor.b, color.a);
                        //color = vec4(vecUv.x, vecUv.y, 0.0, 1.0);
                    }";
        }
    }
}
