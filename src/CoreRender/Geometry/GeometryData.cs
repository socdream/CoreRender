using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreRender.Geometry
{
    public class GeometryData
    {
        public byte[] Data { get; set; }
        public int[] Indices { get; set; }
        public int VertexCount { get; set; }
        public MeshHelper.VertexAttrib[] Attribs { get; set; }
    }
}
