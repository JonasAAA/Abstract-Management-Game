namespace Game1.Lighting
{
    public interface ILightCatchingObject : ILightBlockingObject
    {
        public sealed AngleArc BlockedAngleArc(MyVector2 lightPos)
            => new
            (
                parameters: BlockedAngleArcParams(lightPos: lightPos),
                lightCatchingObject: this
            );

        // May have Propor powerPropor parameter as well
        public void TakeRadiantEnergyFrom<T>(T source, RadiantEnergy radiantEnergy)
            where T : ISourcePile<RadiantEnergy>;
    }
}
