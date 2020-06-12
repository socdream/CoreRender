using CoreMath;
using CoreRender.Shaders;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CoreRender.Geometry
{
    public class SvgMesh : Mesh
    {
        public SvgMesh(GeometryData data) : base (data)
        {
            Shader = ShaderManager.LoadShader<PositionColorShader>();
        }

        public override void Draw(Camera camera, float[] parentTransform = null)
        {
            base.Draw(camera, parentTransform);
        }
    }
}
