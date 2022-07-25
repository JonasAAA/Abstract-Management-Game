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
        private readonly bool createdByMagic;
        
        private MassCounter(bool createdByMagic)
        {
            Mass = Mass.zero;
            this.createdByMagic = createdByMagic;
        }

        public void TransferFrom(MassCounter source, Mass mass)
        {
            if (source == this)
                return;
            source.Mass -= mass;
            Mass += mass;
        }

#if DEBUG
        ~MassCounter()
        {
            if (!createdByMagic && !Mass.IsZero)
                throw new Exception();
        }
#endif
    }
}
