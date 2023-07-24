using static Game1.WorldManager;

namespace Game1.Resources
{
    [Serializable]
    public class RawMaterial : IResource
    {
        public static RawMaterial Get(ulong ind)
        {
            if (CurResConfig.GetRawMatFromInd(ind: ind) is RawMaterial rawMaterial)
                return rawMaterial;
            return new
            (
                ind: ind,
                name: ResAndIndustryAlgos.RawMaterialName(ind: ind),
                mass: ResAndIndustryAlgos.RawMaterialMass(ind: ind),
                heatCapacity: ResAndIndustryAlgos.RawMaterialHeatCapacity(ind: ind),
                area: ResAndIndustryAlgos.RawMaterialArea(ind: ind),
                meltingPoint: ResAndIndustryAlgos.RawMaterialMeltingPoint(ind: ind),
                color: ResAndIndustryAlgos.RawMaterialColor(ind: ind),
                fusionReactionStrengthCoeff: ResAndIndustryAlgos.RawMaterialFusionReactionStrengthCoeff(ind: ind)
            );
        }

        public string Name { get; }
        public Mass Mass { get; }
        public HeatCapacity HeatCapacity { get; }
        public AreaInt Area { get; }
        public AreaInt UsefulArea { get; }
        public RawMatAmounts RawMatComposition { get; }
        public Temperature MeltingPoint { get; }
        public Color Color { get; }
        public UDouble FusionReactionStrengthCoeff { get; }
        public ulong Ind { get; }

        private RawMaterial(ulong ind, string name, Mass mass, HeatCapacity heatCapacity, AreaInt area, Temperature meltingPoint, Color color, UDouble fusionReactionStrengthCoeff)
        {
            Name = name;
            Mass = mass;
            HeatCapacity = heatCapacity;
            Area = area;
            UsefulArea = area;
            MeltingPoint = meltingPoint;
            RawMatComposition = new(res: this, amount: 1);
            Color = color;
            FusionReactionStrengthCoeff = fusionReactionStrengthCoeff;
            Ind = ind;

            CurResConfig.AddRes(resource: this);
        }

        public RawMaterial GetFusionResult()
            => Get(ind: Ind + 1);

        public override string ToString()
            => Name;
    }
}
