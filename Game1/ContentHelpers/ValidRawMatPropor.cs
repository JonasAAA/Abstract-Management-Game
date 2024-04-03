using Game1.GlobalTypes;

namespace Game1.ContentHelpers
{
    [Serializable]
    public readonly struct ValidRawMatPropor
    {
        public static ValidRawMatPropor CreateOrThrow(RawMatPropor rawMatPropor)
            => CreateOrThrow
            (
                rawMaterial: RawMaterialIDUtil.FromRepresentativeNumber(rawMatPropor.RawMaterial)
                    ?? throw new ContentException($"{nameof(rawMatPropor.RawMaterial)} {rawMatPropor.RawMaterial} is not valid"),
                percentage: rawMatPropor.Percentage switch
                {
                    >= 0 => (ulong)rawMatPropor.Percentage,
                    _ => throw new ContentException($"Percentage must not be negative")
                }
            );

        public static ValidRawMatPropor CreateOrThrow(RawMaterialID rawMaterial, ulong percentage)
            => new
            (
                rawMaterial: rawMaterial,
                percentage: percentage switch
                {
                    <= 100 => percentage,
                    _ => throw new ContentException($"Percentage must not be bigger than 100")
                }
            );

        public RawMaterialID RawMaterial { get; }
        public ulong Percentage { get; }

        private ValidRawMatPropor(RawMaterialID rawMaterial, ulong percentage)
        {
            RawMaterial = rawMaterial;
            Percentage = percentage;
        }

        public RawMatPropor ToJsonable()
            => new()
            {
                RawMaterial = (int)RawMaterial.RepresentativeNumber(),
                Percentage = (int)Percentage,
            };
    }
}
