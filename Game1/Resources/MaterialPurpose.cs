using Game1.Collections;

namespace Game1.Resources
{
    // If make ProductClass or MaterialPurpose into a (record) struct, some initialization failure happens and
    // System.TypeLoadException exception is thrown. Couldn't figure out how to fix that, so both of those will be reference types for now.
    [Serializable]
    public record MaterialPurpose
    {
        public static readonly MaterialPurpose mechanical = new(name: "Mechanical");
        public static readonly MaterialPurpose electricalConductor = new(name: "Electrical Conductor");
        public static readonly MaterialPurpose electricalInsulator = new(name: "Electrical Insulator");

        // DON'T forget to put all material purposes in this list.
        // There is a test to check that
        public static readonly EfficientReadOnlyCollection<MaterialPurpose> all = new List<MaterialPurpose> { mechanical, electricalConductor, electricalInsulator }.ToEfficientReadOnlyCollection();

        private readonly string name;

        private MaterialPurpose(string name)
            => this.name = name;

        public override string ToString()
            => name;
    }
}
