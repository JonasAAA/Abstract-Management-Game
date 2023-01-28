namespace Game1.Resources
{
    [Serializable]
    public class EnergyCounter<TAmount> : Counter<TAmount>
        where TAmount : struct, IFormOfEnergy<TAmount>
    {
        public new static EnergyCounter<TAmount> CreateEmpty()
            => new(createdByMagic: false);

        public new static EnergyCounter<TAmount> CreateByMagic(TAmount count)
            => new(createdByMagic: true)
            {
                Count = count
            };

        private EnergyCounter(bool createdByMagic)
            : base(createdByMagic: createdByMagic)
        { }

        public void TransformFrom<TSourceAmount>(EnergyCounter<TSourceAmount> source, TAmount destinCount)
            where TSourceAmount : struct, IUnconstrainedEnergy<TSourceAmount>
        {
            source.Count -= TSourceAmount.CreateFromEnergy(energy: (Energy)destinCount);
            Count += destinCount;
        }

        public void TransformTo<TDestinAmount>(EnergyCounter<TDestinAmount> destin, TAmount sourceCount)
            where TDestinAmount : struct, IUnconstrainedEnergy<TDestinAmount>
        {
            destin.Count += TDestinAmount.CreateFromEnergy(energy: (Energy)sourceCount);
            Count -= sourceCount;
        }
    }
}
