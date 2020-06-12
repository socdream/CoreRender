using CoreRender.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CoreRender.Shaders
{
    public class PositionCustomColorsShader : Shader
    {
        private Uniform _color1 = new Uniform()
        {
            Name = "color1",
            Value = new float[] { 1.0f, 0.0f, 0.0f, 1.0f }
        };
        public Color Color1
        {
            get => ((float[])_color1.Value).ToColor();
            set
            {
                if (((float[])_color1.Value).ToColor() == value)
                    return;

                _color1.Value = value.ToFloatArray();

                ShaderManager.SetUniform(this, _color1);
            }
        }
        private Uniform _color2 = new Uniform()
        {
            Name = "color2",
            Value = new float[] { 1.0f, 0.0f, 0.0f, 1.0f }
        };
        public Color Color2
        {
            get => ((float[])_color2.Value).ToColor();
            set
            {
                _color2.Value = value.ToFloatArray();

                ShaderManager.SetUniform(this, _color2);
            }
        }
        private Uniform _color3 = new Uniform()
        {
            Name = "color3",
            Value = new float[] { 1.0f, 0.0f, 0.0f, 1.0f }
        };
        public Color Color3
        {
            get => ((float[])_color3.Value).ToColor();
            set
            {
                _color3.Value = value.ToFloatArray();

                ShaderManager.SetUniform(this, _color3);
            }
        }

        public PositionCustomColorsShader()
        {
            VertexSource = @"#version 330
                    layout (location = 0) in vec3 position;
                    layout (location = 1) in vec4 color;

                    uniform mat4 viewMat;
                    uniform mat4 projMat;
                    uniform mat4 modelMat;

                    out vec4 vecColor;

                    void main()
                    {
                        gl_Position = projMat * viewMat * modelMat * vec4(position.x, position.y, position.z, 1.0);
                        vecColor = color;
                    }";

            FragmentSource = @"#version 330
                    in vec4 vecColor;

                    uniform vec4 color1 = vec4(1.0, 0.0, 0.0, 1.0);
                    uniform vec4 color2 = vec4(1.0, 0.0, 0.0, 1.0);
                    uniform vec4 color3 = vec4(1.0, 0.0, 0.0, 1.0);

                    out vec4 color;

                    void main()
                    {
                        if(vecColor == vec4(1.0, 0.0, 0.0, 1.0))
                            color = color1;
                        else if(vecColor == vec4(0.0, 1.0, 0.0, 1.0))
                            color = color2;
                        else if(vecColor == vec4(0.0, 0.0, 1.0, 1.0))
                            color = color3;
                        else
                            color = vecColor;
                    }";
        }
    }
}
