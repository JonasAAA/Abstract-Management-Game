namespace Game1.Resources
{
    [Serializable]
    public class RawMaterial : IResource
    {
        public static RawMaterial GetAndAddToCurResConfigIfNeeded(ResConfig curResConfig, ulong ind)
        {
            if (ind > ResAndIndustryAlgos.maxRawMatInd)
                throw new ArgumentException();
            if (curResConfig.GetRawMatFromInd(ind: ind) is RawMaterial rawMaterial)
                return rawMaterial;
            return new
            (
                curResConfig: curResConfig,
                ind: ind,
                name: ResAndIndustryAlgos.RawMaterialName(ind: ind),
                mass: ResAndIndustryAlgos.RawMaterialMass(ind: ind),
                heatCapacity: ResAndIndustryAlgos.RawMaterialHeatCapacity(ind: ind),
                color: ResAndIndustryAlgos.RawMaterialColor(ind: ind),
                fusionReactionStrengthCoeff: ResAndIndustryAlgos.RawMaterialFusionReactionStrengthCoeff(ind: ind)
            );
        }

        public Mass Mass { get; }
        public HeatCapacity HeatCapacity { get; }
        public AreaInt Area { get; }
        public RawMatAmounts RawMatComposition { get; }
        public Color Color { get; }
        public UDouble FusionReactionStrengthCoeff { get; }
        public ulong Ind { get; }

        private readonly string name;

        private RawMaterial(ResConfig curResConfig, ulong ind, string name, Mass mass, HeatCapacity heatCapacity, Color color, UDouble fusionReactionStrengthCoeff)
        {
            this.name = name;
            Mass = mass;
            HeatCapacity = heatCapacity;
            Area = ResAndIndustryAlgos.rawMaterialArea;
            RawMatComposition = new(res: this, amount: 1);
            Color = color;
            FusionReactionStrengthCoeff = fusionReactionStrengthCoeff;
            Ind = ind;

            curResConfig.AddRes(resource: this);
        }

        public RawMaterial GetFusionResult(ResConfig curResConfig)
            => GetAndAddToCurResConfigIfNeeded(curResConfig: curResConfig, ind: Ind + 1);

        public override string ToString()
            => name;
    }
}
