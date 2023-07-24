namespace Game1.Resources
{
    [Serializable]
    public sealed class ResConfig
    {
        private readonly Dictionary<ulong, RawMaterial> indToRawMat;
        private readonly List<IResource> resources;
        private readonly Dictionary<IResource, ulong> resToOrder;
        private ulong nextOrder;

        public ResConfig()
        {
            resources = new();
            indToRawMat = new();
            resToOrder = new();
            nextOrder = 0;
        }

        public RawMaterial? GetRawMatFromInd(ulong ind)
            => indToRawMat.GetValueOrDefault(key: ind);

        public IEnumerable<IResource> GetAllCurRes()
            => resources;

        public void AddRes(IResource resource)
        {
            resources.Add(resource);
            resToOrder.Add(key: resource, value: nextOrder);
            nextOrder++;
            if (resource is RawMaterial rawMaterial)
            {
                Debug.Assert(!indToRawMat.ContainsKey(rawMaterial.Ind));
                indToRawMat[rawMaterial.Ind] = rawMaterial;
            }
        }

        public int CompareRes(IResource left, IResource right)
            => resToOrder[left].CompareTo(resToOrder[right]);
    }
}
