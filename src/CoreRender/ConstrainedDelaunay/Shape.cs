using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreMath;

namespace CoreRender.ConstrainedDelaunay
{
    public class Shape
    {
        /// <summary>
        /// A list of shape outline points.
        /// </summary>
        public readonly List<TriPoint> Points = new List<TriPoint>();

        /// <summary>
        /// Extra points inserted within the shape's area to control or increase triangulation.
        /// </summary>
        public readonly List<TriPoint> SteinerPoints;

        /// <summary>
        /// A list of subtraction shapes fully contained inside this shape.<para/>
        /// Shapes added to this list will be used to create holes during triangulation. Any that are outside or intersect the shape outline are invalid.
        /// </summary>
        public readonly List<Shape> Holes = new List<Shape>();

        public Rectangle Bounds { get; private set; }

        public Shape() { }

        /// <summary>
        /// Create a polygon from a list of at least 3 points with no duplicates.
        /// </summary>
        /// <param name="points">A list of unique points</param>
        public Shape(IList<TriPoint> points)
        {
            Points.AddRange(points);
        }

        /// <summary>
        /// Create a polygon from a list of at least 3 points with no duplicates.
        /// </summary>
        /// <param name="points">A list of unique points</param>
        public Shape(IList<TriPoint> points, float[] offset, float scale)
        {
            for (int i = 0; i < points.Count; i++)
                Points.Add(new TriPoint(new float[] { points[i].X, points[i].Y }.VectorScale(scale).Add(offset)));
        }

        /// <summary>
        /// Create a polygon from a list of at least 3 points with no duplicates.
        /// </summary>
        /// <param name="points">A list of unique points.</param>
        public Shape(IEnumerable<TriPoint> points) : this((points as IList<TriPoint>) ?? points.ToArray()) { }

        /// <summary>
        /// Create a polygon from a list of at least 3 points with no duplicates.
        /// </summary>
        /// <param name="points">A list of unique points.</param>
        public Shape(params TriPoint[] points) : this((IList<TriPoint>)points) { }

        /// <summary>
        /// Create a polygon from a list of at least 3 points with no duplicates.
        /// </summary>
        /// <param name="points">A list of unique points.</param>
        public Shape(params float[][] points) : this((IList<float[]>)points) { }

        /// <summary>
        /// Creates a polygon from a list of at least 3 Vector3 points, with no duplicates.
        /// </summary>
        /// <param name="points">The input points.</param>
        /// <param name="offset">An offset to apply to all of the provided points.</param>
        /// <param name="scale">The scale of the provided points. 0.5f is half size. 2.0f is 2x the normal size.</param>
        public Shape(IList<float[]> points, float[] offset, float scale)
        {
            for (int i = 0; i < points.Count; i++)
                Points.Add(new TriPoint(points[i].VectorScale(scale).Add(offset)));
        }

        /// <summary>
        /// Creates a polygon from a list of at least 3 Vector3 points, with no duplicates.
        /// </summary>
        /// <param name="points">The input points.</param>
        public Shape(IList<float[]> points) : this(points, new float[2], 1.0f) { }

        /// <summary>
        /// Produces a <see cref="RectangleF"/> which contains all of the shape's points.
        /// </summary>
        public Rectangle CalculateBounds()
        {
            Rectangle b = new Rectangle()
            {
                MinX = float.MaxValue,
                MinY = float.MaxValue,
                MaxX = float.MinValue,
                MaxY = float.MinValue,
            };

            foreach (TriPoint p in Points)
            {
                if (p.X < b.MinX)
                    b.MinX = p.X;
                else if (p.X > b.MaxX)
                    b.MaxX = p.Y;

                if (p.Y < b.MinY)
                    b.MinY = p.Y;
                else if (p.Y > b.MaxY)
                    b.MaxY = p.Y;
            }

            return b;
        }

        /// <summary>
        /// Triangulates the shape.
        /// </summary>
        /// <returns>The output list.</returns>
        public List<float[]> Triangulate(float[] offset, float scale = 1.0f)
        {
            var output = new List<float[]>();

            Points.Reverse();

            var context = new SweepContext();
            context.AddPoints(Points);

            // Hole edges
            foreach (Shape h in Holes)
                context.AddHole(h.Points);

            context.InitTriangulation();
            var sweep = new Sweep();
            sweep.Triangulate(context);

            var triangles = context.GetTriangles();

            foreach (Triangle tri in triangles)
            {
                //tri.ReversePointFlow();
                output.Add(((float[])tri.Points[0]).VectorScale(scale).Add(offset));
                output.Add(((float[])tri.Points[2]).VectorScale(scale).Add(offset));
                output.Add(((float[])tri.Points[1]).VectorScale(scale).Add(offset));
            }

            return output;
        }

        /// <summary>
        /// Triangulates the shape.
        /// </summary>
        /// <returns>The output list.</returns>
        public List<Triangle> Triangulate()
        {
            var context = new SweepContext();

            var distinctPoints = Points.Distinct().ToList();
            context.AddPoints(distinctPoints);

            foreach (var hole in Holes)
                context.AddHole(hole.Points);

            context.InitTriangulation();

            var sweep = new Sweep();

            sweep.Triangulate(context);

            return context.GetTriangles();
        }

        public void Scale(float scale)
        {
            for (int i = 0; i < Points.Count; i++)
                Points[i] *= scale;

            foreach (Shape h in Holes)
                h.Scale(scale);
        }

        public void Scale(float[] scale)
        {
            for (int i = 0; i < Points.Count; i++)
                Points[i] *= scale;

            foreach (Shape h in Holes)
                h.Scale(scale);
        }

        public void Offset(float[] offset)
        {
            for (int i = 0; i < Points.Count; i++)
                Points[i] += offset;

            foreach (Shape h in Holes)
                h.Offset(offset);
        }

        public void ScaleAndOffset(float[] offset, float scale)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                Points[i] *= scale;
                Points[i] += offset;
            }

            foreach (Shape h in Holes)
                h.ScaleAndOffset(offset, scale);
        }

        public void ScaleAndOffset(float[] offset, float[] scale)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                Points[i] *= scale;
                Points[i] += offset;
            }

            foreach (Shape h in Holes)
                h.ScaleAndOffset(offset, scale);
        }

        public bool Contains(Shape shape)
        {
            for (int i = 0; i < shape.Points.Count; i++)
            {
                // We only need 1 point to be outside to invalidate a containment.
                if (!Contains((float[])shape.Points[i]))
                    return false;
            }

            return true;
        }

        public bool Contains(float[] point)
        {
            for (int i = 0; i < Holes.Count; i++)
            {
                if (Holes[i].Contains(point))
                    return false;
            }

            // Thanks to: https://codereview.stackexchange.com/a/108903
            int polygonLength = Points.Count;
            int j = 0;
            bool inside = false;
            float pointX = point.X(), pointY = point.Y(); // x, y for tested point.

            // start / end point for the current polygon segment.
            float startX, startY, endX, endY;
            float[] endPoint = (float[])Points[polygonLength - 1];
            endX = endPoint.X();
            endY = endPoint.Y();

            while (j < polygonLength)
            {
                startX = endX; startY = endY;
                endPoint = (float[])Points[j++];
                endX = endPoint.X(); endY = endPoint.Y();
                //
                inside ^= (endY > pointY ^ startY > pointY) /* ? pointY inside [startY;endY] segment ? */
                          && /* if so, test if it is under the segment */
                          ((pointX - endX) < (pointY - endY) * (startX - endX) / (startY - endY));
            }

            return inside;
        }
    }
}
