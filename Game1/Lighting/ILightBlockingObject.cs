namespace Game1.Lighting
{
    public interface ILightBlockingObject
    {
        public AngleArc.Params BlockedAngleArcParams(MyVector2 lightPos);

        /// <summary>
        /// Throws exception if doesn't intersect
        /// </summary>
        public double CloserInterPoint(MyVector2 lightPos, MyVector2 lightDir);
    }
}
