using Nito.Collections;

namespace Game1.Physics
{
    /// <summary>
    /// Y direction is down
    /// </summary>
    [Serializable]
    public class SoftDisk
    {
        [Serializable]
        private readonly struct SoftDiskVertexFormat
        {
            public static readonly VertexDeclaration vertexDeclaration;

            static SoftDiskVertexFormat()
                => vertexDeclaration = new
                (
                    new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
                    new VertexElement(sizeof(float) * 2, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
                    new VertexElement(sizeof(float) * 4, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 1),
                    new VertexElement(sizeof(float) * 5, VertexElementFormat.Color, VertexElementUsage.Color, 0)
                );

#pragma warning disable IDE0052 // Remove unread private members
            private readonly Vector2 screenPos, relToCenterPos;
            private readonly float radiusSquared;
            private readonly Color color;
#pragma warning restore IDE0052 // Remove unread private members

            private SoftDiskVertexFormat(Vector2 screenPos, Vector2 relToCenterPos, float radiusSquared, Color color)
            {
                this.screenPos = screenPos;
                this.relToCenterPos = relToCenterPos;
                this.radiusSquared = radiusSquared;
                this.color = color;
            }

            public SoftDiskVertexFormat(SoftDisk softDisk, Vector2 relToDiskCenterPos)
                : this
                (
                    screenPos: Vector2.Transform((Vector2)(softDisk.center - worldCenter) + relToDiskCenterPos, worldViewProjection),
                    relToCenterPos: relToDiskCenterPos,
                    radiusSquared: softDisk.radius * softDisk.radius,
                    color: softDisk.color
                )
            { }
        }

        /// <summary>
        /// Open half plane, i.e. if a point is on the separating line, it it considered not in the half-plane
        /// </summary>
        
        [Serializable]
        private readonly struct HalfPlane
        {
            public enum Inter3Status
            {
                MidUseful,
                MidRedundant,
                InterEmpty
            }

            public readonly float angle;

            // the half-plane lives on the left side of the direction vector
            private readonly Vector2 rayStart, rayDir, halfPlaneDir;

            public HalfPlane(Vector2 rayStart, Vector2 rayDir)
            {
                this.rayStart = rayStart;
                rayDir.Normalize();
                this.rayDir = rayDir;
                halfPlaneDir = C.OrthDir(direction: rayDir);
                angle = C.Rotation(vector: rayDir);
            }

            // point can be null to support what is retured from half-plane intersection
            private bool Contains(Vector2? point)
                => point switch
                {
                    null => false,
                    not null => Vector2.Dot(point.Value - rayStart, halfPlaneDir) > 0
                };

            public static Inter3Status GetInter3Status(HalfPlane halfPlaneA, HalfPlane halfPlaneB, HalfPlane halfPlaneC)
            {
                bool AContInterBC = halfPlaneA.Contains(point: Inter2Point(halfPlaneB, halfPlaneC)),
                    BContInterCA = halfPlaneB.Contains(point: Inter2Point(halfPlaneC, halfPlaneA)),
                    CContInterAB = halfPlaneC.Contains(point: Inter2Point(halfPlaneA, halfPlaneB));

                if (AContInterBC || CContInterAB)
                    return Inter3Status.MidUseful;
                if (BContInterCA)
                    return Inter3Status.MidRedundant;
                return Inter3Status.InterEmpty;
            }

            /// <summary>
            /// null is the half-planes are parallel
            /// </summary>
            public static Vector2? Inter2Point(HalfPlane halfPlaneA, HalfPlane halfPlaneB)
            {
                float? rayStartAToInterB = RayStartFirstToInter(halfPlane1: halfPlaneA, halfPlane2: halfPlaneB);
                if (rayStartAToInterB.HasValue)
                    return halfPlaneA.rayStart + halfPlaneA.rayDir * rayStartAToInterB.Value;
                return null;
            }

            /// <summary>
            /// returns null if the half-planes are parallel
            /// </summary>
            private static float? RayStartFirstToInter(HalfPlane halfPlane1, HalfPlane halfPlane2)
            {
                float denominator = Vector2.Dot(halfPlane1.rayDir, halfPlane2.halfPlaneDir);
                if (C.IsTiny(denominator))
                    return null;
                return Vector2.Dot(halfPlane2.rayStart - halfPlane1.rayStart, halfPlane2.halfPlaneDir) / denominator;
            }
        }

        private const int defaultDynamicBufferSize = 10000;
        private static readonly Effect effect;
        private static readonly Matrix projectionMatrix;
        private static readonly List<SoftDiskVertexFormat> vertices;
        private static readonly List<int> indices;
        private static DynamicVertexBuffer dynamicVertexBuffer;
        private static DynamicIndexBuffer dynamicIndexBuffer;
        private static Matrix worldViewProjection;
        private static AccurVector2 worldCenter;

        static SoftDisk()
        {
            effect = C.contentManager.Load<Effect>("DiskEffect");
            projectionMatrix = Matrix.CreateScale(2f / GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, -2f / GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height, 1)
                * Matrix.CreateTranslation(-1, 1, 0);

            vertices = new();
            indices = new();
            
            dynamicVertexBuffer = new(C.graphicsDevice, SoftDiskVertexFormat.vertexDeclaration, defaultDynamicBufferSize, BufferUsage.WriteOnly);
            dynamicIndexBuffer = new(C.graphicsDevice, typeof(int), defaultDynamicBufferSize, BufferUsage.WriteOnly);
        }

        // Cut from WorldManager constructor
        //softDisks = new()
        //{
        //    new
        //    (
        //        center: (AccurVector2) new Vector2(-200, 100),
        //        radius: 200,
        //        defaultColor: Color.Yellow
        //    ),
        //    new
        //    (
        //        center: (AccurVector2) new Vector2(0, 100),
        //        radius: 300,
        //        defaultColor: Color.Red
        //    ),
        //    new
        //    (
        //        center: (AccurVector2) new Vector2(-200, 0),
        //        radius: 150,
        //        defaultColor: Color.Green
        //    )
        //};

        // Cut from WorldManager.Draw()
        //SoftDisk.BeginDraw(worldCenter: (AccurVector2) Vector2.Zero, worldToScreenTransform: worldCamera.GetToScreenTransform());
        //softDisks[0].center = (AccurVector2) MouseWorldPos;
        //    foreach (var softDisk in softDisks)
        //        softDisk.InternalUpdate();
        //    foreach (var softDisk in softDisks)
        //    {
        //        softDisk.CalcNextPos(softDisks: softDisks);
        //        softDisk.Draw();
        //    }
        //SoftDisk.EndDraw();

        public static void BeginDraw(AccurVector2 worldCenter, Matrix worldToScreenTransform)
            {
                SoftDisk.worldCenter = worldCenter;
                worldViewProjection = worldToScreenTransform * projectionMatrix;

                vertices.Clear();
                indices.Clear();
            }

        private static void AddPolygon(SoftDiskVertexFormat[] convexPolygon)
        {
            if (convexPolygon.Length < 3)
                return;

            int startInd = vertices.Count;

            for (int ind = 0; ind < convexPolygon.Length; ind++)
                vertices.Add(convexPolygon[ind]);

            for (int ind = startInd + 1; ind < startInd + convexPolygon.Length - 1; ind++)
            {
                indices.Add(startInd);
                indices.Add(ind);
                indices.Add(ind + 1);
            }
        }

        public static void EndDraw()
        {
            if (vertices.Count is 0)
            {
                indices.Clear();
                return;
            }

            if (vertices.Count > dynamicVertexBuffer.VertexCount)
                dynamicVertexBuffer = new(C.graphicsDevice, SoftDiskVertexFormat.vertexDeclaration, 2 * vertices.Count, BufferUsage.WriteOnly);
            dynamicVertexBuffer.SetData(vertices.ToArray(), 0, vertices.Count);

            if (indices.Count > dynamicIndexBuffer.IndexCount)
                dynamicIndexBuffer = new(C.graphicsDevice, typeof(int), 2 * indices.Count, BufferUsage.WriteOnly);
            dynamicIndexBuffer.SetData(indices.ToArray(), 0, indices.Count);

            // to correctly draw clockwise and counterclocwise triangles
            C.graphicsDevice.RasterizerState = new()
            {
                MultiSampleAntiAlias = true,
                CullMode = CullMode.None
            };

            effect.CurrentTechnique = effect.Techniques["DiskDrawing"];

            C.graphicsDevice.SetVertexBuffer(dynamicVertexBuffer);
            C.graphicsDevice.Indices = dynamicIndexBuffer;

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                C.graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indices.Count / 3);
            }
        }

        public AccurVector2 center;
        private float radius, radiusSpeed;
        private readonly double amount;
        private readonly Color defaultColor;
        // clockwise
        private Vector2[] boundary;
        private double area;
        private Color color;

        public SoftDisk(AccurVector2 center, float radius, Color defaultColor)
        {
            this.center = center;
            this.radius = radius;
            this.defaultColor = defaultColor;
            amount = MathHelper.TwoPi * radius * radius;
            boundary = Array.Empty<Vector2>();
            radiusSpeed = 1;
            area = amount;
            color = defaultColor;
        }

        // frame x:
        //      InternalUpdate()
        //      CalcNextPos()
        public void Update()
        {
            radius *= radiusSpeed;
            // move to the pre-calculated position
            //throw new NotImplementedException();
        }

        public void CalcNextPos(IReadOnlyCollection<SoftDisk> softDisks)
        {
            //List<HalfPlane> halfPlanes = new();
            //// add half planes forming a triangle around the disk
            //for (int ind = 0; ind < 3; ind++)
            //{
            //    var direction = C.Direction(rotation: ind * MathHelper.TwoPi / 3);
            //    halfPlanes.Add
            //    (
            //        new
            //        (
            //            rayStart: radius * direction,
            //            rayDir: C.OrthDir(direction: direction)
            //        )
            //    );
            //}

            // add half planes forming a square around the disk
            List<HalfPlane> halfPlanes = new()
            {
                new
                (
                    rayStart: radius * Vector2.UnitX,
                    rayDir: -Vector2.UnitY
                ),
                new
                (
                    rayStart: -radius * Vector2.UnitX,
                    rayDir: Vector2.UnitY
                ),
                new
                (
                    rayStart: radius * Vector2.UnitY,
                    rayDir: Vector2.UnitX
                ),
                new
                (
                    rayStart: -radius * Vector2.UnitY,
                    rayDir: -Vector2.UnitX
                ),
            };

            foreach (var otherSoftDisk in softDisks)
            {
                var separation = Separation(otherSoftDisk: otherSoftDisk);
                if (separation.HasValue)
                    halfPlanes.Add(separation.Value);
            }

            HalfPlane[] convexHull = ConvexHull(halfPlanes: halfPlanes);
            boundary = new Vector2[convexHull.Length];
            for (int halfPlaneInd = 0; halfPlaneInd < convexHull.Length; halfPlaneInd++)
                boundary[halfPlaneInd] = HalfPlane.Inter2Point
                (
                    halfPlaneA: convexHull[halfPlaneInd],
                    halfPlaneB: convexHull[(halfPlaneInd + 1) % convexHull.Length]
                ).Value;

            area = Area();
            var density = amount / area;
            //color = Color.Lerp(Color.Black, defaultColor, (float)relArea);
            radiusSpeed *= Math.Min(10, (float)Math.Pow(100 * density / radius, .001));
            const float radiusDrag = 1.0001f;
            // this currently works as surface friction, probably better if it worked like air drag
            radiusSpeed = radiusSpeed switch
            {
                < 1 / radiusDrag => radiusSpeed * radiusDrag,
                >= 1 / radiusDrag and <= radiusDrag => 1,
                _ => radiusSpeed / radiusDrag
            };


            // save the intersection points to draw the polygon bounding the disk.

            // TODO
            // calculate the intersections, based on them calculate the next position using Verlet integration

            HalfPlane? Separation(SoftDisk otherSoftDisk)
            {
                if (otherSoftDisk == this)
                    return null;
                Vector2 direction = (Vector2)(otherSoftDisk.center - center);
                float distance = direction.Length();
                direction.Normalize();
                // COMMENTING THIS LINE OUT SHOULDN'T MATTER BUT ICREASES THE BUG
                if (distance >= radius + otherSoftDisk.radius)
                    return null;

                float distToSeparation = (radius * radius - otherSoftDisk.radius * otherSoftDisk.radius + distance * distance) / (2 * distance);
                return new
                (
                    rayStart: distToSeparation * direction,
                    rayDir: C.OrthDir(direction: direction)
                );
            }

            // I think this deals with parallel half-planes correctly
            static HalfPlane[] ConvexHull(List<HalfPlane> halfPlanes)
            {
                halfPlanes = halfPlanes.OrderBy(halfPlane => halfPlane.angle).ToList();

                Deque<HalfPlane> curShell = new();
                foreach (var halfPlane in halfPlanes)
                {
                    while (curShell.Count > 1)
                    {
                        HalfPlane.Inter3Status interStatus = HalfPlane.GetInter3Status(halfPlaneA: curShell[^2], halfPlaneB: curShell[^1], halfPlaneC: halfPlane);
                        if (interStatus is HalfPlane.Inter3Status.InterEmpty)
                            return Array.Empty<HalfPlane>();
                        if (interStatus is HalfPlane.Inter3Status.MidUseful)
                            break;
                        curShell.RemoveFromBack();
                    }

                    while (curShell.Count > 1)
                    {
                        HalfPlane.Inter3Status interStatus = HalfPlane.GetInter3Status(halfPlaneA: halfPlane, halfPlaneB: curShell[0], halfPlaneC: curShell[1]);
                        if (interStatus is HalfPlane.Inter3Status.InterEmpty)
                            return Array.Empty<HalfPlane>();
                        if (interStatus is HalfPlane.Inter3Status.MidUseful)
                            break;
                        curShell.RemoveFromFront();
                    }

                    if (curShell.Count > 1)
                    {
                        HalfPlane.Inter3Status interStatus = HalfPlane.GetInter3Status(halfPlaneA: curShell[^1], halfPlaneB: halfPlane, halfPlaneC: curShell[0]);
                        if (interStatus is HalfPlane.Inter3Status.InterEmpty)
                            return Array.Empty<HalfPlane>();
                        if (interStatus is HalfPlane.Inter3Status.MidRedundant)
                            continue;
                    }
                    curShell.AddToBack(halfPlane);
                }

                if (curShell.Count < 3)
                    throw new();
                return curShell.ToArray();
            }
        }

        private double Area()
        {
            double area = 0;
            for (int ind = 0; ind < boundary.Length; ind++)
                area += SignedSegmentInterArea
                (
                    pointA: boundary[ind],
                    pointB: boundary[(ind + 1) % boundary.Length]
                );
            return Math.Max(0, area);

            double SignedSegmentInterArea(Vector2 pointA, Vector2 pointB)
            {
                int areaSign = Math.Sign(pointA.X * pointB.Y - pointB.X * pointA.Y);
                Vector2 segmentDir = pointB - pointA;
                segmentDir.Normalize();
                double centerLeg = Math.Abs(Vector2.Dot(pointA, C.OrthDir(segmentDir))),
                    signedLegToA = Vector2.Dot(-pointA, segmentDir),
                    signedLegToB = Vector2.Dot(pointB, segmentDir);

                return areaSign * (SignedRightTriangleInterArea(centerLeg: centerLeg, signedOtherLeg: signedLegToA)
                   + SignedRightTriangleInterArea(centerLeg: centerLeg, signedOtherLeg: signedLegToB));
            }

            double SignedRightTriangleInterArea(double centerLeg, double signedOtherLeg)
            {
                if (centerLeg is 0)
                    return 0;
                Debug.Assert(centerLeg > 0);
                int areaSign = Math.Sign(signedOtherLeg);
                double otherLeg = Math.Abs(signedOtherLeg),
                    potInsideLegSquared = radius * radius - centerLeg * centerLeg,
                    insideLegPart = potInsideLegSquared switch
                    {
                        > 0 => Math.Min(Math.Sqrt(potInsideLegSquared), otherLeg),
                        _ => 0
                    },
                    insideInterArea = centerLeg * insideLegPart / 2,
                    outsideAngle = Math.Atan(otherLeg / centerLeg) - Math.Atan(insideLegPart / centerLeg),
                    outsideInterArea = Math.Max(0, outsideAngle) * radius * radius;

                return areaSign * (insideInterArea + outsideInterArea);
            }
        }

        public void Draw()
        {
            if (boundary.Length < 3)
                return;

            AddPolygon
            (
                convexPolygon:
                    (from point in boundary
                     select new SoftDiskVertexFormat(softDisk: this, relToDiskCenterPos: point))
                     .ToArray()
            );

            //var worldViewProjection = worldToScreenTransform * projectionMatrix;

            //SoftDiskVertexFormat[] vertices = new SoftDiskVertexFormat[]
            //{
            //    new
            //    (
            //        worldCenter: worldCenter,
            //        worldViewProjection: worldViewProjection,
            //        shaderDisk: this,
            //        relToDiskCenterPos: radius * new Vector2(-1, -1)
            //    ),
            //    new
            //    (
            //        worldCenter: worldCenter,
            //        worldViewProjection: worldViewProjection,
            //        shaderDisk: this,
            //        relToDiskCenterPos: radius * new Vector2(1, -1)
            //    ),
            //    new
            //    (
            //        worldCenter: worldCenter,
            //        worldViewProjection: worldViewProjection,
            //        shaderDisk: this,
            //        relToDiskCenterPos: radius * new Vector2(1, 1)
            //    ),
            //};

            //vertexBuffer = new VertexBuffer(C.graphicsDevice, SoftDiskVertexFormat.vertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
            //vertexBuffer.SetData(vertices);
        }
    }
}
