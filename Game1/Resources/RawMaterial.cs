using Game1.ContentNames;
using Game1.GlobalTypes;
using Game1.UI;
using static Game1.GlobalTypes.GameConfig;

namespace Game1.Resources
{
    [Serializable]
    public class RawMaterial : IResource
    {
        public static RawMaterial Create(RawMaterialID rawMatID)
            => new
            (
                rawMatID: rawMatID,
                name: rawMatID.Name(),
                iconName: TextureName.RawMaterialIconName(rawMatID: rawMatID),
                mass: ResAndIndustryAlgos.RawMaterialMass(rawMatID: rawMatID),
                heatCapacity: ResAndIndustryAlgos.RawMaterialHeatCapacity(rawMatID: rawMatID),
                fusionReactionStrengthCoeff: ResAndIndustryAlgos.RawMaterialFusionReactionStrengthCoeff(rawMatID: rawMatID)
            );

        public static IEnumerable<RawMaterial> GetAllRawMats()
            => Enum.GetValues<RawMaterialID>().Select
            (
                rawMatID => new RawMaterial
                (
                    rawMatID: rawMatID,
                    name: rawMatID.Name(),
                    iconName: TextureName.RawMaterialIconName(rawMatID: rawMatID),
                    mass: ResAndIndustryAlgos.RawMaterialMass(rawMatID: rawMatID),
                    heatCapacity: ResAndIndustryAlgos.RawMaterialHeatCapacity(rawMatID: rawMatID),
                    fusionReactionStrengthCoeff: ResAndIndustryAlgos.RawMaterialFusionReactionStrengthCoeff(rawMatID: rawMatID)
                )
            );

        public string Name { get; }
        public ConfigurableIcon Icon { get; }
        public ConfigurableIcon SmallIcon { get; }
        public Mass Mass { get; }
        public HeatCapacity HeatCapacity { get; }
        public AreaInt Area { get; }
        public RawMatAmounts RawMatComposition { get; }
        public UDouble FusionReactionStrengthCoeff { get; }
        public RawMaterialID RawMatID { get; }

        private RawMaterial(RawMaterialID rawMatID, string name, TextureName iconName, Mass mass, HeatCapacity heatCapacity, UDouble fusionReactionStrengthCoeff)
        {
            Name = name;
            Icon = new Icon(name: iconName, height: CurGameConfig.iconHeight).WithDefaultBackgroundColor();
            SmallIcon = new Icon(name: iconName, height: CurGameConfig.smallIconHeight).WithDefaultBackgroundColor();
            Mass = mass;
            HeatCapacity = heatCapacity;
            Area = ResAndIndustryAlgos.rawMaterialArea;
            RawMatComposition = new(res: this, amount: 1);
            FusionReactionStrengthCoeff = fusionReactionStrengthCoeff;
            RawMatID = rawMatID;
        }

        public RawMaterial GetFusionResult(ResConfig curResConfig)
            => RawMatID.Next() switch
            {
                RawMaterialID nextRawMatID => curResConfig.GetRawMatFromID(rawMatID: nextRawMatID),
                null => throw new ArgumentException()
            };

        public override string ToString()
            => Name;
    }
}
