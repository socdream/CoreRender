using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreRender.Geometry
{
    public class XYPlaneMesh : Mesh
    {
        public XYPlaneMesh(float[] bindShapeMatrix = null)
            : base(MeshHelper.CreateXYPlane(bindShapeMatrix))
        { }
    }
}
