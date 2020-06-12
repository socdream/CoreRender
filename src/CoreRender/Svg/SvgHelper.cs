using CoreRender.Delaunay;
using CoreRender.Geometry;
using CoreSvg;
using CoreSvg.Extensions;
using CoreSvg.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CoreMath;
using CoreRender.ConstrainedDelaunay;

namespace CoreRender.Svg
{
    public class SvgHelper
    {
        private static List<SvgMeshData> GetSvgMeshDatas(SvgFile svg, bool normalize = true)
        {
            var meshesData = new List<SvgMeshData>();

            var svgStyle = default(Style);

            foreach (var item in svg.GraphicElements)
            {
                if (item is CoreSvg.Models.Group)
                    meshesData.AddRange(GetMeshesData((CoreSvg.Models.Group)item, null, svg.Definitions));
                else
                    meshesData.Add(GetMesh(item, null, svg.Definitions));
            }

            meshesData.ForEach(data =>
            {
                if(!string.IsNullOrWhiteSpace(svg.ViewBox))
                {
                    var viewBox = svg.ViewBox.Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(a => float.Parse(a.FixDecimalPoint())).ToList();
                    data.MinPosition = new float[] { viewBox[0], viewBox[1] };
                    data.Size = new float[] { viewBox[2], viewBox[3] };
                }

                ApplyTransforms(data);
            });

            return meshesData;
        }

        public static List<SvgInstancedMesh> GetInstancedMeshes(SvgFile svg)
        {
            var meshesData = GetSvgMeshDatas(svg);

            //var boundingBox = ResetOrigin(meshesData);

            return meshesData.Select(data => new SvgInstancedMesh(GetGeometryData(data, true))
            {
                Size = data.Size,
                MinPosition = data.MinPosition
            }).ToList();
        }

        public static List<SvgMesh> GetMeshes(SvgFile svg)
        {
            var meshesData = GetSvgMeshDatas(svg);

            return meshesData.Select(data => new SvgMesh(GetGeometryData(data, false))).ToList();
        }

        public static SvgMeshData GetBorderMesh(List<float[]> points, float width, Color color)
        {
            var finalPoints = points.Select(a => new float[] { a[0], a[1], 0 }).ToList();

            var polygonalLine = LineConverter.GetPolygonalLine(finalPoints, width);

            finalPoints.AddRange(polygonalLine.Item1);

            return new SvgMeshData()
            {
                Vertices = finalPoints.Select(a => new MeshHelper.PositionColorVertex()
                {
                    PosX = a[0],
                    PosY = a[1],
                    PosZ = a[2],
                    R = color.R / 255f,
                    G = color.G / 255f,
                    B = color.B / 255f,
                    A = color.A / 255f
                }).ToList(),
                Indices = polygonalLine.Item2
            };
        }

        public static List<SvgMeshData> GetMeshesData(CoreSvg.Models.Group group, Style parentStyle, Definitions definitions)
        {
            var result = new List<SvgMeshData>();
            var style = StyleHelper.GetStyle(group, parentStyle, definitions);

            foreach (var item in group.GraphicElements)
            {
                if (item is CoreSvg.Models.Group)
                    result.AddRange(GetMeshesData((CoreSvg.Models.Group)item, style, definitions));
                else
                    result.Add(GetMesh(item, style, definitions));
            }

            return result;
        }

        public static SvgMeshData GetMesh(GraphicElement item, Style parentStyle, Definitions definitions)
        {
            var style = StyleHelper.GetStyle(item, parentStyle, definitions);
            var mesh = new SvgMeshData()
            {
                Scaling = new float[] { style.Transform.Scale[0], -style.Transform.Scale[1], 1f },
                Translation = new float[] { style.Transform.Translate[0], -style.Transform.Translate[1], 1f }
            };

            if (item is Circle)
            {
                var points = GetPoints((Circle)item);

                var temp = GetMeshData((Circle)item, points, style.Fill);

                mesh.Indices = temp.Indices;
                mesh.Vertices = temp.Vertices;

                var border = GetBorderMesh(points, (float)style.StrokeWidth, style.Stroke);

                mesh.Indices.AddRange(border.Indices.Select(a => a + mesh.Vertices.Count));
                mesh.Vertices.AddRange(border.Vertices);
            }
            else if (item is CoreSvg.Models.Rectangle)
            {
                var points = GetPoints((CoreSvg.Models.Rectangle)item);

                var temp = GetMeshData((CoreSvg.Models.Rectangle)item, points, style.Fill);

                mesh.Indices = temp.Indices;
                mesh.Vertices = temp.Vertices;

                var border = GetBorderMesh(points, (float)style.StrokeWidth, style.Stroke);

                mesh.Indices.AddRange(border.Indices.Select(a => a + mesh.Vertices.Count));
                mesh.Vertices.AddRange(border.Vertices);
            }
            else if (item is CoreSvg.Models.Path)
            {
                var points = GetPoints((Path)item);

                var temp = GetMeshData((Path)item, points, style);

                mesh.Indices = temp.Indices;
                mesh.Vertices = temp.Vertices;

                var border = GetBorderMesh(points, (float)style.StrokeWidth, style.Stroke);

                mesh.Indices.AddRange(border.Indices.Select(a => a + mesh.Vertices.Count));
                mesh.Vertices.AddRange(border.Vertices);
            }
            else if (item is Ellipse)
            {
                var points = GetPoints((Ellipse)item);

                var temp = GetMeshData((Ellipse)item, points, style.Fill);

                mesh.Indices = temp.Indices;
                mesh.Vertices = temp.Vertices;

                var border = GetBorderMesh(points, (float)style.StrokeWidth, style.Stroke);

                mesh.Indices.AddRange(border.Indices.Select(a => a + mesh.Vertices.Count));
                mesh.Vertices.AddRange(border.Vertices);
            }

            return mesh;
        }

        /// <summary>
        /// Applies translation and scaling transforms to the vertices so you don't need to keep track of them later on
        /// </summary>
        /// <param name="data">Mesh data</param>
        /// <param name="normalize">Gets the document width as the unit and normalizes the scaling given this new unit, if the document has 512px width, 512px becomes 1 unit</param>
        /// <returns></returns>
        public static SvgMeshData ApplyTransforms(SvgMeshData data, bool normalize = true)
        {
            var translate = new float[] 
            {
                data.Translation?.X() ?? 0f,
                data.Translation?.Y() ?? 0,
                0f
            };
            var scale = new float[] 
            {
                data.Scaling?.X() ?? 1f,
                data.Scaling?.Y() ?? 1f,
                1f
            };

            if (normalize)
                if (!(data.Size is null))
                {
                    scale = scale.VectorScale(1 / data.Size[0]);
                    translate = translate.VectorScale(1 / data.Size[0]);
                    translate[1] += 1;
                }

            var transform = translate.MatrixCompose(translate, new float[] { 0, 0, 0, 1f }, scale);

            data.Vertices = data.Vertices.Select(point =>
            {
                var transformed = new float[] { point.PosX, point.PosY, point.PosZ }.VectorTransform(transform);
                point.PosX = transformed.X();
                point.PosY = transformed.Y();
                point.PosZ = transform.Z();

                return point;
            }).ToList();

            return data;
        }

        public static SvgMeshData GetMeshData(Circle circle, List<float[]> points, Color color)
        {
            var data = GetVertices(points).ToList();

            data.Add(new MeshHelper.PositionColorVertex()
            {
                PosX = (float)circle.CenterX,
                PosY = (float)circle.CenterY,
            });

            data.ForEach(v =>
            {
                v.R = color.R / 255f;
                v.G = color.G / 255f;
                v.B = color.B / 255f;
                v.A = color.A / 255f;
            });

            var meshIndices = Enumerable.Range(0, data.Count - 2).SelectMany(i => new int[] { i, i + 1, data.Count - 1 }).ToList();

            meshIndices.AddRange(new int[] { data.Count - 2, 0, data.Count - 1 });

            return new SvgMeshData()
            {
                Vertices = data,
                Indices = meshIndices
            };
        }

        public static SvgMeshData GetMeshData(Ellipse ellipse, List<float[]> points, Color color)
        {
            var data = GetVertices(points).ToList();

            data.Add(new MeshHelper.PositionColorVertex()
            {
                PosX = (float)ellipse.CenterX,
                PosY = (float)ellipse.CenterY,
            });

            data.ForEach(v =>
            {
                v.R = color.R / 255f;
                v.G = color.G / 255f;
                v.B = color.B / 255f;
                v.A = color.A / 255f;
            });

            var meshIndices = Enumerable.Range(0, data.Count - 2).SelectMany(i => new int[] { i, i + 1, data.Count - 1 }).ToList();

            meshIndices.AddRange(new int[] { data.Count - 2, 0, data.Count - 1 });

            return new SvgMeshData()
            {
                Vertices = data,
                Indices = meshIndices
            };
        }

        public static GeometryData GetGeometryData(SvgMeshData data, bool instanced)
        {
            var result = new GeometryData()
            {
                VertexCount = data.Vertices.Count,
                Indices = data.Indices.ToArray(),
                Attribs = instanced ? MeshHelper.PositionColorInstancedVertex.VertexAttribs : MeshHelper.PositionColorVertex.VertexAttribs
            };

            using (var stream = new System.IO.MemoryStream())
            using (var writer = new System.IO.BinaryWriter(stream))
            {
                for (int i = 0; i < data.Vertices.Count; i++)
                {
                    writer.Write(data.Vertices[i].PosX);
                    writer.Write(data.Vertices[i].PosY);
                    writer.Write(data.Vertices[i].PosZ);
                    writer.Write(data.Vertices[i].R);
                    writer.Write(data.Vertices[i].G);
                    writer.Write(data.Vertices[i].B);
                    writer.Write(data.Vertices[i].A);
                }

                result.Data = stream.ToArray();
            }

            return result;
        }

        public static SvgMeshData GetMeshData(CoreSvg.Models.Rectangle rectangle, List<float[]> points, Color color)
        {
            var data = GetVertices(points).ToList();
            var meshIndices = new List<int> { 0, 1, 2, 0, 2, 3 };

            data.ForEach(v =>
            {
                v.R = color.R / 255f;
                v.G = color.G / 255f;
                v.B = color.B / 255f;
                v.A = color.A / 255f;
            });

            return new SvgMeshData()
            {
                Vertices = data,
                Indices = meshIndices
            };
        }

        public static DelaunayTriangulation GetConstrainedDelaunayTriangulation(List<float[]> vertices)
        {
            var shape = new Shape(vertices);
            var triangles = shape.Triangulate();

            return new DelaunayTriangulation()
            {
                Triangles = Enumerable.Range(0, triangles.Count * 3).ToList(),
                Vertices = triangles.SelectMany(triangle => triangle.Points.Select(point => new float[] { point.X, point.Y })).ToList()
            };
        }

        public static DelaunayTriangulation GetDelaunayIndexes(List<float[]> points)
        {
            var delaunay = new DelaunayCalculator();

            return delaunay.CalculateTriangulation(points);
        }

        public static SvgMeshData GetMeshData(CoreSvg.Models.Path path, List<float[]> points, Style style, bool constrained = true)
        {
            var triangulation = points.Count > 4 ? (constrained ? GetConstrainedDelaunayTriangulation(points) : GetDelaunayIndexes(points)) : null;
            var data = triangulation is null ? GetVertices(points).ToList() : GetVertices(triangulation.Vertices.SelectMany(a => a).ToArray()).ToList();

            var meshIndices = triangulation is null ? (data.Count == 3) ? new List<int> { 0, 1, 2 } : (data.Count == 4) ? new List<int> { 0, 1, 2, 0, 2, 3 } : new List<int> { } : triangulation.Triangles;

            if (style.LinearGradient != null)
            {
                var gradient1 = new float[] { (float)style.LinearGradient.X1, (float)style.LinearGradient.Y1, 0f };
                var gradient2 = new float[] { (float)style.LinearGradient.X2, (float)style.LinearGradient.Y2, 0f };
                var gradientTransform = StyleHelper.GetTransform(style.LinearGradient.GradientTransform);
                var translate = gradientTransform.Translate.Add(style.Transform.Translate);
                var scale = new float[] { style.Transform.Scale.X(), style.Transform.Scale.Y(), 0f };
                gradient1 = gradient1.Add(new float[] { translate.X(), translate.Y(), 0f }).MultiplyComponents(scale);
                gradient2 = gradient2.Add(new float[] { translate.X(), translate.Y(), 0f }).MultiplyComponents(scale);

                data.ForEach(v =>
                {
                    var current = new float[] { v.PosX, v.PosY, 0f };

                    gradient1.SegmentClosestPoint(gradient2, current, out var offset);

                    var stop = style.LinearGradient.Stops.FirstOrDefault(a => a.Offset == offset);
                    var color = Color.Black;

                    if (stop is null)
                    {
                        var stops = style.LinearGradient.Stops.OrderBy(a => Math.Abs(a.Offset - offset)).Take(2).ToList();

                        var stopStyle = StyleHelper.GetStyle(stops[0].Style, null, null);
                        var stopColor1 = Color.FromArgb((byte)(stopStyle.StopOpacity * 255), stopStyle.StopColor);

                        stopStyle = StyleHelper.GetStyle(stops[1].Style, null, null);
                        var stopColor2 = Color.FromArgb((byte)(stopStyle.StopOpacity * 255), stopStyle.StopColor);

                        var stopOffsetDif = stops[1].Offset - stops[0].Offset;
                        var stop1Offset = (offset - stops[0].Offset) / stopOffsetDif;
                        var stop2Offset = 1f - stop1Offset;

                        color = Color.FromArgb((byte)(stopColor1.A * stop1Offset) + (byte)(stopColor2.A * stop2Offset),
                                                (byte)(stopColor1.R * stop1Offset) + (byte)(stopColor2.R * stop2Offset),
                                                (byte)(stopColor1.G * stop1Offset) + (byte)(stopColor2.G * stop2Offset),
                                                (byte)(stopColor1.B * stop1Offset) + (byte)(stopColor2.B * stop2Offset));
                    }

                    v.R = color.R / 255f;
                    v.G = color.G / 255f;
                    v.B = color.B / 255f;
                    v.A = color.A / 255f;
                });
            }
            else
                data.ForEach(v =>
                {
                    v.R = style.Fill.R / 255f;
                    v.G = style.Fill.G / 255f;
                    v.B = style.Fill.B / 255f;
                    v.A = style.Fill.A / 255f;
                });

            return new SvgMeshData()
            {
                Vertices = data,
                Indices = meshIndices
            };
        }

        public static IEnumerable<MeshHelper.PositionColorVertex> GetVertices(float[] positions)
        {
            for (int i = 0; i < positions.Length / 2; i++)
            {
                yield return new MeshHelper.PositionColorVertex()
                {
                    PosX = positions[i * 2],
                    PosY = positions[i * 2 + 1]
                };
            }
        }

        public static IEnumerable<MeshHelper.PositionColorVertex> GetVertices(List<float[]> positions)
        {
            return positions.Select(a => new MeshHelper.PositionColorVertex()
            {
                PosX = a[0],
                PosY = a[1]
            });
        }

        public static List<float[]> GetPoints(CoreSvg.Models.Rectangle rectangle)
        {
            return new List<float[]>
            {
                new float[] { (float)rectangle.X, (float)rectangle.Y },
                new float[] { (float)(rectangle.X + rectangle.Width), (float)rectangle.Y },
                new float[] { (float)(rectangle.X + rectangle.Width), (float)(rectangle.Y + rectangle.Height) },
                new float[] { (float)rectangle.X, (float)(rectangle.Y + rectangle.Height) }
            };
        }

        public static List<float[]> GetPoints(Circle circle)
        {
            var segments = 32;
            var incrRad = Math.PI * 2 / segments;

            return Enumerable.Range(0, segments).Select(current => new float[]
            {
                (float)(Math.Sin(incrRad * current) * circle.Radius + circle.CenterX),
                (float)(Math.Cos(incrRad * current) * circle.Radius + circle.CenterY)
            }).ToList();
        }

        public static List<float[]> GetPoints(Ellipse circle)
        {
            var segments = 32;
            var incrRad = Math.PI * 2 / segments;

            return Enumerable.Range(0, segments).Select(current => new float[]
            {
                (float)(Math.Sin(incrRad * current) * circle.RadiusX + circle.CenterX),
                (float)(Math.Cos(incrRad * current) * circle.RadiusY + circle.CenterY)
            }).ToList();
        }

        public static List<float[]> GetPoints(CoreSvg.Models.Path path)
        {
            var commands = GetPathCommands(path);
            var result = new List<float[]>();

            foreach (var command in commands)
            {
                var points = GetPoints(command, result);

                result.AddRange(points);
            }

            return result;
        }

        public static List<float[]> GetPoints(SvgPathCommand command, List<float[]> points)
        {
            var lastPoint = points?.Count > 0 ? points.LastOrDefault() : new float[2];

            if (command.Command == "M")
                return Enumerable.Range(0, command.Values.Length / 2).Select(i => new float[] { command.Values[i * 2], command.Values[i * 2 + 1] }).ToList();
            else if (command.Command == "m")
            {
                var current = new float[] { lastPoint[0], lastPoint[1] };
                var result = new List<float[]>();

                for (int i = 0; i < command.Values.Length / 2; i++)
                {
                    current[0] += command.Values[i * 2];
                    current[1] += command.Values[i * 2 + 1];

                    result.Add(new float[] { current[0], current[1] });
                }

                return result;
            }
            else if (command.Command == "L")
                return Enumerable.Range(0, command.Values.Length / 2).Select(i => new float[] { command.Values[i * 2], command.Values[i * 2 + 1] }).ToList();
            else if (command.Command == "l")
            {
                var current = new float[] { lastPoint[0], lastPoint[1] };
                var result = new List<float[]>();

                for (int i = 0; i < command.Values.Length / 2; i++)
                {
                    current[0] += command.Values[i * 2];
                    current[1] += command.Values[i * 2 + 1];

                    result.Add(new float[] { current[0], current[1] });
                }

                return result;
            }
            else if (command.Command == "Z" || command.Command == "z")
                return new List<float[]>();
            else if (command.Command == "C")
            {
                var result = new List<float[]>();

                for (int i = 0; i < command.Values.Length; i += 6)
                {
                    var start = lastPoint ?? new float[] { 0, 0 };
                    var control1 = command.Values.Skip(i).Take(2).ToArray();
                    var control2 = command.Values.Skip(i + 2).Take(2).ToArray();
                    var end = command.Values.Skip(i + 4).Take(2).ToArray();

                    result.AddRange(GetBezierCurve(start, control1, control2, end, 16).Skip(1));
                    result.Add(end);
                    lastPoint = end;
                }

                return result;
            }
            else if (command.Command == "c")
            {
                var result = new List<float[]>();

                for (int i = 0; i < command.Values.Length; i += 6)
                {
                    var start = lastPoint ?? new float[] { 0, 0 };
                    var control1 = command.Values.Skip(i).Take(2).ToArray().Add(start);
                    var control2 = command.Values.Skip(i + 2).Take(2).ToArray().Add(start);
                    var end = command.Values.Skip(i + 4).Take(2).ToArray().Add(start);

                    result.AddRange(GetBezierCurve(start, control1, control2, end, 16).Skip(1));
                    result.Add(end);
                    lastPoint = end;
                }

                return result;
            }
            else if (command.Command == "V")
            {
                var current = lastPoint ?? new float[] { 0, 0 };

                return new List<float[]> { new float[] { current[0], command.Values[0] } };
            }
            else if (command.Command == "v")
            {
                var current = lastPoint ?? new float[] { 0, 0 };

                return new List<float[]> { new float[] { current[0], current[1] + command.Values[0] } };
            }
            else if (command.Command == "H")
            {
                var current = lastPoint ?? new float[] { 0, 0 };

                return new List<float[]> { new float[] { command.Values[0], current[1] } };
            }
            else if (command.Command == "h")
            {
                var current = lastPoint ?? new float[] { 0, 0 };

                return new List<float[]> { new float[] { current[0] + command.Values[0], current[1] } };
            }

            return new List<float[]>();
        }

        public static List<float[]> GetBezierCurve(float[] start, float[] target1, float[] target2, float[] end, int segments)
        {
            return Enumerable
                .Range(0, segments)
                .Select(current => start.GetBezierPoint(target1, target2, end, (float)current / segments))
                .ToList();
        }

        public static IEnumerable<SvgPathCommand> GetPathCommands(Path path)
        {
            var separators = @"(?=[MZLHVCSQTAmzlhvcsqta])"; // these letters are valid SVG
                                                            // commands. Whenever we find one, a new command is 
                                                            // starting. Let's split the string there.
            var tokens = Regex.Split(path.Drawing, separators).Where(t => !string.IsNullOrEmpty(t));

            // our "interpreter". Runs the list of commands and does something for each of them.
            foreach (string token in tokens)
            {
                var remainingargs = token.Substring(1);

                //var argSeparators = @"[\s,]|(?=-)";
                var validValuesReg = @"-?\d+\.?\d*(?:e-)?\d?";

                var matches = Regex
                    .Matches(token, validValuesReg);

                var values = new List<float>(matches.Count);

                for (int i = 0; i < matches.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace(matches[i].Value))
                        values.Add(float.Parse(matches[i].Value.FixDecimalPoint()));
                }

                yield return new SvgPathCommand()
                {
                    Command = token.Substring(0, 1),
                    Values = values.ToArray()
                };
            }
        }
    }
}
