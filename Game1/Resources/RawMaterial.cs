using Game1.ContentNames;
using Game1.UI;
using static Game1.GameConfig;
using static Game1.UI.ActiveUIManager;

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
                    iconName: TextureName.RawMaterialIconName(ind: ind),
                    mass: ResAndIndustryAlgos.RawMaterialMass(ind: ind),
                    heatCapacity: ResAndIndustryAlgos.RawMaterialHeatCapacity(ind: ind),
                    fusionReactionStrengthCoeff: ResAndIndustryAlgos.RawMaterialFusionReactionStrengthCoeff(ind: ind)
                );
        }

        public string Name { get; }
        public ConfigurableIcon Icon { get; }
        public ConfigurableIcon SmallIcon { get; }
        public Mass Mass { get; }
        public HeatCapacity HeatCapacity { get; }
        public AreaInt Area { get; }
        public RawMatAmounts RawMatComposition { get; }
        public UDouble FusionReactionStrengthCoeff { get; }
        public ulong Ind { get; }

        private RawMaterial(ulong ind, string name, TextureName iconName, Mass mass, HeatCapacity heatCapacity, UDouble fusionReactionStrengthCoeff)
        {
            Name = name;
            Icon = new Icon(name: iconName, height: CurGameConfig.iconHeight).WithDefaultBackgroundColor();
            SmallIcon = new Icon(name: iconName, height: CurGameConfig.smallIconHeight).WithDefaultBackgroundColor();
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
