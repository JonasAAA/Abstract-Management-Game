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

        public BasicResInd BasicResIndFromName(string resName)
        {
            foreach (var basicResInd in BasicResInd.All)
                if (resources[basicResInd].Name == resName)
                    return basicResInd;
            throw new ArgumentException();
        }
    }
}
