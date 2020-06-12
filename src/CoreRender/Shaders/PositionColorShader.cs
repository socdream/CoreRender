using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CoreRender.Shaders
{
    public class PositionColorShader : Shader
    {
        public PositionColorShader()
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

                    out vec4 color;

                    void main()
                    {
                        color = vecColor;
                    }";
        }
    }
}
