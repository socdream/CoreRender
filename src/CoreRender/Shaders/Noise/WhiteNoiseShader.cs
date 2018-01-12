using System;
using System.Collections.Generic;
using System.Text;

namespace CoreRender.Shaders.Noise
{
    public class WhiteNoiseShader : Shader
    {
        private Uniform _seed = new Uniform()
        {
            Name = "seed",
            Value = 0f
        };

        public float Seed
        {
            get
            {
                return (float)_seed.Value;
            }
            set
            {
                if ((float)_seed.Value == value)
                    return;

                _seed.Value = value;

                ShaderManager.SetUniform(this, _seed);
            }
        }

        private Uniform _color1 = new Uniform()
        {
            Name = "color1",
            Value = new float[] { 0f, 0f, 0f }
        };
        public float[] Color1
        {
            get
            {
                return (float[])_color1.Value;
            }
            set
            {
                if ((float[])_color1.Value == value)
                    return;

                _color1.Value = value;

                ShaderManager.SetUniform(this, _color1);
            }

        }

        private Uniform _color2 = new Uniform()
        {
            Name = "color2",
            Value = new float[] { 1.0f, 1.0f, 1.0f }
        };
        public float[] Color2
        {
            get
            {
                return (float[])_color2.Value;
            }
            set
            {
                if ((float[])_color2.Value == value)
                    return;

                _color2.Value = value;

                ShaderManager.SetUniform(this, _color2);
            }
        }

        public WhiteNoiseShader()
        {
            VertexSource = @"#version 330
  
                    layout (location = 0) in vec3 position;
                    layout (location = 1) in vec3 normal;

                    uniform mat4 viewMat;
                    uniform mat4 projMat;
                    uniform mat4 modelMat;

                    out vec3 vecNormal;
                    out vec2 vecPos;

                    void main()
                    {
                        gl_Position = projMat * viewMat * modelMat * vec4(position.x, position.y, position.z, 1.0);
                        vecNormal = normal;
                        vecPos = position.xy;
                    }";

            FragmentSource = @"#version 330
                    in vec3 vecNormal;
                    in vec2 vecPos;

                    uniform vec3 lightDir = vec3(1.0f, 1.0f, 1.0f);
                    uniform vec4 ambientLight = vec4(0.3f, 0.3f, 0.3f, 1.0f);

                    uniform float seed = 12.25f;
                    uniform vec3 color1 = vec3(0.0f, 0.0f, 0.0f);
                    uniform vec3 color2 = vec3(1.0f, 1.0f, 1.0f);

                    out vec4 color;

                    highp float rand(vec2 co)
                    {
                        highp float a = 12.9898;
                        highp float b = 78.233;
                        highp float c = 43758.5453;
                        highp float dt= dot(co.xy ,vec2(a,b));
                        highp float sn= mod(dt,3.14);
                        return fract(sin(sn) * c);
                    }

                    void main()
                    {
                        float noise = rand(vecPos);
                        
                        vec3 finalColor = (color1 * noise) + (color2 * (1.0f - noise));

                        vec3 diff = finalColor * max(dot(normalize(vecNormal), normalize(lightDir)), 0.0);
                        color = vec4((diff + ambientLight).xyz, 1.0);
                        //color = vec4(1.0f, 0f, 0f, 1.0f);
                    }";
        }
    }
}
