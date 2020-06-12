using System;
using System.Collections.Generic;
using System.Text;

namespace CoreRender.Shaders
{
    public class PositionColorInstancedShader : Shader
    {
        public PositionColorInstancedShader()
        {
            VertexSource = @"#version 330
                    layout (location = 0) in vec3 position;
                    layout (location = 1) in vec4 color;
                    layout (location = 2) in mat4 modelMat;

                    uniform mat4 viewMat;
                    uniform mat4 projMat;

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
