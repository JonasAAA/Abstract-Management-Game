namespace Game1.Lighting
{
    public interface IRadiantEnergyConsumer
    {
        public NodeID? NodeID { get; }

        // May have Propor powerPropor parameter as well
        public void TakeRadiantEnergyFrom(EnergyPile<RadiantEnergy> source, RadiantEnergy amount);

        public void EnergyTakingComplete(IRadiantEnergyConsumer reflectedEnergyDestin);
    }
}
