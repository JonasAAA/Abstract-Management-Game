namespace Game1.Resources
{
    [Serializable]
    public sealed class ResConfig
    {
        private readonly List<IResource> resources;
        private readonly Dictionary<IResource, ulong> resToOrder;
        private ulong nextOrder;

        public ResConfig()
        {
            resources = new();
            resToOrder = new();
            nextOrder = 0;
        }

        public IEnumerable<ResOrRawMatsMix> GetAllCurResOrRawMatsMix()
        {
            yield return ResOrRawMatsMix.FromRawMatsMix();
            foreach (var res in resources)
                yield return ResOrRawMatsMix.FromRes(res: res);
        }

        public void AddRes(IResource resource)
        {
            resources.Add(resource);
            resToOrder.Add(key: resource, value: nextOrder);
            nextOrder++;
        }

        public int CompareRes(IResource left, IResource right)
            => resToOrder[left].CompareTo(resToOrder[right]);
    }
}
