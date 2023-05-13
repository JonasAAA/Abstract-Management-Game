using static Game1.WorldManager;

namespace Game1.Resources
{
    [Serializable]
    public class RawMaterial : IResource
    {
        private static readonly Dictionary<ulong, RawMaterial> indToRawMat;

        static RawMaterial()
            => indToRawMat = new();

        /// <summary>
        /// For an
        /// </summary>
        public static RawMaterial Get(ulong ind)
        {
            if (indToRawMat.TryGetValue(ind, out var value))
                return value;
            throw new NotImplementedException();
        }

        public string Name { get; }
        public Mass Mass { get; }
        public HeatCapacity HeatCapacity { get; }
        public ulong Area { get; }
        public SomeResAmounts<RawMaterial> RawMatComposition { get; }
        public Propor Reflectance { get; }
        public Propor Emissivity { get; }
        public readonly Color color;

        private RawMaterial(string name, Mass mass, HeatCapacity heatCapacity, ulong area, Propor reflectance, Propor emissivity, Color color)
        {
            Name = name;
            Mass = mass;
            HeatCapacity = heatCapacity;
            Area = area;
            Reflectance = reflectance;
            Emissivity = emissivity;
            RawMatComposition = new(res: this, amount: 1);
            this.color = color;

            CurResConfig.AddRes(resource: this);
        }
    }
}
