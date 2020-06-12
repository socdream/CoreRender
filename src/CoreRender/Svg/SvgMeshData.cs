using CoreRender.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CoreRender.Svg
{
    public class SvgMeshData
    {
        public List<int> Indices { get; set; }
        public List<MeshHelper.PositionColorVertex> Vertices { get; set; }
        public float[] Scaling { get; set; }
        public float[] Translation { get; set; }
        /// <summary>
        /// ViewBox minimum position
        /// </summary>
        public float[] MinPosition { get; set; } = new float[] { 0, 0 };
        /// <summary>
        /// ViewBox size
        /// </summary>
        public float[] Size { get; set; } = new float[] { 1, 1 };
    }
}
