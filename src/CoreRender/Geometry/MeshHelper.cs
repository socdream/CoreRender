using CoreMath;
using CoreRender.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CoreRender.Geometry
{
    public class MeshHelper
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PositionNormalUV0Vertex
        {
            public float PosX { get; set; }
            public float PosY { get; set; }
            public float PosZ { get; set; }
            public float NormalX { get; set; }
            public float NormalY { get; set; }
            public float NormalZ { get; set; }
            public float UvX { get; set; }
            public float UvY { get; set; }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PositionVertex
        {
            public float PosX { get; set; }
            public float PosY { get; set; }
            public float PosZ { get; set; }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PositionNormalVertex
        {
            public float PosX { get; set; }
            public float PosY { get; set; }
            public float PosZ { get; set; }
            public float NormalX { get; set; }
            public float NormalY { get; set; }
            public float NormalZ { get; set; }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PositionNormalUV0SkinVertex
        {
            public float PosX { get; set; }
            public float PosY { get; set; }
            public float PosZ { get; set; }
            public float NormalX { get; set; }
            public float NormalY { get; set; }
            public float NormalZ { get; set; }
            public float UvX { get; set; }
            public float UvY { get; set; }
            public int BoneId1 { get; set; }
            public int BoneId2 { get; set; }
            public int BoneId3 { get; set; }
            public int BoneId4 { get; set; }
            public float Weight1 { get; set; }
            public float Weight2 { get; set; }
            public float Weight3 { get; set; }
            public float Weight4 { get; set; }
        }
        
        public struct VertexAttrib
        {
            /// <summary>
            /// Size in the datatype of the vertex a float3 has a size of 3
            /// </summary>
            public int Size { get; set; }
            public int Type { get; set; }
            /// <summary>
            /// Size in bytes of the vertex
            /// </summary>
            public int Stride { get; set; }
            /// <summary>
            /// Offset in bytes
            /// </summary>
            public int Offset { get; set; }
        }

        public static void ApplyVertexAttribs(VertexAttrib[] attribs)
        {
            for (int i = 0; i < attribs.Length; i++)
            {
                if(attribs[i].Type == (int)OpenTK.Graphics.OpenGL4.VertexAttribIntegerType.Int)
                {
                    OpenTK.Graphics.OpenGL4.GL.VertexAttribIPointer(i, attribs[i].Size, OpenTK.Graphics.OpenGL4.VertexAttribIntegerType.Int, attribs[i].Stride, IntPtr.Add(IntPtr.Zero, attribs[i].Offset));
                    OpenTK.Graphics.OpenGL4.GL.EnableVertexAttribArray(i);
                }
                else
                {
                    OpenTK.Graphics.OpenGL4.GL.VertexAttribPointer(i, attribs[i].Size, (OpenTK.Graphics.OpenGL4.VertexAttribPointerType)attribs[i].Type, false, attribs[i].Stride, attribs[i].Offset);
                    OpenTK.Graphics.OpenGL4.GL.EnableVertexAttribArray(i);
                }
            }
        }

        public static GeometryData FromVertexArray(float[] vertices, int verticesPerFace = 3, float[] bindShapeMatrix = null)
        {
            var data = new PositionVertex[vertices.Length / 3];
            var meshIndices = new int[vertices.Length / 3];

            for (var i = 0; i < vertices.Length / 3; i++)
            {
                var vertex = (bindShapeMatrix != null) ?
                    new float[] {
                        vertices[i * 3],
                        vertices[i * 3 + 1],
                        vertices[i * 3 + 2]
                    }.VectorTransform(bindShapeMatrix)
                    : new float[] {
                        vertices[i * 3],
                        vertices[i * 3 + 1],
                        vertices[i * 3 + 2]
                    };

                data[i].PosX = vertex[0];
                data[i].PosY = vertex[1];
                data[i].PosZ = vertex[2];

                meshIndices[i] = i;
            }

            // check if we need  to triangulate the mesh
            if (verticesPerFace == 4)
            {
                meshIndices = QuadIndicesToTriangles(meshIndices);
            }

            var result = new GeometryData()
            {
                VertexCount = data.Length,
                Indices = meshIndices
            };

            using (var stream = new System.IO.MemoryStream())
            using (var writer = new System.IO.BinaryWriter(stream))
            {
                for (int i = 0; i < data.Length; i++)
                {
                    writer.Write(data[i].PosX);
                    writer.Write(data[i].PosY);
                    writer.Write(data[i].PosZ);
                }

                result.Data = stream.ToArray();
            }

            // add vertex attribs
            result.Attribs = new VertexAttrib[]
            {
                new VertexAttrib()
                {
                    Size = 3,
                    Type = (int)OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float,
                    Stride = 3 * sizeof(float),
                    Offset = 0
                }
            };

            return result;
        }

        public static GeometryData FromVertexArray(float[] vertices, int[] indices, int indicesPerFace = 3, float[] bindShapeMatrix = null)
        {
            var data = new PositionVertex[indices.Length];
            var meshIndices = new int[indices.Length];

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

                meshIndices[i] = i;
            }

            // check if we need  to triangulate the mesh
            if (indicesPerFace == 4)
            {
                meshIndices = QuadIndicesToTriangles(meshIndices);
            }

            var result = new GeometryData()
            {
                VertexCount = data.Length,
                Indices = meshIndices
            };

            using (var stream = new System.IO.MemoryStream())
            using (var writer = new System.IO.BinaryWriter(stream))
            {
                for (int i = 0; i < data.Length; i++)
                {
                    writer.Write(data[i].PosX);
                    writer.Write(data[i].PosY);
                    writer.Write(data[i].PosZ);
                }

                result.Data = stream.ToArray();
            }

            // add vertex attribs
            result.Attribs = new VertexAttrib[]
            {
                new VertexAttrib()
                {
                    Size = 3,
                    Type = (int)OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float,
                    Stride = 3 * sizeof(float),
                    Offset = 0
                }
            };

            return result;
        }

        public static GeometryData FromCollada(float[] vertices, int[] indices, int indicesPerFace, float[] normals, int[] normalIndices, float[] bindShapeMatrix = null)
        {
            var data = new PositionNormalVertex[indices.Length];
            var meshIndices = new int[indices.Length];

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
                data[i].NormalX = normals[normalIndices[i] * 3];
                data[i].NormalY = normals[normalIndices[i] * 3 + 1];
                data[i].NormalZ = normals[normalIndices[i] * 3 + 2];

                meshIndices[i] = i;
            }

            // check if we need  to triangulate the mesh
            if (indicesPerFace == 4)
            {
                meshIndices = QuadIndicesToTriangles(meshIndices);
            }
            
            var result = new GeometryData()
            {
                VertexCount = data.Length,
                Indices = meshIndices
            };
            
            using (var stream = new System.IO.MemoryStream())
            using (var writer = new System.IO.BinaryWriter(stream))
            {
                for (int i = 0; i < data.Length; i++)
                {
                    writer.Write(data[i].PosX);
                    writer.Write(data[i].PosY);
                    writer.Write(data[i].PosZ);
                    writer.Write(data[i].NormalX);
                    writer.Write(data[i].NormalY);
                    writer.Write(data[i].NormalZ);
                }

                result.Data = stream.ToArray();
            }

            // add vertex attribs
            result.Attribs = new VertexAttrib[]
            {
                new VertexAttrib()
                {
                    Size = 3,
                    Type = (int)OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float,
                    Stride = 6 * sizeof(float),
                    Offset = 0
                },
                new VertexAttrib()
                {
                    Size = 3,
                    Type = (int)OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float,
                    Stride = 6 * sizeof(float),
                    Offset = 3 * sizeof(float)
                },
            };

            return result;
        }

        public static GeometryData FromCollada(float[] vertices, int[] indices, int indicesPerFace, float[] normals, int[] normalIndices, float[] uv, int[] uvIndices, float[] bindShapeMatrix = null)
        {
            var data = new PositionNormalUV0Vertex[indices.Length];
            var meshIndices = new int[indices.Length];

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
                data[i].NormalX = normals[normalIndices[i] * 3];
                data[i].NormalY = normals[normalIndices[i] * 3 + 1];
                data[i].NormalZ = normals[normalIndices[i] * 3 + 2];
                data[i].UvX = uv[uvIndices[i] * 2];
                data[i].UvY = 1f - uv[uvIndices[i] * 2 + 1];

                meshIndices[i] = i;
            }

            // check if we need  to triangulate the mesh
            if (indicesPerFace == 4)
            {
                meshIndices = QuadIndicesToTriangles(meshIndices);
            }
            
            var result = new GeometryData()
            {
                VertexCount = data.Length,
                Indices = meshIndices
            };

            using (var stream = new System.IO.MemoryStream())
            using (var writer = new System.IO.BinaryWriter(stream))
            {
                for (int i = 0; i < data.Length; i++)
                {
                    writer.Write(data[i].PosX);
                    writer.Write(data[i].PosY);
                    writer.Write(data[i].PosZ);
                    writer.Write(data[i].NormalX);
                    writer.Write(data[i].NormalY);
                    writer.Write(data[i].NormalZ);
                    writer.Write(data[i].UvX);
                    writer.Write(data[i].UvY);
                }

                result.Data = stream.ToArray();
            }

            // add vertex attribs
            result.Attribs = new VertexAttrib[]
            {
                new VertexAttrib()
                {
                    Size = 3,
                    Type = (int)OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float,
                    Stride = 8 * sizeof(float),
                    Offset = 0
                },
                new VertexAttrib()
                {
                    Size = 3,
                    Type = (int)OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float,
                    Stride = 8 * sizeof(float),
                    Offset = 3 * sizeof(float)
                },
                new VertexAttrib()
                {
                    Size = 2,
                    Type = (int)OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float,
                    Stride = 8 * sizeof(float),
                    Offset = 6 * sizeof(float)
                }
            };

            return result;
        }
        
        public static GeometryData FromCollada(float[] vertices, int[] indices, int indicesPerFace, float[] normals, int[] normalIndices, float[] uv, int[] uvIndices, List<Dictionary<string, float>> vertexWeights, List<Bone> bones, float[] bindShapeMatrix = null)
        {
            var data = new PositionNormalUV0SkinVertex[indices.Length];
            var meshIndices = new int[indices.Length];

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
                data[i].NormalX = normals[normalIndices[i] * 3];
                data[i].NormalY = normals[normalIndices[i] * 3 + 1];
                data[i].NormalZ = normals[normalIndices[i] * 3 + 2];
                data[i].UvX = uv[uvIndices[i] * 2];
                data[i].UvY = 1f - uv[uvIndices[i] * 2 + 1];

                // skinning
                var weight = 0;

                foreach (var b in vertexWeights[indices[i]].OrderByDescending(a => a.Value).Select(a => a.Key))
                {
                    if (vertexWeights[indices[i]][b] > 0)
                    {
                        if (weight == 0)
                        {
                            data[i].BoneId1 = bones.TakeWhile(a => a.Id != b).Count();
                            data[i].Weight1 = vertexWeights[indices[i]][b];
                        }
                        else if (weight == 1)
                        {
                            data[i].BoneId2 = bones.TakeWhile(a => a.Id != b).Count();
                            data[i].Weight2 = vertexWeights[indices[i]][b];
                        }
                        else if (weight == 2)
                        {
                            data[i].BoneId3 = bones.TakeWhile(a => a.Id != b).Count();
                            data[i].Weight3 = vertexWeights[indices[i]][b];
                        }
                        else if (weight == 3)
                        {
                            data[i].BoneId4 = bones.TakeWhile(a => a.Id != b).Count();
                            data[i].Weight4 = vertexWeights[indices[i]][b];
                        }

                        weight += 1;
                    }

                }

                // normalize weights
                var totalWeights = data[i].Weight1 + data[i].Weight2 + data[i].Weight3 + data[i].Weight4;
                if (totalWeights != 1f)
                {
                    var normalizedWeight = 1.0f / totalWeights;
                    data[i].Weight1 *= normalizedWeight;
                    data[i].Weight2 *= normalizedWeight;
                    data[i].Weight3 *= normalizedWeight;
                    data[i].Weight4 *= normalizedWeight;
                }

                meshIndices[i] = i;
            }
            
            // check if we need  to triangulate the mesh
            if (indicesPerFace == 4)
            {
                meshIndices = MeshHelper.QuadIndicesToTriangles(meshIndices);
            }

            var result = new GeometryData()
            {
                VertexCount = data.Length,
                Indices = meshIndices
            };

            using (var stream = new System.IO.MemoryStream())
            using (var writer = new System.IO.BinaryWriter(stream))
            {
                for (int i = 0; i < data.Length; i++)
                {
                    writer.Write(data[i].PosX);
                    writer.Write(data[i].PosY);
                    writer.Write(data[i].PosZ);
                    writer.Write(data[i].NormalX);
                    writer.Write(data[i].NormalY);
                    writer.Write(data[i].NormalZ);
                    writer.Write(data[i].UvX);
                    writer.Write(data[i].UvY);
                    writer.Write(data[i].BoneId1);
                    writer.Write(data[i].BoneId2);
                    writer.Write(data[i].BoneId3);
                    writer.Write(data[i].BoneId4);
                    writer.Write(data[i].Weight1);
                    writer.Write(data[i].Weight2);
                    writer.Write(data[i].Weight3);
                    writer.Write(data[i].Weight4);
                }

                result.Data = stream.ToArray();
            }

            // add vertex attribs
            result.Attribs = new VertexAttrib[]
            {
                new VertexAttrib()
                {
                    Size = 3,
                    Type = (int)OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float,
                    Stride = 16 * sizeof(float),
                    Offset = 0
                },
                new VertexAttrib()
                {
                    Size = 3,
                    Type = (int)OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float,
                    Stride = 16 * sizeof(float),
                    Offset = 3 * sizeof(float)
                },
                new VertexAttrib()
                {
                    Size = 2,
                    Type = (int)OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float,
                    Stride = 16 * sizeof(float),
                    Offset = 6 * sizeof(float)
                },
                new VertexAttrib()
                {
                    Size = 4,
                    Type = (int)OpenTK.Graphics.OpenGL4.VertexAttribIntegerType.Int,
                    Stride = 16 * sizeof(float),
                    Offset = 8 * sizeof(float)
                },
                new VertexAttrib()
                {
                    Size = 4,
                    Type = (int)OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float,
                    Stride = 16 * sizeof(float),
                    Offset = 12 * sizeof(float)
                }
            };

            return result;
        }

        public static GeometryData FromVertices(List<PositionNormalUV0Vertex> vertices, List<int> indices)
        {
            var result = new GeometryData()
            {
                VertexCount = vertices.Count,
                Indices = indices.ToArray()
            };

            using (var stream = new System.IO.MemoryStream())
            using (var writer = new System.IO.BinaryWriter(stream))
            {
                for (int i = 0; i < vertices.Count; i++)
                {
                    writer.Write(vertices[i].PosX);
                    writer.Write(vertices[i].PosY);
                    writer.Write(vertices[i].PosZ);
                    writer.Write(vertices[i].NormalX);
                    writer.Write(vertices[i].NormalY);
                    writer.Write(vertices[i].NormalZ);
                    writer.Write(vertices[i].UvX);
                    writer.Write(vertices[i].UvY);
                }

                result.Data = stream.ToArray();
            }

            // add vertex attribs
            result.Attribs = new VertexAttrib[]
            {
                new VertexAttrib()
                {
                    Size = 3,
                    Type = (int)OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float,
                    Stride = 8 * sizeof(float),
                    Offset = 0
                },
                new VertexAttrib()
                {
                    Size = 3,
                    Type = (int)OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float,
                    Stride = 8 * sizeof(float),
                    Offset = 3 * sizeof(float)
                },
                new VertexAttrib()
                {
                    Size = 2,
                    Type = (int)OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float,
                    Stride = 8 * sizeof(float),
                    Offset = 6 * sizeof(float)
                }
            };

            return result;
        }
        public static GeometryData FromVertices(List<PositionNormalVertex> vertices, List<int> indices)
        {
            var result = new GeometryData()
            {
                VertexCount = vertices.Count,
                Indices = indices.ToArray()
            };

            using (var stream = new System.IO.MemoryStream())
            using (var writer = new System.IO.BinaryWriter(stream))
            {
                for (int i = 0; i < vertices.Count; i++)
                {
                    writer.Write(vertices[i].PosX);
                    writer.Write(vertices[i].PosY);
                    writer.Write(vertices[i].PosZ);
                    writer.Write(vertices[i].NormalX);
                    writer.Write(vertices[i].NormalY);
                    writer.Write(vertices[i].NormalZ);
                }

                result.Data = stream.ToArray();
            }

            // add vertex attribs
            result.Attribs = new VertexAttrib[]
            {
                new VertexAttrib()
                {
                    Size = 3,
                    Type = (int)OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float,
                    Stride = 6 * sizeof(float),
                    Offset = 0
                },
                new VertexAttrib()
                {
                    Size = 3,
                    Type = (int)OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float,
                    Stride = 6 * sizeof(float),
                    Offset = 3 * sizeof(float)
                }
            };

            return result;
        }

        public static GeometryData CreateXYPlane(float[] bindShapeMatrix = null)
        {
            float[] vertices = new float[] {
                    -1, -1, 0, // bottom left corner
                    -1,  1, 0, // top left corner
                    1,  1, 0, // top right corner
                    1, -1, 0
                    };
            int[] indices = new int[] {
                        0,1,2, // first triangle (bottom left - top left - top right)
                        0,2,3 // second triangle (bottom left - top right - bottom right)
                    };
            float[] normals = new float[] {
                    0, 0, 1
                    };
            int[] normalIndices = new int[] {
                        0,0,0, // first triangle (bottom left - top left - top right)
                        0,0,0 // second triangle (bottom left - top right - bottom right)
                    };
            float[] uv = new float[]
                    {
                        1, 1,
                        1, 0,
                        0, 0,
                        0, 1
                    };
            int[] uvIndices = new int[] {
                        0,1,2, // first triangle (bottom left - top left - top right)
                        0,2,3 // second triangle (bottom left - top right - bottom right)
                    };

            var data = new PositionNormalUV0Vertex[indices.Length];
            var meshIndices = new int[indices.Length];

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
                data[i].NormalX = normals[normalIndices[i] * 3];
                data[i].NormalY = normals[normalIndices[i] * 3 + 1];
                data[i].NormalZ = normals[normalIndices[i] * 3 + 2];
                data[i].UvX = uv[uvIndices[i] * 2];
                data[i].UvY = uv[uvIndices[i] * 2 + 1];

                meshIndices[i] = i;
            }

            var result = new GeometryData()
            {
                VertexCount = data.Length,
                Indices = meshIndices
            };

            using (var stream = new System.IO.MemoryStream())
            using (var writer = new System.IO.BinaryWriter(stream))
            {
                for (int i = 0; i < data.Length; i++)
                {
                    writer.Write(data[i].PosX);
                    writer.Write(data[i].PosY);
                    writer.Write(data[i].PosZ);
                    writer.Write(data[i].NormalX);
                    writer.Write(data[i].NormalY);
                    writer.Write(data[i].NormalZ);
                    writer.Write(data[i].UvX);
                    writer.Write(data[i].UvY);
                }

                result.Data = stream.ToArray();
            }

            // add vertex attribs
            result.Attribs = new VertexAttrib[]
            {
                new VertexAttrib()
                {
                    Size = 3,
                    Type = (int)OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float,
                    Stride = 8 * sizeof(float),
                    Offset = 0
                },
                new VertexAttrib()
                {
                    Size = 3,
                    Type = (int)OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float,
                    Stride = 8 * sizeof(float),
                    Offset = 3 * sizeof(float)
                },
                new VertexAttrib()
                {
                    Size = 2,
                    Type = (int)OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float,
                    Stride = 8 * sizeof(float),
                    Offset = 6 * sizeof(float)
                }
            };

            return result;
        }

        public static int[] QuadIndicesToTriangles(int[] indices)
        {
            var result = new int[indices.Length / 4 * 6];

            for (var i = 0; i * 4 < indices.Length; i++)
            {
                result[i * 6] = indices[i * 4];
                result[i * 6 + 1] = indices[i * 4 + 1];
                result[i * 6 + 2] = indices[i * 4 + 2];
                result[i * 6 + 3] = indices[i * 4];
                result[i * 6 + 4] = indices[i * 4 + 2];
                result[i * 6 + 5] = indices[i * 4 + 3];
            }

            return result;
        }
    }
}
