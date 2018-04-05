using CoreMath;
using CoreRender.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CoreRender.Collada
{
    public class ColladaHelper
    {
        public static List<Mesh> GetMeshes(CoreCollada.Collada file, CoreCollada.Node parent = null, float[] parentTransform = null, string path = "")
        {
            var result = new List<Mesh>();
            var nodes = (parent != null) ? parent.Nodes : file.LibraryVisualScenes[0].Nodes;

            if (parentTransform == null)
            {
                /*parentTransform = (file.Asset.Unit.Meter != 1) ?
                    new float[] { file.Asset.Unit.Meter , file.Asset.Unit.Meter , file.Asset.Unit.Meter }.ScalingMatrix()
                    : new float[] { }.IdentityMatrix();*/
                parentTransform = new float[] { }.IdentityMatrix();
            }
            
            foreach (var node in nodes)
            {
                var transform = (node.MatrixValue != null) ?
                    node.MatrixValue.TransposeMatrix().MatrixProduct(parentTransform)
                    : parentTransform;

                if (node.InstanceGeometry != null)
                    result.AddRange(GetStaticMeshes(file, node, transform, path));

                if (node.InstanceController != null)
                    result.AddRange(GetSkinnedMeshes(file, node, transform, path));

                result.AddRange(GetMeshes(file, node, transform, path));
            }

            return result;
        }

        private static List<SkinnedMesh> GetSkinnedMeshes(CoreCollada.Collada file, CoreCollada.Node node, float[] transform = null, string path = "")
        {
            var result = new List<SkinnedMesh>();

            if (transform == null)
                transform = new float[] { }.IdentityMatrix();
            
            var materials = node.InstanceController.BindMaterial != null ?
                GetMaterials(node.InstanceController.BindMaterial.TechniqueCommon.InstanceMaterials, file, path)
                : new List<Material>();

            // get the controller
            var controllerId = node.InstanceController.Url.Substring(1);

            var controller = file.LibraryControllers.Where(a => a.Id == controllerId).FirstOrDefault();

            if (controller != null)
            {
                if (controller.Skin != null)
                {
                    // Get the bones and fill the right matrices for animation
                    var bones = GetColladaGeometryBones(controller, Animation.Bone.GetBoneHierarchy(file));

                    // get the geometry node
                    var geometryId = controller.Skin.Source.Substring(1);

                    // a geometry can be splitted in several ones depending on the material
                    var geometries = LoadGeometry(file.LibraryGeometries.Where(a => a.Id == geometryId).FirstOrDefault());

                    foreach (var geometry in geometries)
                    {
                        var material = materials.Where(a => a.Name == geometry.Material).FirstOrDefault();
                        
                        var vertexWeights = GetColladaVertexWeights(controller);
                        var bindShapeMatrix = controller.Skin.BindShapeMatrixValue.TransposeMatrix();

                        var geometryData = MeshHelper.FromCollada(geometry.Positions, geometry.VertexIndices, 3, geometry.Normals, geometry.NormalIndices, geometry.UVs, geometry.UV0Indices, vertexWeights, bones, bindShapeMatrix);
                        var mesh = new SkinnedMesh(geometryData, bones)
                        {
                            Texture = material != null ? material.Texture : 0,
                            Name = node.Name
                        };

                        // set up shader
                        var shader = Shaders.ShaderManager.LoadShader<Shaders.SkinnedMeshShader>();

                        mesh.Shader = shader;
                        mesh.Transform = transform;

                        result.Add(mesh);
                    }
                }
            }

            return result;
        }

        private static List<Mesh> GetStaticMeshes(CoreCollada.Collada file, CoreCollada.Node node, float[] transform = null, string path = "")
        {
            var result = new List<Mesh>();

            if (transform == null)
                transform = new float[] { }.IdentityMatrix();
            
            var materials = node.InstanceGeometry.BindMaterial != null ?
                GetMaterials(node.InstanceGeometry.BindMaterial.TechniqueCommon.InstanceMaterials, file, path)
                : new List<Material>();

            var geometryId = node.InstanceGeometry.Url.Substring(1);

            var geometries = LoadGeometry(file.LibraryGeometries.Where(a => a.Id == geometryId).FirstOrDefault());

            foreach(var geometry in geometries)
            {
                var material = materials.Where(a => a.Name == geometry.Material).FirstOrDefault();

                var mesh = geometry.UVs != null ?
                    new Mesh(MeshHelper.FromCollada(geometry.Positions, geometry.VertexIndices, 3, geometry.Normals, geometry.NormalIndices, geometry.UVs, geometry.UV0Indices))
                    : new Mesh(MeshHelper.FromCollada(geometry.Positions, geometry.VertexIndices, 3, geometry.Normals, geometry.NormalIndices));
                    //Mesh.FromCollada(geometry.Positions, geometry.VertexIndices, 3, geometry.Normals, geometry.NormalIndices, geometry.UVs, geometry.UV0Indices)
                    //: Mesh.FromCollada(geometry.Positions, geometry.VertexIndices, 3, geometry.Normals, geometry.NormalIndices);

                mesh.Texture = material != null ? material.Texture : 0;
                mesh.Name = node.Name;

                // set up shader
                var shader = mesh.VertexType == typeof(MeshHelper.PositionNormalVertex) ?
                    (Shaders.Shader)Shaders.ShaderManager.LoadShader<Shaders.MeshShader>():
                    (Shaders.Shader)Shaders.ShaderManager.LoadShader<Shaders.TexturedMeshShader>();

                mesh.Shader = shader;
                mesh.Transform = transform;

                result.Add(mesh);
            }

            return result;
        }

        public struct ColladaGeometry
        {
            public string Material { get; set; }
            public float[] Positions { get; set; }
            public float[] Normals { get; set; }
            public float[] UVs { get; set; }
            public int[] VertexIndices { get; set; }
            public int[] NormalIndices { get; set; }
            public int[] UV0Indices { get; set; }
            public int[] UV1Indices { get; set; }
        }

        public static List<ColladaGeometry> LoadGeometry(CoreCollada.Geometry geometry)
        {
            // usually geometry has 1 mesh with 3 source nodes
            var result = new List<ColladaGeometry>();

            foreach (var triangleList in geometry.Mesh.Triangles)
            {
                var trianglesCount = triangleList.Count;

                if (trianglesCount > 0)
                {
                    var newGeometry = new ColladaGeometry()
                    {
                        Material = triangleList.Material,
                        Positions = geometry.Mesh.Sources[0].FloatValues,
                        Normals = geometry.Mesh.Sources[1].FloatValues,
                        UVs = (geometry.Mesh.Sources.Count > 2) ? geometry.Mesh.Sources[2].FloatValues : null,
                        VertexIndices = new int[trianglesCount * 3],
                        NormalIndices = new int[trianglesCount * 3],
                        UV0Indices = (geometry.Mesh.Sources.Count > 2) ? new int[trianglesCount * 3] : null,
                        UV1Indices = (geometry.Mesh.Sources.Count > 3) ? new int[trianglesCount * 3] : null
                    };

                    var vertexOffset = triangleList.Inputs[0].Offset;
                    var normalOffset = triangleList.Inputs[1].Offset;
                    var uvs0Offset = newGeometry.UVs != null ? triangleList.Inputs[2].Offset : 0;
                    var uvs1Offset = newGeometry.UVs != null && triangleList.Inputs.Count > 3 ? triangleList.Inputs[3].Offset : 0;

                    // Structure of indices can be:
                    // (Position + Normal)             V1(P N)         V2(P N)         V3(P N)
                    // (Position + Normal + UV0)       V1(P N UV0)     V2(P N UV0)     V3(P N UV0)
                    // (Position + Normal + UV0 + UV1) V1(P N UV0 UV1) V2(P N UV0 UV1) V3(P N UV0 UV1)

                    var totalOffset = geometry.Mesh.Sources.Count == 2 ? 6 : geometry.Mesh.Sources.Count == 3 ? 9 : geometry.Mesh.Sources.Count == 4 ? 12 : 0;
                    var componentOffset = geometry.Mesh.Sources.Count;

                    for (int i = 0; i < trianglesCount; i++)
                    {
                        newGeometry.VertexIndices[i * 3] = triangleList.Values[i * totalOffset + vertexOffset];
                        newGeometry.VertexIndices[i * 3 + 1] = triangleList.Values[i * totalOffset + vertexOffset + componentOffset];
                        newGeometry.VertexIndices[i * 3 + 2] = triangleList.Values[i * totalOffset + vertexOffset + (componentOffset * 2)];
                        newGeometry.NormalIndices[i * 3] = triangleList.Values[i * totalOffset + normalOffset];
                        newGeometry.NormalIndices[i * 3 + 1] = triangleList.Values[i * totalOffset + normalOffset + componentOffset];
                        newGeometry.NormalIndices[i * 3 + 2] = triangleList.Values[i * totalOffset + normalOffset + (componentOffset * 2)];

                        if (newGeometry.UV0Indices != null)
                        {
                            newGeometry.UV0Indices[i * 3] = triangleList.Values[i * totalOffset + uvs0Offset];
                            newGeometry.UV0Indices[i * 3 + 1] = triangleList.Values[i * totalOffset + uvs0Offset + componentOffset];
                            newGeometry.UV0Indices[i * 3 + 2] = triangleList.Values[i * totalOffset + uvs0Offset + (componentOffset * 2)];
                        }
                        if (newGeometry.UV1Indices != null)
                        {
                            newGeometry.UV1Indices[i * 3] = triangleList.Values[i * totalOffset + uvs1Offset];
                            newGeometry.UV1Indices[i * 3 + 1] = triangleList.Values[i * totalOffset + uvs1Offset + componentOffset];
                            newGeometry.UV1Indices[i * 3 + 2] = triangleList.Values[i * totalOffset + uvs1Offset + (componentOffset * 2)];
                        }
                    }

                    result.Add(newGeometry);
                }
            }

            return result;
        }

        public static List<Animation.Bone> GetColladaGeometryBones(CoreCollada.Controller controller, List<Animation.Bone> bones)
        {
            var result = ((Animation.Bone[])bones.ToArray().Clone()).ToList();

            var namesSource = controller.Skin.Sources.Where(a => a.TechniqueCommon.Accessor[0].Params[0].Type == "name").FirstOrDefault();

            var invBindMatrixSource = controller
                ?.Skin.Joints.Where(a => a.Semantic == "INV_BIND_MATRIX").FirstOrDefault()
                ?.Source;

            var invBindMatrix = (invBindMatrixSource != null) ?
                 controller.Skin.Sources.Where(a => a.Id == invBindMatrixSource.Substring(1)).FirstOrDefault().FloatValues
                 : new float[] { }.IdentityMatrix();

            foreach (var bone in result)
            {
                var boneId = namesSource.NameValues.TakeWhile(a => a != bone.Id).Count();

                if (boneId < namesSource.NameValues.Length)
                {
                    bone.InverseBindMatrix = invBindMatrix.Skip(boneId * 16).Take(16).ToArray().TransposeMatrix();
                }
            }

            return result;
        }

        public static List<Dictionary<string, float>> GetColladaVertexWeights(CoreCollada.Controller controller)
        {
            var vertexWeights = new List<Dictionary<string, float>>();
            var namesSource = controller.Skin.Sources.Where(a => a.TechniqueCommon.Accessor[0].Params[0].Type == "name").FirstOrDefault();
            var weightsSource = controller.Skin.Sources.Where(a => a.Id.ToLower().Contains("weights")).FirstOrDefault();

            var vindex = 0;

            for (var vcount = 0; vcount < controller.Skin.VertexWeights.VCountValue.Length; vcount++)
            {
                var vertexWeight = new Dictionary<string, float>();

                for (var v = 0; v < controller.Skin.VertexWeights.VCountValue[vcount]; v++)
                {
                    var boneId = namesSource.NameValues[controller.Skin.VertexWeights.VValue[vindex]];
                    var weight = weightsSource.FloatValues[controller.Skin.VertexWeights.VValue[vindex + 1]];

                    vertexWeight.Add(boneId, weight);

                    vindex += 2;
                }

                vertexWeights.Add(vertexWeight);
            }

            return vertexWeights;
        }
        
        public static List<Material> GetMaterials(List<CoreCollada.InstanceMaterial> instanceMaterials, CoreCollada.Collada file, string path = "")
        {
            var result = new List<Material>();

            foreach (var instanceMaterial in instanceMaterials)
            {
                var materialId = instanceMaterial.Target.Substring(1);
                var material = file.LibraryMaterials.Where(a => a.Id == materialId).FirstOrDefault();
                var effect = file.LibraryEffects.Where(a => a.Id == material.InstanceEffect.Url.Substring(1)).FirstOrDefault();

                if (effect.Profile.Technique.Phong.Diffuse.Texture != null)
                {
                    var imageId = effect.Profile.Technique.Phong.Diffuse.Texture.Image;
                    var image = file.LibraryImages.Where(a => a.Id == imageId).FirstOrDefault();
                    var imageName = Path.GetFileName(image.InitFrom);

                    var filePath = Path.Combine(Path.GetDirectoryName(path), imageName);

                    result.Add(new Material()
                    {
                        Name = material.Name,
                        Texture = TextureManager.LoadTexture(filePath)
                    });
                }
                else
                {
                    result.Add(new Material()
                    {
                        Name = material.Name,
                        Texture = 0
                    });
                }
            }

            return result;
        }
    }
}
