// The MIT License
// Copyright © 2013 Inigo Quilez
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions: The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software. THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.


// Gradient Noise (http://en.wikipedia.org/wiki/Gradient_noise), not to be confused with
// Value Noise, and neither with Perlin's Noise (which is one form of Gradient Noise)
// is probably the most convenient way to generate noise (a random smooth signal with 
// mostly all its energy in the low frequencies) suitable for procedural texturing/shading,
// modeling and animation.
//
// It produces smoother and higher quality than Value Noise, but it's of course slighty more
// expensive.
//
// The princpiple is to create a virtual grid/latice all over the plane, and assign one
// random vector to every vertex in the grid. When querying/requesting a noise value at
// an arbitrary point in the plane, the grid cell in which the query is performed is
// determined (line 32), the four vertices of the grid are determined and their random
// vectors fetched (lines 37 to 40). Then, the position of the current point under 
// evaluation relative to each vertex is doted (projected) with that vertex' random
// vector, and the result is bilinearly interpolated (lines 37 to 40 again) with a 
// smooth interpolant (line 33 and 35).


// Value    Noise 2D, Derivatives: https://www.shadertoy.com/view/4dXBRH
// Gradient Noise 2D, Derivatives: https://www.shadertoy.com/view/XdXBRH
// Value    Noise 3D, Derivatives: https://www.shadertoy.com/view/XsXfRH
// Gradient Noise 3D, Derivatives: https://www.shadertoy.com/view/4dffRH
// Value    Noise 2D             : https://www.shadertoy.com/view/lsf3WH
// Value    Noise 3D             : https://www.shadertoy.com/view/4sfGzS
// Gradient Noise 2D             : https://www.shadertoy.com/view/XdXGW8
// Gradient Noise 3D             : https://www.shadertoy.com/view/Xsl3Dl
// Simplex  Noise 2D             : https://www.shadertoy.com/view/Msf3WH
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreRender.Shaders.Noise
{
    public class GradientNoiseShader : Shader
    {
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

        public GradientNoiseShader()
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

                    uniform vec3 color1 = vec3(0.0f, 0.0f, 0.0f);
                    uniform vec3 color2 = vec3(1.0f, 1.0f, 1.0f);

                    uniform float width = 800.0f;
                    uniform float height = 600.0f;

                    out vec4 color;

                    vec2 hash( vec2 x )  // replace this by something better
                    {
                        const vec2 k = vec2( 0.3183099, 0.3678794 );
                        x = x*k + k.yx;
                        return -1.0 + 2.0*fract( 16.0 * k*fract( x.x*x.y*(x.x+x.y)) );
                    }

                    float noise( in vec2 p )
                    {
                        vec2 i = floor( p );
                        vec2 f = fract( p );
	
	                    vec2 u = f*f*(3.0-2.0*f);

                        return mix( mix( dot( hash( i + vec2(0.0,0.0) ), f - vec2(0.0,0.0) ), 
                                         dot( hash( i + vec2(1.0,0.0) ), f - vec2(1.0,0.0) ), u.x),
                                    mix( dot( hash( i + vec2(0.0,1.0) ), f - vec2(0.0,1.0) ), 
                                         dot( hash( i + vec2(1.0,1.0) ), f - vec2(1.0,1.0) ), u.x), u.y);
                    }

                    float smoothNoise( in vec2 uv, in vec2 resolution )
                    {
	                    float f = 0.0;
	
                        // fractal noise (4 octaves)
                        uv *= 8.0;
                        mat2 m = mat2( 1.6,  1.2, -1.2,  1.6 );
                        f  = 0.5000*noise( uv ); uv = m*uv;
                        f += 0.2500*noise( uv ); uv = m*uv;
                        f += 0.1250*noise( uv ); uv = m*uv;
                        f += 0.0625*noise( uv ); uv = m*uv;

	                    f = 0.5 + 0.5*f;

                        return f;
                    }

                    void main()
                    {
                        float noise = smoothNoise(vecUV, vec2(width, height));
                        
                        vec3 finalColor = (color1 * noise) + (color2 * (1.0f - noise));

                        vec3 diff = finalColor * max(dot(normalize(vecNormal), normalize(lightDir)), 0.0);
                        color = vec4((diff + ambientLight).xyz, 1.0);
                        //color = vec4(vecUV, 0.0f, 1.0f);
                        //color = vec4(1.0f, 0f, 0f, 1.0f);
                    }";
        }
    }
}
