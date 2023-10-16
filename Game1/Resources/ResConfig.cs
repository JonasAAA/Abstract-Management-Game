using Game1.Collections;
using Game1.UI;

namespace Game1.Resources
{
    [Serializable]
    public sealed class ResConfig
    {
        public MaterialPaletteChoices StartingMaterialPaletteChoices { get; private set; }

        public SortedResSet<IResource> AllCurRes
            => SortedResSet<IResource>.FromSortedUniqueResListUnsafe(sortedUniqueResList: resources.ToEfficientReadOnlyCollection());

        private readonly Dictionary<ulong, RawMaterial> indToRawMat;
        private readonly List<IResource> resources;
        private readonly Dictionary<IResource, ulong> resToOrder;
        private ulong nextMaterialInd, nextMatPaletteInd;
        private readonly EfficientReadOnlyDictionary<ProductClass, List<MaterialPalette>> materialPalettes;
        private readonly Dictionary<MaterialPalette, ulong> matPaletteToInd;
        private readonly EfficientReadOnlyDictionary<ProductClass, ulong> prodClassToInd;
        private const ulong
            rawMatOrderOffset = 0,
            materialOrderOffset = 1_000_000,
            productOrderOffset = 2_000_000,
            maxMaterialPaletteCount = 1_000_000,
            maxProdInClassCount = ResAndIndustryAlgos.maxProductIndInClass + 1;

        public ResConfig()
        {
            resources = new();
            indToRawMat = new();
            resToOrder = new();
            matPaletteToInd = new();
            prodClassToInd = ProductClass.all.Select((prodClass, ind) => (prodClass, ind)).ToEfficientReadOnlyDict
            (
                keySelector: prodClassAndInd => prodClassAndInd.prodClass,
                elementSelector: prodClassAndInd => (ulong)prodClassAndInd.ind
            );
            nextMaterialInd = 0;
            nextMatPaletteInd = 0;
            materialPalettes = ProductClass.all.ToEfficientReadOnlyDict(elementSelector: _ => new List<MaterialPalette>());
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
                        name: "def. mech.",
                        productClass: ProductClass.mechanical,
                        materialChoices: new()
                        {
                            [MaterialPurpose.mechanical] = material0
                        }
                    ).UnwrapOrThrow(),
                    MaterialPalette.CreateAndAddToResConfig
                    (
                        name: "def. elec.",
                        productClass: ProductClass.electronics,
                        materialChoices: new()
                        {
                            [MaterialPurpose.electricalConductor] = material0,
                            [MaterialPurpose.electricalInsulator] = material1
                        }
                    ).UnwrapOrThrow(),
                    MaterialPalette.CreateAndAddToResConfig
                    (
                        name: "def. roof",
                        productClass: ProductClass.roof,
                        materialChoices: new()
                        {
                            [MaterialPurpose.roofSurface] = material2
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
            resToOrder.Add(key: resource, value: GetOrder(resource: resource));
            resources.Sort();
            if (resource is RawMaterial rawMaterial)
            {
                Debug.Assert(!indToRawMat.ContainsKey(rawMaterial.Ind));
                indToRawMat[rawMaterial.Ind] = rawMaterial;
            }

            ulong GetOrder(IResource resource)
                => resource switch
                {
                    RawMaterial rawMaterial => rawMatOrderOffset + rawMaterial.Ind,
                    Material => materialOrderOffset + nextMaterialInd++,
                    Product product => productOrderOffset + product.IndInClass + maxProdInClassCount * (matPaletteToInd[product.MaterialPalette] + maxMaterialPaletteCount * prodClassToInd[product.ProductClass]),
                    _ => throw new ArgumentException()
                };
        }

        /// <summary>
        /// Want the order to be:
        /// * Raw materials (0, 1, ...)
        /// * Materials (by order of creation)
        ///   Alphabetic order is difficult to implement as player may rename some material, and would be difficult to reflect that
        ///   change in all resAmounts.
        ///   Maybe could store internally in any order and only when showing to the player, order nicely, e.g. alphabetically
        /// * Products (material palettes sorted by order of creation):
        ///   * Product class 0 + Material palette 0
        ///     * Prod params 0
        ///     * Prod params 1
        ///   * Product class 0 + Material palette 1
        ///   * Product class 1 + Material palette 0
        ///   * Product class 1 + Material palette 1
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public int CompareRes(IResource left, IResource right)
            => resToOrder[left].CompareTo(resToOrder[right]);

        public int CompareNullableRes<TRes>(TRes? left, TRes? right)
            where TRes : class, IResource
            => (left, right) switch
            {
                (not null, not null) => CompareRes(left: left, right: right),
                (not null, null) => -1,
                (null, not null) => 1,
                (null, null) => 0
            };

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
            matPaletteToInd[materialPalette] = nextMatPaletteInd++;
            return new(ok: new());
        }

        public EfficientReadOnlyCollection<MaterialPalette> GetMatPalettes(ProductClass productClass)
            => new(list: materialPalettes[productClass]);
    }
}
