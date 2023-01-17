namespace Game1.Resources
{
    [Serializable]
    public class EnergyCounter<T> : Counter<T>
        where T : struct, IFormOfEnergy<T>
    {
        public new static EnergyCounter<T> CreateEmpty()
            => new(createdByMagic: false);

        public new static EnergyCounter<T> CreateCounterByMagic(T count)
            => new(createdByMagic: true)
            {
                Count = count
            };

        private EnergyCounter(bool createdByMagic)
            : base(createdByMagic: createdByMagic)
        { }

        public void TransformFrom<U>(EnergyCounter<U> source, T count)
            where U : struct, IUnconstrainedEnergy<U>
        {
            source.Count -= U.CreateFromEnergy(energy: (Energy)count);
            Count += count;
        }

        public void TransformTo<U>(EnergyCounter<U> destin, T count)
            where U : struct, IUnconstrainedEnergy<U>
        {
            destin.Count += U.CreateFromEnergy(energy: (Energy)count);
            Count -= count;
        }
    }
}
