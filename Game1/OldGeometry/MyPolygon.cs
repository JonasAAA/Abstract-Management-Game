using ClipperLib;
using LibTessDotNet;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game1.OldGeometry
{
    public class MyPolygon
    {
        public Color color;

        public IReadOnlyCollection<VertexPositionColor> Vertices
            => vertices;
        public IReadOnlyCollection<int> TriangleIndices
            => triangleIndices;

        private List<MyPoint> boundary;
        private List<List<MyPoint>> holes;
        private VertexPositionColor[] vertices;
        private int[] triangleIndices;
        
        ///// <param name="boundary">must have clocwise orientation</param>
        ///// <param name="holes">each hole must have counterclockwise orientation</param>
        public MyPolygon(List<MyPoint> boundary, List<List<MyPoint>> holes)
        {
            //if (!Clockwise(polygon: boundary))
            //    throw new ArgumentException();

            //if (holes.Any(hole => Clockwise(polygon: hole)))
            //    throw new ArgumentException();

            this.boundary = boundary;
            this.holes = holes;

            Triangulate();

            //static bool Clockwise(List<MyPoint> polygon)
            //    => Clipper.Orientation
            //    (
            //        (from myPoint in polygon
            //         select (IntPoint)myPoint)
            //         .ToList()
            //    );
        }

        public static double CommonPerimeter(MyPolygon myPolygon1, MyPolygon myPolygon2)
        {
            throw new NotImplementedException();
        }

        private void Triangulate()
        {
            Tess tessellator = new();
            foreach (var polygon in holes.Append(boundary))
                tessellator.AddContour
                (
                    (from myPoint in polygon
                     select new ContourVertex((Vec3)myPoint))
                     .ToArray()
                );
            tessellator.Tessellate();

            vertices = (from vertex in tessellator.Vertices
                        select new VertexPositionColor(vertex.Position.ToVector3(), color))
                        .ToArray();
            triangleIndices = tessellator.Elements;
        }
    }
}
