using Game1.ContentNames;
using Game1.UI;

namespace Game1.Resources
{
    [Serializable]
    public class RawMaterial : IResource
    {
        public static IEnumerable<RawMaterial> GetInitialRawMats()
        {
            for (ulong ind = 0; ind <= ResAndIndustryAlgos.maxRawMatInd; ind++)
                yield return new
                (
                    ind: ind,
                    name: ResAndIndustryAlgos.RawMaterialName(ind: ind),
                    icon: new Icon(name: TextureName.RawMaterialIconName(ind: ind)),
                    mass: ResAndIndustryAlgos.RawMaterialMass(ind: ind),
                    heatCapacity: ResAndIndustryAlgos.RawMaterialHeatCapacity(ind: ind),
                    fusionReactionStrengthCoeff: ResAndIndustryAlgos.RawMaterialFusionReactionStrengthCoeff(ind: ind)
                );
        }

        public string Name { get; }
        public IImage Icon { get; }
        public Mass Mass { get; }
        public HeatCapacity HeatCapacity { get; }
        public AreaInt Area { get; }
        public RawMatAmounts RawMatComposition { get; }
        public UDouble FusionReactionStrengthCoeff { get; }
        public ulong Ind { get; }

        private RawMaterial(ulong ind, string name, IImage icon, Mass mass, HeatCapacity heatCapacity, UDouble fusionReactionStrengthCoeff)
        {
            Name = name;
            Icon = icon;
            Mass = mass;
            HeatCapacity = heatCapacity;
            Area = ResAndIndustryAlgos.rawMaterialArea;
            RawMatComposition = new(res: this, amount: 1);
            FusionReactionStrengthCoeff = fusionReactionStrengthCoeff;
            Ind = ind;
        }

        public RawMaterial GetFusionResult(ResConfig curResConfig)
            => curResConfig.GetRawMatFromInd(ind: Ind + 1);

        public override string ToString()
            => Name;
    }
}
