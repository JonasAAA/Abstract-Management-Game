namespace Game1.Resources
{
    [Serializable]
    public class MassCounter
    {
        public static MassCounter CreateEmpty()
            => new(createdByMagic: false);

        public static MassCounter CreateMassCounterByMagic(Mass mass)
            => new(createdByMagic: true)
            {
                Mass = mass
            };

        public Mass Mass { get; private set; }
#if DEBUG2
        private readonly bool createdByMagic;
#endif

        private MassCounter(bool createdByMagic)
        {
            Mass = Mass.zero;
#if DEBUG2
            this.createdByMagic = createdByMagic;
#endif
        }

        public void TransferFrom(MassCounter source, Mass mass)
        {
            if (source == this)
                return;
            source.Mass -= mass;
            Mass += mass;
        }

#if DEBUG2
        ~MassCounter()
        {
            if (!createdByMagic && !Mass.IsZero)
                throw new Exception();
        }
#endif
    }
}
