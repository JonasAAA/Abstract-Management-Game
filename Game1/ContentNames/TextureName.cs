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
            gear = new("Gear"),
            roofTile = new("Roof Tile"),
            wire = new("Wire");

        public static TextureName RawMaterialIconName(ulong ind)
            => ind <= ResAndIndustryAlgos.maxRawMatInd ? new($"Raw Material {ind}") : throw new ArgumentOutOfRangeException();

        public static TextureName PrimitiveMaterialIconName(ulong rawMatInd)
            => rawMatInd <= ResAndIndustryAlgos.maxRawMatInd ? new($"Primitive Material {rawMatInd}") : throw new ArgumentOutOfRangeException();

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
