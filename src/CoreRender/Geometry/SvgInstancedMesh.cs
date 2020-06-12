using CoreRender.Shaders;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CoreRender.Geometry
{
    public class SvgInstancedMesh : InstancedMesh
    {
        /// <summary>
        /// ViewBox minimum position
        /// </summary>
        public float[] MinPosition { get; set; } = new float[] { 0, 0 };
        /// <summary>
        /// ViewBox size
        /// </summary>
        public float[] Size { get; set; } = new float[] { 1, 1 };

        public SvgInstancedMesh(GeometryData data) : base(data)
        {
            Shader = ShaderManager.LoadShader<PositionColorInstancedShader>();
        }

        public override void Draw(Camera camera, float[] parentTransform = null)
        {
            base.Draw(camera, parentTransform);
        }
    }
}
