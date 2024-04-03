using Game1.GlobalTypes;

namespace Game1.ContentHelpers
{
    [Serializable]
    public readonly struct FullValidRawMatPropor
    {
        public static Result<FullValidRawMatPropor, TextErrors> Create(ValidRawMatPropor rawMatPropor)
            => new(ok: new(rawMaterial: rawMatPropor.RawMaterial, percentage: rawMatPropor.Percentage));

        public RawMaterialID RawMaterial { get; }
        public ulong Percentage { get; }

        private FullValidRawMatPropor(RawMaterialID rawMaterial, ulong percentage)
        {
            RawMaterial = rawMaterial;
            Percentage = percentage;
        }
    }
}
