using static Game1.WorldManager;

namespace Game1.Resources
{
    [Serializable]
    public sealed class Material : IResource
    {
        private readonly SomeResAmounts<RawMaterial> composition;

        public Mass Mass { get; }
        public HeatCapacity HeatCapacity { get; }
        public ulong Area { get; }
        public SomeResAmounts<RawMaterial> RawMatComposition { get; }
        public Propor Reflectance { get; }
        public Propor Emissivity { get; }

        public Material(SomeResAmounts<RawMaterial> composition)
        {
            this.composition = composition;
            Mass = composition.Mass();
            HeatCapacity = composition.HeatCapacity();
            Area = composition.Area();
            RawMatComposition = composition.RawMatComposition();
            Reflectance = Propor.Create
            (
                part: composition.Sum(resAndAmount => resAndAmount.res.Area * resAndAmount.amount * resAndAmount.res.Reflectance),
                whole: composition.Sum(resAndAmount => resAndAmount.res.Area * resAndAmount.amount)
            )!.Value;
            Emissivity = Propor.Create
            (
                part: composition.Sum(resAndAmount => resAndAmount.res.Area * resAndAmount.amount * resAndAmount.res.Emissivity),
                whole: composition.Sum(resAndAmount => resAndAmount.res.Area * resAndAmount.amount)
            )!.Value;

            CurResConfig.AddRes(resource: this);
        }

        public ulong GetAmountFromArea(ulong area)
            => throw new NotImplementedException();
    }
}
