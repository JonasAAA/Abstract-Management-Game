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

        public IEnumerable<TRes> GetCurRes<TRes>()
            where TRes : class, IResource
        {
            foreach (var res in resources)
                if (res is TRes wantedRes)
                    yield return wantedRes;
        }

        public IEnumerable<IResource> GetAllCurRes()
            => resources;

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
