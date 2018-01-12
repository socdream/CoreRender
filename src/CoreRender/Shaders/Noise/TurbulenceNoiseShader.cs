using System;
using System.Collections.Generic;
using System.Text;

namespace CoreRender.Shaders.Noise
{
    public class TurbulenceNoiseShader : Shader
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

        private Uniform _width = new Uniform()
        {
            Name = "width",
            Value = 800.0f
        };
        public float Width
        {
            get
            {
                return (float)_width.Value;
            }
            set
            {
                if ((float)_width.Value == value)
                    return;

                _width.Value = value;

                ShaderManager.SetUniform(this, _width);
            }
        }

        private Uniform _height = new Uniform()
        {
            Name = "height",
            Value = 600.0f
        };
        public float Height
        {
            get
            {
                return (float)_height.Value;
            }
            set
            {
                if ((float)_height.Value == value)
                    return;

                _height.Value = value;

                ShaderManager.SetUniform(this, _height);
            }
        }

        public TurbulenceNoiseShader()
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

                    highp float rand(in vec2 co)
                    {
                        highp float a = 12.9898;
                        highp float b = 78.233;
                        highp float c = 43758.5453;
                        highp float dt= dot(co.xy ,vec2(a,b));
                        highp float sn= mod(dt,3.14);
                        return fract(sin(sn) * c);
                    }

                    float modulus(float a, float b){
                        return a - (b * floor(a/b));
                    }

                    float smoothNoise(float x, float y, float noiseWidth, float noiseHeight)
                    {
                        //get fractional part of x and y
                        float fractX = modulus(x, 1.0f);
                        float fractY = modulus(y, 1.0f);

                        //wrap around
                        float x1 = modulus(x + noiseWidth, noiseWidth);
                        float y1 = modulus(y + noiseHeight, noiseHeight);

                        //neighbor values
                        float x2 = modulus(x1 + noiseWidth - 1.0f, noiseWidth);
                        float y2 = modulus(y1 + noiseHeight - 1.0f, noiseHeight);

                        //smooth the noise with bilinear interpolation
                        float value = 0.0f;

                        value += fractX * fractY * rand(vec2(x1, y1));
                        value += (1.0f - fractX) * fractY * rand(vec2(x1, y2));
                        value += fractX * (1.0f - fractY) * rand(vec2(x2, y1));
                        value += (1.0f - fractX) * (1.0f - fractY) * rand(vec2(x2, y2));

                        return value;
                    }

                    float Turbulence(float x, float y, float size, float width, float height)
                    {
                        float value = 0.0f;
                        float initialSize = size;

                        while (size >= 1.0F)
                        {
                            value += smoothNoise(x / size, y / size, width, height) * size;
                            size /= 2.0f;
                        }

                        return (128.0f * value / initialSize) / 256.0f;
                    }

                    void main()
                    {
                        float noiseResult = Turbulence(vecPos.x, vecPos.y, 64.0f, 800.0f, 600.0f);
                        
                        vec3 finalColor = (color1 * noiseResult) + (color2 * (1.0f - noiseResult));

                        vec3 diff = finalColor * max(dot(normalize(vecNormal), normalize(lightDir)), 0.0);
                        color = vec4((diff + ambientLight).xyz, 1.0);
                        //color = vec4(1.0f, 0f, 0f, 1.0f);
                    }";
        }
    }
}
