namespace Game1.Lighting
{
    public interface ILightCatchingObject : ILightBlockingObject, IDeletable
    {
        public sealed AngleArc BlockedAngleArc(MyVector2 lightPos)
            => new
            (
                parameters: BlockedAngleArcParams(lightPos: lightPos),
                lightCatchingObject: this
            );

        public void BeginSetWatts();

        public void SetWatts(StarID starPos, UDouble watts, Propor powerPropor);
    }
}
