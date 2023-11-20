namespace Game1.ContentNames
{
    [Serializable]
    public readonly struct ShaderName
    {
        public static readonly ShaderName
            starLight = new("StarLight");

        // This is a propertry instead of field so that saves from one operating system could be loaded on another one
        public readonly string Path
            => System.IO.Path.Combine("Shaders", name);

        private readonly string name;

        public ShaderName()
            => throw new InvalidOperationException();

        private ShaderName(string name)
            => this.name = name;
    }
}
