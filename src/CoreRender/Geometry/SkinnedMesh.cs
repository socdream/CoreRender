using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using CoreMath;
using CoreRender.Animation;

namespace CoreRender.Geometry
{
    public class SkinnedMesh : Mesh
    {
        /// <summary>
        /// A single matrix that represents the transform of the bind-shape at the time when 
        /// the mesh was bound to a skeleton.This matrix transforms the bind-shape from object space to 
        /// bind-space.
        /// </summary>
        public float[] BindShapeMatrix { get; set; } = new float[] { }.IdentityMatrix();

        public List<Bone> Bones { get; set; } = new List<Bone>();
        
        public Animator Animator { get; set; }

        public float AnimationFrame { get; set; } = 0;

        public SkinnedMesh() : base() { }
        public SkinnedMesh(GeometryData data, List<Bone> bones)
            : base(data)
        {
            Bones = ((Bone[])bones.ToArray().Clone()).ToList();
        }
        
        public static SkinnedMesh FromFBX(float[] vertices, int[] indices, int indicesPerFace, float[] normals, float[] uv, int[] uvIndices, List<CoreFBX.FBX.Deformer> deformers, List<Bone> bones, float[] bindShapeMatrix = null)
        {
            var mesh = new SkinnedMesh();

            var data = new MeshHelper.PositionNormalUV0SkinVertex[indices.Length];
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
                var vertex = (bindShapeMatrix != null) ?
                    new float[] {
                        vertices[indices[i] * 3],
                        vertices[indices[i] * 3 + 1],
                        vertices[indices[i] * 3 + 2]
                    }.VectorTransform(bindShapeMatrix)
                    : new float[] {
                        vertices[indices[i] * 3],
                        vertices[indices[i] * 3 + 1],
                        vertices[indices[i] * 3 + 2]
                    };

                data[i].PosX = vertex[0];
                data[i].PosY = vertex[1];
                data[i].PosZ = vertex[2];
                data[i].NormalX = normals[i * 3];
                data[i].NormalY = normals[i * 3 + 1];
                data[i].NormalZ = normals[i * 3 + 2];
                data[i].UvX = uv[uvIndices[i] * 2];
                data[i].UvY = 1f - uv[uvIndices[i] * 2 + 1];

                meshIndices[i] = i;
            }

            mesh.Bones = bones;

            // add skin information
            for (int b = 0; b < deformers.Count; b++)
            {
                var bone = bones.Where(a => a.Id == deformers[b].BoneId).FirstOrDefault();
                var boneId = bones.IndexOf(bone);

                bone.InverseBindMatrix = deformers[b].TransformLink.MatrixInverse().TransposeMatrix(); //deformers[b].Transform.MatrixProduct(deformers[b].TransformLink.MatrixInverse());// deformers[b].TransformLink.TransposeMatrix().MatrixInverse().MatrixProduct(deformers[b].TransformLink.TransposeMatrix());

                for (int indexId = 0; indexId < deformers[b].Indexes.Length; indexId++)
                {
                    // the weight could be needed in more than one vertex
                    for (var i = 0; i < indices.Length; i++)
                    {
                        if (indices[i] == deformers[b].Indexes[indexId])
                        {
                            if (data[i].Weight1 == 0)
                            {
                                data[i].BoneId1 = boneId;
                                data[i].Weight1 = deformers[b].Weights[indexId];
                            }
                            else if (data[i].Weight2 == 0)
                            {
                                data[i].BoneId2 = boneId;
                                data[i].Weight2 = deformers[b].Weights[indexId];
                            }
                            else if (data[i].Weight3 == 0)
                            {
                                data[i].BoneId3 = boneId;
                                data[i].Weight3 = deformers[b].Weights[indexId];
                            }
                            else if (data[i].Weight4 == 0)
                            {
                                data[i].BoneId4 = boneId;
                                data[i].Weight4 = deformers[b].Weights[indexId];
                            }
                        }
                    }
                }
            }

            // normalize weights
            for (int i = 0; i < data.Length; i++)
            {
                var totalWeights = data[i].Weight1 + data[i].Weight2 + data[i].Weight3 + data[i].Weight4;

                if (totalWeights != 1f)
                {
                    var normalizedWeight = 1.0f / totalWeights;
                    data[i].Weight1 *= normalizedWeight;
                    data[i].Weight2 *= normalizedWeight;
                    data[i].Weight3 *= normalizedWeight;
                    data[i].Weight4 *= normalizedWeight;
                }
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
            GL.BufferData(BufferTarget.ArrayBuffer, 16 * 4 * data.Length, data, BufferUsageHint.StaticDraw);

            mesh.ElementBuffer = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.ElementBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, meshIndices.Length * 4, meshIndices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 16 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 16 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 16 * sizeof(float), 6 * sizeof(float));
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribIPointer(3, 4, VertexAttribIntegerType.Int, 16 * sizeof(float), IntPtr.Add(IntPtr.Zero, 8 * sizeof(float)));
            GL.EnableVertexAttribArray(3);
            GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, 16 * sizeof(float), 12 * sizeof(float));
            GL.EnableVertexAttribArray(4);

            mesh.ElementBufferSize = meshIndices.Length;

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            return mesh;
        }
        
        private void SetSkeleton(int frame, Bone parent = null)
        {
            var children = (parent != null) ? 
                Bones.Where(a => parent.Children.Contains(a.Id)) :
                Bones.Where(a => string.IsNullOrEmpty(a.Parent));

            foreach (var child in children)
            {
                child.WorldMatrix = child.JointMatrix;

                if (frame > -1)
                {
                    var key = child.Keyframes.Where(a => a.Frame == frame).FirstOrDefault();

                    if (key != null)
                        child.WorldMatrix = key.Transform;
                    else
                        child.WorldMatrix = new float[] { }.IdentityMatrix();
                }

                if (parent != null)
                    child.WorldMatrix = child.WorldMatrix.MatrixProduct(parent.WorldMatrix);

                SetSkeleton(frame, child);
            }
        }

        /*
         The skinning calculation for each vertex v in a bind shape is
         for i to n
              v += {[(v * BSM) * IBMi * JMi] * JW}
 
         • n: The number of joints that influence vertex v
         • BSM: Bind-shape matrix
         • IBMi: Inverse bind-pose matrix of joint i
         • JMi: Transformation matrix of joint i
         • JW: Weight of the influence of joint i on vertex v
         */
        public float[] GetSkinningMatrices(int frame)
        {
            var result = new float[16 * Bones.Count];

            SetSkeleton(frame);
            
            for (int i = 0; i < Bones.Count; i++)
            {
                var matrix = Bones[i].InverseBindMatrix.MatrixProduct(Bones[i].WorldMatrix);
                //matrix = new float[] { }.IdentityMatrix();
                
                Array.Copy(matrix, 0, result, i * 16, 16);
            }

            return result;
        }
    }
}
