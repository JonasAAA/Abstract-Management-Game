namespace Game1.Lighting
{
    public interface ILightSource
    {
        public void ProduceAndDistributeRadiantEnergy(List<ILightCatchingObject> lightCatchingObjects, IRadiantEnergyConsumer vacuumAsRadiantEnergyConsumer);

        public void Draw(Matrix worldToScreenTransform, int actualScreenWidth, int actualScreenHeight);
    }
}
