using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreRender.Shaders
{
    public class SkinnedMeshShader : Shader
    {
        public SkinnedMeshShader()
        {
            VertexSource = @"#version 330
  
                    layout (location = 0) in vec3 position;
                    layout (location = 1) in vec3 normal;
                    layout (location = 2) in vec2 uv;
                    layout (location = 3) in ivec4 boneIDs;
                    layout (location = 4) in vec4 weights;

                    uniform mat4 viewMat;
                    uniform mat4 projMat;
                    uniform mat4 modelMat;

                    const int MAX_BONES = 100;

                    uniform mat4 gBones[MAX_BONES];

                    out vec3 vecNormal;
                    out vec2 vecUv;

                    void main()
                    {
                        /*mat4 BoneTransform = gBones[boneIDs[0]] * weights[0];
                        BoneTransform += gBones[boneIDs[1]] * weights[1];
                        BoneTransform += gBones[boneIDs[2]] * weights[2];
                        BoneTransform += gBones[boneIDs[3]] * weights[3];

                        gl_Position = projMat * viewMat * BoneTransform * vec4(position, 1.0);
                        //gl_Position = projMat * viewMat * BoneTransform * vec4(position, 1.0);
                        //gl_Position = projMat * viewMat * BoneTransform * modelMat * vec4(position, 1.0);
                        //gl_Position = projMat * viewMat * modelMat * BoneTransform * vec4(position, 1.0);
                        //gl_Position = projMat * viewMat * modelMat * vec4(position, 1.0);
                        vecNormal = (modelMat * BoneTransform * vec4(normal, 0.0)).xyz;
                        vecUv = uv;*/

                        vec4 BoneTransform = (gBones[boneIDs[0]] * vec4(position, 1.0)) * weights[0];
                        BoneTransform += (gBones[boneIDs[1]] * vec4(position, 1.0)) * weights[1];
                        BoneTransform += (gBones[boneIDs[2]] * vec4(position, 1.0)) * weights[2];
                        BoneTransform += (gBones[boneIDs[3]] * vec4(position, 1.0)) * weights[3];

                        gl_Position = projMat * viewMat * modelMat * vec4(BoneTransform.xyz, 1.0);
                        vecNormal = (modelMat * BoneTransform * vec4(normal, 0.0)).xyz;
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
                        //color = vec4(1.0f, 0.5f, 0.2f, 1.0f) * diff;
                        //color = vec4(vecNormal, 1.0f);
                        //color = vec4(vecUv, 0.0, 1.0f);
                        //color = texture2D(diffuseTexture, vecUv.st);
                        //color = boneColor;
                        vec4 texColor = texture2D(diffuseTexture, vecUv.st);
                        color = texColor * diff + texColor * ambientLight;
                    }";
        }
    }
}
