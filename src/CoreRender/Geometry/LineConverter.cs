using CoreMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreRender.Geometry
{
    public static class LineConverter
    {
        private static float[] ExtrudePoint(float[] a, float[] b, float[] c, float width)
        {
            var seg1 = b.Substract(a).NormalizeVector();
            var seg2 = b.Substract(c).NormalizeVector();

            var normal1 = new float[] { -seg1[1], seg1[0], seg1[2] };
            var normal2 = new float[] { seg2[1], -seg2[0], seg2[2] };

            var bisectorDistance = width / Math.Sqrt(1 + normal1.VectorDotProduct(normal2));

            var dir = normal1.Add(normal2).NormalizeVector().VectorScale((float)bisectorDistance);

            return b.Add(dir);
        }


        /// <summary>
        /// Extrudes a line a given width and creates a polygon with the original line and the extruded one
        /// </summary>
        /// <param name="points">Array containing the line as X1, Y1, Z1, X2, Y2, Z3, etc...</param>
        /// <param name="width"></param>
        /// <returns>A tuple containing the extruded vertices as X1, Y1, Z1, X2, Y2, Z3, etc... and the indices that create the polygon</returns>
        public static Tuple<List<float[]>, List<int>> GetPolygonalLine(List<float[]> points, float width)
        {
            var extruded = new List<float[]>(points.Count);

            for (int i = 0; i < points.Count; i++)
            {
                var a = i == 0 ? points.LastOrDefault() : points[i - 1];
                var b = points[i];
                var c = i == (points.Count - 1) ? points[0] : points[i + 1];

                extruded.Add(ExtrudePoint(a,b,c, width));
            }

            var count = points.Count;

            var indices = points.SelectMany((a, i) => new int[]
            {
                count + i,
                i + 1 == count ? count : count + i + 1,
                i,
                i,
                i + 1 == count ? count : count + i + 1,
                i + 1 == count ? 0 : 1 + i
            });

            points.AddRange(extruded);

            return new Tuple<List<float[]>, List<int>>(points, indices.ToList());
        }
    }
}
