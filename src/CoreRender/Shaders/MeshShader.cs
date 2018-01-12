using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreRender.Shaders
{
    public class MeshShader : Shader
    {
        public MeshShader()
        {
            VertexSource = @"#version 330
  
                    layout (location = 0) in vec3 position;
                    layout (location = 1) in vec3 normal;

                    uniform mat4 viewMat;
                    uniform mat4 projMat;
                    uniform mat4 modelMat;

                    out vec3 vecNormal;

                    void main()
                    {
                        gl_Position = projMat * viewMat * modelMat * vec4(position.x, position.y, position.z, 1.0);
                        vecNormal = normal;
                    }";

            FragmentSource = @"#version 330
                    in vec3 vecNormal;

                    uniform vec3 lightDir = vec3(1.0f, 1.0f, 1.0f);
                    uniform vec4 ambientLight = vec4(0.3f, 0.3f, 0.3f, 1.0f);
                    uniform sampler2D diffuseTexture;

                    out vec4 color;

                    void main()
                    {
                        float diff = max(dot(normalize(vecNormal), normalize(lightDir)), 0.0);
                        color = diff + ambientLight;
                    }";
        }
    }
}
