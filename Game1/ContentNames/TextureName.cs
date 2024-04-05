using Game1.GlobalTypes;

namespace Game1.ContentNames
{
    [Serializable]
    public readonly struct TextureName
    {
        public static readonly TextureName
            pixel = new("Pixel"),
            triangle = new("Triangle"),
            disk = new("Disk"),
            arrowDown = new("Arrow Down"),
            electricity = new("Electricity"),
            cosmicBody = new("Cosmic Body"),
            starlight = new("Starlight"),
            building = new("Building"),
            rawMaterialIcon = new("Raw Material"),
            materialIcon = new("Material"),
            resIcon = new("Resource"),
            beam = new("Beam"),
            screw = new("Screw"),
            gear = new("Gear"),
            wire = new("Wire"),
            circuit = new("Circuit"),
            processor = new("Processor");

        public static TextureName RawMaterialIconName(RawMaterialID rawMatID)
            => new($"Raw Material {rawMatID.RepresentativeNumber()}");

        public static TextureName PrimitiveMaterialIconName(RawMaterialID rawMatID)
            => new($"Primitive Material {rawMatID.RepresentativeNumber()}");

        // This is a propertry instead of field so that saves from one operating system could be loaded on another one
        public readonly string Path
            => System.IO.Path.Combine("Textures", name);

        private readonly string name;

        public TextureName()
            => throw new InvalidOperationException();

        private TextureName(string name)
            => this.name = name;
    }
}
