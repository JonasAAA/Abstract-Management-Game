using Game1.Collections;
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
        public SomeResAmounts<RawMaterial> RawMatComposition { get; }
        public Temperature MeltingPoint { get; }
        public readonly Color color;

        private RawMaterial(string name, Mass mass, HeatCapacity heatCapacity, AreaInt area, Temperature meltingPoint, Color color)
        {
            Name = name;
            Mass = mass;
            HeatCapacity = heatCapacity;
            Area = area;
            UsefulArea = area;
            MeltingPoint = meltingPoint;
            RawMatComposition = new(res: this, amount: 1);
            this.color = color;

            CurResConfig.AddRes(resource: this);
        }

        public UDouble Strength(Temperature temperature)
            => throw new NotImplementedException();
    }
}
