using CoreMath;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreRender.Delaunay
{
    public class Geom
    {

        /// <summary>
        /// Is point p to the left of the line from l0 to l1?
        /// </summary>
        public static bool ToTheLeft(float[] p, float[] l0, float[] l1)
        {
            return ((l1.X() - l0.X()) * (p.Y() - l0.Y()) - (l1.Y() - l0.Y()) * (p.X() - l0.X())) >= 0;
        }

        /// <summary>
        /// Is point p to the right of the line from l0 to l1?
        /// </summary>
        public static bool ToTheRight(float[] p, float[] l0, float[] l1)
        {
            return !ToTheLeft(p, l0, l1);
        }

        /// <summary>
        /// Is point p inside the triangle formed by c0, c1 and c2 (assuming c1,
        /// c2 and c3 are in CCW order)
        /// </summary>
        public static bool PointInTriangle(float[] p, float[] c0, float[] c1, float[] c2)
        {
            return ToTheLeft(p, c0, c1)
                && ToTheLeft(p, c1, c2)
                && ToTheLeft(p, c2, c0);
        }

        /// <summary>
        /// Is point p inside the circumcircle formed by c0, c1 and c2?
        /// </summary>
        public static bool InsideCircumcircle(float[] p, float[] c0, float[] c1, float[] c2)
        {
            var ax = c0.X() - p.X();
            var ay = c0.Y() - p.Y();
            var bx = c1.X() - p.X();
            var by = c1.Y() - p.Y();
            var cx = c2.X() - p.X();
            var cy = c2.Y() - p.Y();

            return (
                    (ax * ax + ay * ay) * (bx * cy - cx * by) -
                    (bx * bx + by * by) * (ax * cy - cx * ay) +
                    (cx * cx + cy * cy) * (ax * by - bx * ay)
            ) > 0;
        }

        /// <summary>
        /// Rotate vector v left 90 degrees
        /// </summary>
        public static float[] RotateRightAngle(float[] v)
        {
            var x = v.X();
            v[0] = -v.Y();
            v[1] = x;

            return v;
        }

        /// <summary>
        /// General line/line intersection method. Each line is defined by a
        /// two vectors, a point on the line (p0 and p1 for the two lines) and a
        /// direction vector (v0 and v1 for the two lines). The returned value
        /// indicates whether the lines intersect. m0 and m1 are the
        /// coefficients of how much you have to multiply the direction vectors
        /// to get to the intersection. 
        ///
        /// In other words, if the intersection is located at X, then: 
        ///
        ///     X = p0 + m0 * v0
        ///     X = p1 + m1 * v1
        ///
        /// By checking the m0/m1 values, you can check intersections for line
        /// segments and rays.
        /// </summary>
        public static bool LineLineIntersection(float[] p0, float[] v0, float[] p1, float[] v1, out float m0, out float m1)
        {
            var det = (v0.X() * v1.Y() - v0.Y() * v1.X());

            if (Math.Abs(det) < 0.001f)
            {
                m0 = float.NaN;
                m1 = float.NaN;

                return false;
            }
            else
            {
                m0 = ((p0.Y() - p1.Y()) * v1.X() - (p0.X() - p1.X()) * v1.Y()) / det;

                if (Math.Abs(v1.X()) >= 0.001f)
                {
                    m1 = (p0.X() + m0 * v0.X() - p1.X()) / v1.X();
                }
                else
                {
                    m1 = (p0.Y() + m0 * v0.Y() - p1.Y()) / v1.Y();
                }

                return true;
            }
        }

        /// <summary>
        /// Returns the intersections of two lines. p0/p1 are points on the
        /// line, v0/v1 are the direction vectors for the lines. 
        ///
        /// If there are no intersections, returns a NaN vector
        /// <summary>
        public static float[] LineLineIntersection(float[] p0, float[] v0, float[] p1, float[] v1)
        {
            float m0, m1;

            if (LineLineIntersection(p0, v0, p1, v1, out m0, out m1))
            {
                return p0.Add(v0.VectorScale(m0));
            }
            else
            {
                return new float[] { float.NaN, float.NaN };
            }
        }

        /// <summary>
        /// Returns the center of the circumcircle defined by three points (c0,
        /// c1 and c2) on its edge.
        /// </summary>
        public static float[] CircumcircleCenter(float[] c0, float[] c1, float[] c2)
        {
            var mp0 = c0.Add(c1).VectorScale(0.5f);
            var mp1 = c1.Add(c2).VectorScale(0.5f);

            var v0 = RotateRightAngle(c0.Substract(c1));
            var v1 = RotateRightAngle(c1.Substract(c2));

            float m0, m1;

            Geom.LineLineIntersection(mp0, v0, mp1, v1, out m0, out m1);

            return mp0.Add(v0.VectorScale(m0));
        }

        /// <summary>
        /// Returns the triangle centroid for triangle defined by points c0, c1
        /// and c2. 
        /// </summary>
        public static float[] TriangleCentroid(float[] c0, float[] c1, float[] c2)
        {
            var val = c0.Add(c1).Add(c2).VectorScale(1.0f / 3.0f);
            return val;
        }

        /// <summary>
        /// Returns the signed area of a polygon. CCW polygons return a positive
        /// area, CW polygons return a negative area.
        /// </summary>
        public static float Area(IList<float[]> polygon)
        {
            var area = 0.0f;

            var count = polygon.Count;

            for (int i = 0; i < count; i++)
            {
                var j = (i == count - 1) ? 0 : (i + 1);

                var p0 = polygon[i];
                var p1 = polygon[j];

                area += p0.X() * p1.Y() - p1.Y() * p1.X();
            }

            return 0.5f * area;
        }
    }
}
