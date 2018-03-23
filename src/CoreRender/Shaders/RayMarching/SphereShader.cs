using System;
using System.Collections.Generic;
using System.Text;

namespace CoreRender.Shaders.RayMarching
{
    public class SphereShader : Shader
    {
        public SphereShader()
        {
            VertexSource = @"#version 330
                    const vec2 madd=vec2(0.5,0.5);
                    layout (location = 0) in vec3 position;
                    layout (location = 1) in vec3 normal;
                    layout (location = 2) in vec2 uv;
                    uniform mat4 projMat;
                    uniform mat4 modelMat;

                    out vec2 vecUv;
                    out vec3 cameraOrigin;
                    out vec3 rayDir;

                    void main()
                    {
                        gl_Position = vec4(position.xz,0.0,1.0);
                        
                        // camera settings
                        cameraOrigin = vec3(2.0, -2.0, 2.0);
                        vec3 cameraTarget = vec3(0.0, 0.0, 0.0);
                        vec3 upDirection = vec3(0.0, 1.0, 0.0);
                        vec3 cameraDir = normalize(cameraTarget - cameraOrigin);
                        vec3 cameraRight = normalize(cross(upDirection, cameraOrigin));
                        vec3 cameraUp = cross(cameraDir, cameraRight);

                        vecUv = uv;
                        vec2 screenPos = -1.0 + 2.0 * uv; // screenPos can range from -1 to 1
                        screenPos.x *= 800.0 / 600.0; // iResolution.x / iResolution.y; // Correct aspect ratio
                        rayDir = normalize(cameraRight * screenPos.x + cameraUp * screenPos.y + cameraDir);
                    }";

            FragmentSource = @"#version 330
                    in vec2 vecUv;
                    in vec3 cameraOrigin;
                    in vec3 rayDir;

                    uniform vec3 lightDir = normalize(vec3(-1.0f, -1.0f, -1.0f));
                    uniform vec4 ambientLight = vec4(0.3f, 0.3f, 0.3f, 1.0f);
                    uniform sampler2D diffuseTexture;

                    uniform mat4 viewMat;

                    const int MAX_ITER = 256; // 100 is a safe number to use, it won't produce too many artifacts and still be quite fast
                    const float MAX_DIST = 100.0; // Make sure you change this if you have objects farther than 20 units away from the camera
                    const float EPSILON = 0.001; // At this distance we are close enough to the object that we have essentially hit it

                    out vec4 fragColor;

                    float sphere(vec3 pos, float radius)
                    {
                        return length(pos) - radius;
                    }

                    float distfunc(vec3 pos)
                    {
                        return sphere(pos, 1.0);
                    }

                    vec4 lightning(vec3 pos)
                    {
                        vec2 eps = vec2(0.0, EPSILON);
                        vec3 normal = normalize(vec3(
                            distfunc(pos + eps.yxx) - distfunc(pos - eps.yxx),
                            distfunc(pos + eps.xyx) - distfunc(pos - eps.xyx),
                            distfunc(pos + eps.xxy) - distfunc(pos - eps.xxy)));

                        float diffuse = max(0.0, dot(-lightDir, normal));
                        float specular = pow(diffuse, 32.0);
                        vec3 color = vec3(diffuse + specular);
                        return vec4(color, 1.0);
                    }

                    vec4 raymarch()
                    {
                        float totalDist = 0.0;
                        vec3 pos = cameraOrigin;
                        float dist = EPSILON;

                        for (int i = 0; i < MAX_ITER; i++)
                        {
                            // Either we've hit the object or hit nothing at all, either way we should break out of the loop
                            if (dist < EPSILON || totalDist > MAX_DIST)
                                break; // If you use windows and the shader isn't working properly, change this to continue;

                            dist = distfunc(pos); // Evalulate the distance at the current point
                            totalDist += dist;
                            pos += dist * rayDir; // Advance the point forwards in the ray direction by the distance
                        }

                        if (dist < EPSILON)
                        {
                            // Lighting code
                            return lightning(pos);
                        }
                        else
                        {
                            //return vec4(0.0, 0.0, 1.0, 1.0);
                            return vec4(0.0);
                        }
                    }

                    void main()
                    {
                        fragColor = raymarch();
                    }";
        }
    }
}
