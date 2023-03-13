using Game1.Shapes;

namespace Game1.Lighting
{
    [Serializable]
    public sealed class LightCatchingDisk : Disk, ILightBlockingObject
    {
        public LightCatchingDisk(IParams parameters)
            : base(parameters: parameters)
        { }

        AngleArc.Params ILightBlockingObject.BlockedAngleArcParams(MyVector2 lightPos)
        {
            UDouble dist = MyVector2.Distance(lightPos, Center);
            if (dist <= parameters.Radius)
            {
                // light source in inside the disk
                // just return empty interval for the time being
                return new();
                //return new(startAngle: -MyMathHelper.pi + MyMathHelper.minPosDouble, endAngle: MyMathHelper.pi - MyMathHelper.minPosDouble, HUDRadius: parameters.HUDRadius);
            }

            double halfArcSin = parameters.Radius / dist,
                  halfArcCos = MyMathHelper.Sqrt((UDouble)(1 - halfArcSin * halfArcSin));

            // keyPoint is the intersection between the light ray and the two touching points
            MyVector2 keyPoint = Center * halfArcCos * halfArcCos + lightPos * halfArcSin * halfArcSin,
                    relPos = Center - lightPos,
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
            //double halfArcSin = parameters.HUDRadius / dist,
            //    mainAngle = MyMathHelper.Atan2(y: (Center - lightPos).Y, x: (Center - lightPos).Y);
            //return new AngleArc.Params
            //(
            //    startAngle: MyMathHelper.WrapAngle(mainAngle - MyMathHelper.Asin(halfArcSin)),
            //    endAngle: MyMathHelper.WrapAngle(mainAngle + MyMathHelper.Asin(halfArcSin)),
            //    radius: MyMathHelper.Sqrt((UDouble)(dist * dist - parameters.HUDRadius * parameters.HUDRadius))
            //);
        }
    }
}
