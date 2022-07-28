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
                throw new ArgumentException("light source in inside the disk");

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
        }

        double ILightBlockingObject.CloserInterPoint(MyVector2 lightPos, MyVector2 lightDir)
        {
            MyVector2 relLightPos = lightPos - Center;
            double e = MyVector2.Dot(lightDir, relLightPos),
                f = MyVector2.Dot(relLightPos, relLightPos) - parameters.Radius * parameters.Radius,
                g = e * e - f,
                h = MyMathHelper.Sqrt((UDouble)g);
            // the two intersection points are -e + h and -e - h
            return -e - h;
        }
    }
}
