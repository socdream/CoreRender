using System;
using System.Collections.Generic;
using System.Text;

namespace CoreRender.Shaders.Noise
{
    // https://www.shadertoy.com/view/ltB3zD
    public class GoldNoiseShader : Shader
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

        public GoldNoiseShader()
        {
            VertexSource = @"#version 330
  
                    layout (location = 0) in vec3 position;
                    layout (location = 1) in vec3 normal;
                    layout (location = 2) in vec2 uv;

                    uniform mat4 viewMat;
                    uniform mat4 projMat;
                    uniform mat4 modelMat;

                    out vec3 vecNormal;
                    out vec2 vecPos;
                    out vec2 vecUV;

                    void main()
                    {
                        gl_Position = projMat * viewMat * modelMat * vec4(position.x, position.y, position.z, 1.0);
                        vecNormal = normal;
                        vecPos = position.xy;
                        vecUV = uv;
                    }";

            FragmentSource = @"#version 330
                    in vec3 vecNormal;
                    in vec2 vecPos;
                    in vec2 vecUV;

                    uniform vec3 lightDir = vec3(1.0f, 1.0f, 1.0f);
                    uniform vec4 ambientLight = vec4(0.3f, 0.3f, 0.3f, 1.0f);

                    uniform float seed = 12.25f;
                    uniform vec3 color1 = vec3(0.0f, 0.0f, 0.0f);
                    uniform vec3 color2 = vec3(1.0f, 1.0f, 1.0f);

                    out vec4 color;

                    // Gold Noise ©2017 dcerisano@standard3d.com
                    //  - based on the golden ratio, PI, and the square root of two
                    //  - faster one-line fractal noise generator function
                    //  - improved random distribution
                    //  - works with all chipsets (including low precision)
                    //  - gpu-optimized floating point operations (faster than integer)
                    //  - does not contain any slow division or unsupported bitwise operations

                    // Use mediump or highp for improved random distribution.
                    // This line can be removed for low precision chipsets and older GL versions.

                    // precision highp   float;
                    // precision mediump float;
                    // precision lowp    float;

                    // Irrationals with precision shifting
                    float PHI = 1.61803398874989484820459 * 00000.1f; // Golden Ratio   
                    float PI  = 3.14159265358979323846264 * 00000.1f; // PI
                    float SRT = 1.41421356237309504880169 * 10000.0f; // Square Root of Two

                    // Gold Noise function
                    //
                    float gold_noise(in vec2 coordinate, in float seed)
                    {
                        return fract(sin(dot(coordinate*seed, vec2(PHI, PI)))*SRT);
                    }

                    void main()
                    {
                        float noise = gold_noise(vecUV, seed);
                        
                        vec3 finalColor = (color1 * noise) + (color2 * (1.0f - noise));

                        vec3 diff = finalColor * max(dot(normalize(vecNormal), normalize(lightDir)), 0.0);
                        color = vec4((diff + ambientLight).xyz, 1.0);
                        //color = vec4(1.0f, 0f, 0f, 1.0f);
                    }";
        }
    }
}
