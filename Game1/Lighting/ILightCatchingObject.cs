namespace Game1.Lighting
{
    public interface ILightCatchingObject : ILightBlockingObject, IRadiantEnergyConsumer
    {
        public sealed AngleArc BlockedAngleArc(MyVector2 lightPos)
            => new
            (
                parameters: BlockedAngleArcParams(lightPos: lightPos),
                lightCatchingObject: this
            );
    }
}
