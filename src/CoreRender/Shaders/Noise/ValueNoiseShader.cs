using System;
using System.Collections.Generic;
using System.Text;

namespace CoreRender.Shaders.Noise
{
    public class ValueNoiseShader : Shader
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

        public ValueNoiseShader()
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

                    float hash(vec2 p)  // replace this by something better
                    {
                        p = floor(p);
                        p = 50.0*fract( p*0.3183099 + vec2(0.71,0.113));
                        return -1.0+2.0*fract( p.x*p.y*(p.x+p.y) );
                    }

                    float noise( in vec2 p )
                    {
                        vec2 i = floor( p );
                        vec2 f = fract( p );
	
	                    vec2 u = f*f*(3.0-2.0*f);

                        return mix( mix( hash( i + vec2(0.0,0.0) ), 
                                         hash( i + vec2(1.0,0.0) ), u.x),
                                    mix( hash( i + vec2(0.0,1.0) ), 
                                         hash( i + vec2(1.0,1.0) ), u.x), u.y);
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
