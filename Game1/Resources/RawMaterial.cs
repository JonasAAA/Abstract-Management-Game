using static Game1.WorldManager;

namespace Game1.Resources
{
    [Serializable]
    public class RawMaterial : IResource
    {
        private static readonly Dictionary<ulong, RawMaterial> indToRawMat;

        static RawMaterial()
            => indToRawMat = new();

        public static RawMaterial Get(ulong ind)
        {
            if (indToRawMat.TryGetValue(ind, out var value))
                return value;
            throw new NotImplementedException();
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

        private readonly ulong ind;

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
            this.ind = ind;

            CurResConfig.AddRes(resource: this);
        }

        public RawMaterial GetFusionResult()
            => Get(ind: ind + 1);

        public UDouble Strength(Temperature temperature)
            => throw new NotImplementedException();
    }
}
