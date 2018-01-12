using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CoreRender.Geometry
{
    public class ModelManager
    {
        public static string ModelsPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Resources\Models\");
        private static Dictionary<string, List<Mesh>> _models = new Dictionary<string, List<Mesh>>();

        public static List<Mesh> LoadMeshes(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Mesh path can't be empty.");

            if (_models.ContainsKey(path))
                return _models[path];

            var extension = System.IO.Path.GetExtension(path);

            if (extension == ".dae")
            {
                using (var stream = new FileStream(Path.Combine(ModelsPath, path), FileMode.Open))
                using (var reader = XmlReader.Create(stream))
                {
                    var serializer = new XmlSerializer(typeof(CoreCollada.Collada));

                    var file = (CoreCollada.Collada)serializer.Deserialize(reader);

                    _models.Add(path, Collada.ColladaHelper.GetMeshes(file, null, null, TextureManager.TexturesPath));
                }
            }

            return _models[path];
        }
    }
}
