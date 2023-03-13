namespace Game1.Lighting
{
    public interface ILightBlockingObject
    {
        public AngleArc.Params BlockedAngleArcParams(MyVector2 lightPos);
    }
}
