using System;
using System.Collections.Generic;
using System.Text;
using CoreMath;
using CoreRender.Shaders;
using OpenTK.Graphics.OpenGL4;

namespace CoreRender.Geometry
{
    public class InstancedMesh : Mesh
    {
        public int Instances { get; set; }
        public override int InstanceBuffer { get; set; } = GL.GenBuffer();

        public InstancedMesh(GeometryData data) : base(data)
        {
        }

        public void SetInstanceData(int count, List<float> transforms)
        {
            Instances = count;

            GL.BindBuffer(BufferTarget.ArrayBuffer, InstanceBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, count * 4 * 16, transforms.ToArray(), BufferUsageHint.StaticDraw);
        }

        public override void Draw(Camera camera, float[] parentTransform = null)
        {
            ShaderManager.UseProgram(Shader.Program);

            if (!(camera is null))
            {
                Shader.ViewMatrix = camera.ViewMatrix;
                Shader.ProjectionMatrix = camera.ProjectionMatrix;
            }

            if (ElementBuffer == -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, Buffer);

                GL.DrawArraysInstanced(PrimitiveType, 0, VertexCount, Instances);
            }
            else
            {
                // When drawing we only need to bind the VertexArrayObject
                GL.BindVertexArray(VertexArrayObject);

                GL.DrawElementsInstanced(PrimitiveType, ElementBufferSize, DrawElementsType.UnsignedInt, IntPtr.Zero, Instances);
            }
        }
    }
}
