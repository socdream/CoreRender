using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreRender.Shaders
{
    public class TexturedMeshShader : Shader
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
                if ((int)_texture.Value == value)
                    return;

                _texture.Value = value;
                
                ShaderManager.SetUniform(this, _texture);
            }
        }

        public TexturedMeshShader()
        {
            VertexSource = @"#version 330
  
                    layout (location = 0) in vec3 position;
                    layout (location = 1) in vec3 normal;
                    layout (location = 2) in vec2 uv;

                    uniform mat4 viewMat;
                    uniform mat4 projMat;
                    uniform mat4 modelMat;

                    out vec3 vecNormal;
                    out vec2 vecUv;

                    void main()
                    {
                        gl_Position = projMat * viewMat * modelMat * vec4(position.x, position.y, position.z, 1.0);
                        vecNormal = normal;
                        vecUv = uv;
                    }";

            FragmentSource = @"#version 330
                    in vec3 vecNormal;
                    in vec2 vecUv;

                    uniform vec3 lightDir = vec3(1.0f, 1.0f, 1.0f);
                    uniform vec4 ambientLight = vec4(0.3f, 0.3f, 0.3f, 1.0f);
                    uniform sampler2D diffuseTexture;

                    out vec4 color;

                    void main()
                    {
                        float diff = max(dot(normalize(vecNormal), normalize(lightDir)), 0.0);
                        vec4 texColor = texture2D(diffuseTexture, vecUv.st);
                        color = texColor * diff + texColor * ambientLight;
                    }";
        }
    }
}
