using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreRender.Geometry
{
    public class LineMesh : Mesh
    {
        public LineMesh(List<float[]> points) : base(MeshHelper.FromVertices(points.Select(a => new MeshHelper.PositionVertex()
        {
            PosX = a[0],
            PosY = a[1],
            PosZ = a[2]
        }).ToList(), Enumerable.Range(0, points.Count).ToList()))
        {
            PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType.LineStrip;
            Shader = Shaders.ShaderManager.LoadShader<Shaders.DefaultShader>();
        }
    }
}