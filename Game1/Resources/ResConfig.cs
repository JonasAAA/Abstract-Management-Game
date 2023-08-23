namespace Game1.Resources
{
    [Serializable]
    public sealed class ResConfig
    {
        public MaterialChoices StartingMaterialChoices { get; private set; }

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

        public void Initialize()
        {
            var material0 = Material.CreateAndAddToCurResConfig
            (
                name: "Material 0",
                composition: new
                (
                    res: RawMaterial.GetAndAddToCurResConfigIfNeeded(curResConfig: this, ind: 0),
                    amount: 1
                )
            );
            var material1 = Material.CreateAndAddToCurResConfig
            (
                name: "Material 1",
                composition: new
                (
                    res: RawMaterial.GetAndAddToCurResConfigIfNeeded(curResConfig: this, ind: 1),
                    amount: 1
                )
            );
            var material2 = Material.CreateAndAddToCurResConfig
            (
                name: "Material 0 and 1 mix",
                composition: new
                (
                    resAmounts: new List<ResAmount<RawMaterial>>()
                    {
                        new(res: RawMaterial.GetAndAddToCurResConfigIfNeeded(curResConfig: this, ind: 0), amount: 1),
                        new(res: RawMaterial.GetAndAddToCurResConfigIfNeeded(curResConfig: this, ind: 1), amount: 1)
                    }
                )
            );
            StartingMaterialChoices = new()
            {
                [IMaterialPurpose.electricalInsulator] = material0,
                [IMaterialPurpose.electricalConductor] = material1,
                [IMaterialPurpose.roofSurface] = material2,
            };
            foreach (var prodParams in Product.productParamsDict.Values)
                prodParams.GetProduct(materialChoices: StartingMaterialChoices);
        }

        public RawMaterial? GetRawMatFromInd(ulong ind)
            => indToRawMat.GetValueOrDefault(key: ind);

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
