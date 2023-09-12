using Game1.Collections;
using Game1.UI;

namespace Game1.Resources
{
    [Serializable]
    public sealed class ResConfig
    {
        public MaterialPaletteChoices StartingMaterialPaletteChoices { get; private set; }

        public EfficientReadOnlyCollection<IResource> AllCurRes
            => new(list: resources);

        private readonly Dictionary<ulong, RawMaterial> indToRawMat;
        private readonly List<IResource> resources;
        private readonly Dictionary<IResource, ulong> resToOrder;
        private ulong nextOrder;
        private readonly EfficientReadOnlyDictionary<IProductClass, List<MaterialPalette>> materialPalettes;

        public ResConfig()
        {
            resources = new();
            indToRawMat = new();
            resToOrder = new();
            nextOrder = 0;
            materialPalettes = IProductClass.all.ToEfficientReadOnlyDict(elementSelector: _ => new List<MaterialPalette>());
        }

        public void Initialize()
        {
            var material0 = Material.CreateAndAddToCurResConfig
            (
                name: "Material 0",
                rawMatAreaPropors: new
                (
                    res: RawMaterial.GetAndAddToCurResConfigIfNeeded(curResConfig: this, ind: 0),
                    amount: 1
                )
            );
            var material1 = Material.CreateAndAddToCurResConfig
            (
                name: "Material 1",
                rawMatAreaPropors: new
                (
                    res: RawMaterial.GetAndAddToCurResConfigIfNeeded(curResConfig: this, ind: 1),
                    amount: 1
                )
            );
            var material2 = Material.CreateAndAddToCurResConfig
            (
                name: "Material 0 and 1 mix",
                rawMatAreaPropors: new
                (
                    resAmounts: new List<ResAmount<RawMaterial>>()
                    {
                        new(res: RawMaterial.GetAndAddToCurResConfigIfNeeded(curResConfig: this, ind: 0), amount: 1),
                        new(res: RawMaterial.GetAndAddToCurResConfigIfNeeded(curResConfig: this, ind: 1), amount: 1)
                    }
                )
            );

            StartingMaterialPaletteChoices = MaterialPaletteChoices.Create
            (
                choices: new List<MaterialPalette>()
                {
                    MaterialPalette.CreateAndAddToResConfig
                    (
                        name: "default mechanical",
                        productClass: IProductClass.mechanical,
                        materialChoices: new()
                        {
                            [IMaterialPurpose.mechanical] = material0
                        }
                    ).UnwrapOrThrow(),
                    MaterialPalette.CreateAndAddToResConfig
                    (
                        name: "default electronics",
                        productClass: IProductClass.electronics,
                        materialChoices: new()
                        {
                            [IMaterialPurpose.electricalConductor] = material1,
                            [IMaterialPurpose.electricalInsulator] = material0
                        }
                    ).UnwrapOrThrow(),
                    MaterialPalette.CreateAndAddToResConfig
                    (
                        name: "default roof",
                        productClass: IProductClass.roof,
                        materialChoices: new()
                        {
                            [IMaterialPurpose.roofSurface] = material2
                        }
                    ).UnwrapOrThrow()
                }
            );
            foreach (var prodParams in Product.productParamsDict.Values)
                prodParams.GetProduct(materialPalette: StartingMaterialPaletteChoices[prodParams.productClass]);
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

        /// <summary>
        /// Returns error string if material palette with such name already exists
        /// </summary>
        public Result<UnitType, TextErrors> AddMaterialPalette(MaterialPalette materialPalette)
        {
            var relevantMatPalettes = materialPalettes[materialPalette.productClass];
            foreach (var otherMatPalette in relevantMatPalettes)
            {
                if (otherMatPalette.name == materialPalette.name)
                    return new(errors: new(UIAlgorithms.MatPaletteWithThisNameAlreadyExists));
                var possibleErrors = materialPalette.VerifyThatHasdifferentContents(otherMatPalette: otherMatPalette);
                if (!possibleErrors.isOk)
                    return possibleErrors;
            }
            relevantMatPalettes.Add(materialPalette);
            return new(ok: new());
        }

        public EfficientReadOnlyCollection<MaterialPalette> GetMatPalettes(IProductClass productClass)
            => new(list: materialPalettes[productClass]);
    }
}
