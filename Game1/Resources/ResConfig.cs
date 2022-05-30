namespace Game1.Resources
{
    [Serializable]
    public sealed class ResConfig
    {
        public readonly ResourceArray resources;

        public ResConfig()
            => resources = new();

        public void Initialize()
            => resources.Initialize();
    }
}
