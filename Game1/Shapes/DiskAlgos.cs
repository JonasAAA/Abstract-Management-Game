using Game1.Lighting;

namespace Game1.Shapes
{
    public static class DiskAlgos
    {
        private static readonly Texture2D diskTexture = C.LoadTexture(name: "big disk");

        public static UDouble RadiusFromArea(AreaDouble area)
            => MyMathHelper.Sqrt(area.valueInMetSq / MyMathHelper.pi);

        public static AreaDouble Area(UDouble radius)
            => AreaDouble.CreateFromMetSq(valueInMetSq: MyMathHelper.pi * radius * radius);

        public static UDouble Length(UDouble radius)
            => 2 * MyMathHelper.pi * radius;

        public static AngleArc.Params BlockedAngleArcParams(MyVector2 center, UDouble radius, MyVector2 lightPos)
        {
            UDouble dist = MyVector2.Distance(lightPos, center);
            if (dist <= radius)
            {
                // light source in inside the disk
                // just return empty interval for the time being
                return new();
                //return new(startAngle: -MyMathHelper.pi + MyMathHelper.minPosDouble, endAngle: MyMathHelper.pi - MyMathHelper.minPosDouble, radius: parameters.radius);
            }

            double halfArcSin = radius / dist,
                  halfArcCos = MyMathHelper.Sqrt((UDouble)(1 - halfArcSin * halfArcSin));

            // keyPoint is the intersection between the light ray and the two touching points
            MyVector2 keyPoint = center * halfArcCos * halfArcCos + lightPos * halfArcSin * halfArcSin,
                    relPos = center - lightPos,
                    orth = MyMathHelper.Rotate90DegClockwise(vector: relPos),
                    arcStartPoint = keyPoint + orth * halfArcSin * halfArcCos - lightPos,
                    arcEndPoint = keyPoint - orth * halfArcSin * halfArcCos - lightPos;
            return new
            (
                startAngle: MyMathHelper.Atan2(arcStartPoint.Y, arcStartPoint.X),
                endAngle: MyMathHelper.Atan2(arcEndPoint.Y, arcEndPoint.X),
                radius: arcStartPoint.Length()
            );

            // This simpler calculation doesn't work for some reason
            //double halfArcSin = parameters.radius / dist,
            //    mainAngle = MyMathHelper.Atan2(y: (Center - lightPos).Y, x: (Center - lightPos).Y);
            //return new AngleArc.GeneralParams
            //(
            //    startAngle: MyMathHelper.WrapAngle(mainAngle - MyMathHelper.Asin(halfArcSin)),
            //    endAngle: MyMathHelper.WrapAngle(mainAngle + MyMathHelper.Asin(halfArcSin)),
            //    radius: MyMathHelper.Sqrt((UDouble)(dist * dist - parameters.radius * parameters.radius))
            //);
        }

        public static bool Contains(MyVector2 center, UDouble radius, MyVector2 otherPos)
            => MyVector2.Distance(otherPos, center) < radius;

        public static void Draw(MyVector2 center, UDouble radius, Color color)
            => C.Draw
            (
                texture: diskTexture,
                position: center,
                color: color,
                rotation: 0,
                origin: new MyVector2(diskTexture.Width, diskTexture.Height) * .5,
                scale: 2 * radius / (UDouble)diskTexture.Width
            );
    }
}
