namespace Game1.ContentNames
{
    [Serializable]
    public readonly struct FontName
    {
        public static readonly FontName
            mainFont = new("MainFont");

        // This is a propertry instead of field so that saves from one operating system could be loaded on another one
        public readonly string Path
            => System.IO.Path.Combine("Fonts", name);

        private readonly string name;

        public FontName()
            => throw new InvalidOperationException();

        private FontName(string name)
            => this.name = name;
    }
}
