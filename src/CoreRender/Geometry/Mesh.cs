using CoreMath;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CoreRender.Geometry
{
    public class Mesh : IDisposable
    { 
        public string Name { get; set; }

        public int Buffer { get; set; } = -1;
        public int VertexCount { get; set; }
        public OpenTK.Graphics.OpenGL4.PrimitiveType PrimitiveType { get; set; } = PrimitiveType.Triangles;
        public int VertexArrayObject { get; set; } = 0;
        public int ElementBuffer { get; set; } = -1;
        public int ElementBufferSize { get; set; } = 0;
        public int Texture { get; set; } = 0;
        public bool Enabled { get; set; } = true;

        public Type VertexType { get; set; } = typeof(MeshHelper.PositionNormalUV0Vertex);

        public Shaders.Shader Shader { get; set; }

        /// <summary>
        /// 3D vector
        /// </summary>
        public float[] Translation { get; set; } = new float[] { 0, 0, 0 };
        /// <summary>
        /// Quaternion
        /// </summary>
        public float[] Rotation { get; set; } = new float[] { 0, 0, 0, 1 };

        /// <summary>
        /// 3 scaling factors (x, y, z)
        /// </summary>
        public float[] Scaling { get; set; } = new float[] { 1, 1, 1, };

        public float[] Transform
        {
            get
            {
                return new float[] { }.MatrixCompose(Translation, Rotation, Scaling);
            }
            set
            {
                value.MatrixDecompose(out float[] t, out float[] r, out float[] s);

                Translation = t;
                Rotation = r;
                Scaling = s;
            }
        }

        public Mesh() { }

        public Mesh(float[] vertices, float[] normals, int[] indices)
        {
            var data = new float[vertices.Length * 2];

            // if the indices and the normals have the same length assign them directly
            if (indices.Length == normals.Length)
            {
                for (var i = 0; i < vertices.Length / 3; i++)
                {
                    data[i * 6] = vertices[i * 3];
                    data[i * 6 + 1] = vertices[i * 3 + 1];
                    data[i * 6 + 2] = vertices[i * 3 + 2];
                    data[i * 6 + 3] = normals[i * 3];
                    data[i * 6 + 4] = normals[i * 3 + 1];
                    data[i * 6 + 5] = normals[i * 3 + 2];
                }
            }
            else
            {
                // the normals are referred to the indices, not the vertices
                for (var i = 0; i < vertices.Length / 3; i++)
                {
                    data[i * 6] = vertices[i * 3];
                    data[i * 6 + 1] = vertices[i * 3 + 1];
                    data[i * 6 + 2] = vertices[i * 3 + 2];
                    data[i * 6 + 3] = 0;
                    data[i * 6 + 4] = 0;
                    data[i * 6 + 5] = 0;
                }

                // if the normals are normalized, adding them and then normalizing them will have the same effect
                for (var i = 0; i < indices.Length; i++)
                {
                    data[indices[i] * 6 + 3] = normals[i * 3];
                    data[indices[i] * 6 + 4] = normals[i * 3 + 1];
                    data[indices[i] * 6 + 5] = normals[i * 3 + 2];
                }
            }

            VertexCount = vertices.Length / 3;

            Buffer = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, Buffer);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * 2 * 4, data, BufferUsageHint.StaticDraw);

            ElementBuffer = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * 4, indices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            PrimitiveType = PrimitiveType.Triangles;

            ElementBufferSize = indices.Length;
        }

        public Mesh(float[] vertices, int[] indices, float[] normals, int[] normalIndices)
        {
            var data = new float[vertices.Length * 2];

            // the normals are referred to the indices, not the vertices
            for (var i = 0; i < vertices.Length / 3; i++)
            {
                data[i * 6] = vertices[i * 3];
                data[i * 6 + 1] = vertices[i * 3 + 1];
                data[i * 6 + 2] = vertices[i * 3 + 2];
                data[i * 6 + 3] = 0;
                data[i * 6 + 4] = 0;
                data[i * 6 + 5] = 0;
            }

            // if the normals are normalized, adding them and then normalizing them will have the same effect
            for (var i = 0; i < normalIndices.Length; i++)
            {
                data[normalIndices[i] * 6 + 3] = normals[i * 3];
                data[normalIndices[i] * 6 + 4] = normals[i * 3 + 1];
                data[normalIndices[i] * 6 + 5] = normals[i * 3 + 2];
            }

            VertexCount = vertices.Length / 3;

            Buffer = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, Buffer);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * 2 * 4, data, BufferUsageHint.StaticDraw);

            ElementBuffer = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * 4, indices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            PrimitiveType = PrimitiveType.Triangles;

            ElementBufferSize = indices.Length;
        }

        public Mesh(float[] vertices, int[] indices, float[] normals, int[] normalIndices, float[] uv, int[] uvIndices)
        {
            var data = new float[vertices.Length / 3 * 8];

            // the normals are referred to the indices, not the vertices
            for (var i = 0; i < vertices.Length / 3; i++)
            {
                data[i * 8] = vertices[i * 3];
                data[i * 8 + 1] = vertices[i * 3 + 1];
                data[i * 8 + 2] = vertices[i * 3 + 2];
                data[i * 8 + 3] = 0;
                data[i * 8 + 4] = 0;
                data[i * 8 + 5] = 0;
                data[i * 8 + 6] = 0;
                data[i * 8 + 7] = 0;
            }

            // if the normals are normalized, adding them and then normalizing them will have the same effect
            for (var i = 0; i < normalIndices.Length; i++)
            {
                data[normalIndices[i] * 8 + 3] += normals[i * 3];
                data[normalIndices[i] * 8 + 4] += normals[i * 3 + 1];
                data[normalIndices[i] * 8 + 5] += normals[i * 3 + 2];
            }

            for (var i = 0; i < uvIndices.Length; i++)
            {
                data[normalIndices[i] * 8 + 6] = uv[uvIndices[i] * 2];
                data[normalIndices[i] * 8 + 7] = uv[uvIndices[i] * 2 + 1];
            }

            VertexCount = vertices.Length / 3;

            Buffer = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, Buffer);
            GL.BufferData(BufferTarget.ArrayBuffer, data.Length * 4, data, BufferUsageHint.StaticDraw);

            ElementBuffer = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * 4, indices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
            GL.EnableVertexAttribArray(2);

            PrimitiveType = PrimitiveType.Triangles;

            ElementBufferSize = indices.Length;
        }

        public Mesh(GeometryData data)
        {
            VertexCount = data.VertexCount;

            VertexArrayObject = GL.GenVertexArray();

            GL.BindVertexArray(VertexArrayObject);

            Buffer = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, Buffer);
            GL.BufferData(BufferTarget.ArrayBuffer, data.Data.Length, data.Data, BufferUsageHint.StaticDraw);

            ElementBuffer = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, data.Indices.Length * 4, data.Indices, BufferUsageHint.StaticDraw);

            MeshHelper.ApplyVertexAttribs(data.Attribs);

            ElementBufferSize = data.Indices.Length;

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public static Mesh FromFBX(float[] vertices, int[] indices, int indicesPerFace, float[] normals, float[] uv, int[] uvIndices)
        {
            var mesh = new Mesh();
            
            var data = new MeshHelper.PositionNormalUV0Vertex[indices.Length];
            var meshIndices = new int[indices.Length];

            // the normals are referred to the indices, not the vertices if 
            //MappingInformationType
            //- ByPolygonVertex
            //ReferenceInformationType
            //- Direct
            // UV
            //MappingInformationType
            //- ByPolygonVertex
            //ReferenceInformationType
            //- IndexToDirect
            for (var i = 0; i < indices.Length; i++)
            {
                data[i].PosX = vertices[indices[i] * 3];
                data[i].PosY = vertices[indices[i] * 3 + 1];
                data[i].PosZ = vertices[indices[i] * 3 + 2];
                data[i].NormalX = normals[i * 3];
                data[i].NormalY = normals[i * 3 + 1];
                data[i].NormalZ = normals[i * 3 + 2];
                data[i].UvX = uv[uvIndices[i] * 2];
                data[i].UvY = 1f - uv[uvIndices[i] * 2 + 1];

                meshIndices[i] = i;
            }

            // check if we need  to triangulate the mesh
            if (indicesPerFace == 4)
            {
                meshIndices = MeshHelper.QuadIndicesToTriangles(meshIndices);
            }

            mesh.VertexCount = data.Length;

            mesh.VertexArrayObject = GL.GenVertexArray();

            GL.BindVertexArray(mesh.VertexArrayObject);

            mesh.Buffer = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, mesh.Buffer);
            GL.BufferData(BufferTarget.ArrayBuffer, 32 * data.Length, data, BufferUsageHint.StaticDraw);

            mesh.ElementBuffer = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.ElementBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, meshIndices.Length * 4, meshIndices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
            GL.EnableVertexAttribArray(2);

            mesh.ElementBufferSize = meshIndices.Length;

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            return mesh;
        }
        
        public void Draw()
        {
            if (ElementBuffer == -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, Buffer);

                GL.DrawArrays(PrimitiveType, 0, VertexCount);
            }
            else
            {
                // When drawing we only need to bind the VertexArrayObject
                GL.BindVertexArray(VertexArrayObject);

                GL.DrawElements(PrimitiveType, ElementBufferSize, DrawElementsType.UnsignedInt, 0);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                }

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                if (ElementBuffer != -1)
                    GL.DeleteBuffer(ElementBuffer);

                if (VertexArrayObject != -1)
                    GL.DeleteVertexArray(VertexArrayObject);

                if (Buffer != -1)
                    GL.DeleteBuffer(Buffer);

                disposedValue = true;
            }
        }
        
        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
