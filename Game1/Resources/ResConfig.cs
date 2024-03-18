using Game1.Collections;
using Game1.ContentNames;
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
        private readonly Dictionary<RawMaterial, Material> rawMatToPrimitiveMat;
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
            resources = [];
            indToRawMat = [];
            rawMatToPrimitiveMat = [];
            resToOrder = [];
            matPaletteToInd = [];
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
            // Can't be done in constructor due to AddRes calling Compare which refers to this which is not yet defined
            foreach (var rawMat in RawMaterial.GetInitialRawMats())
                AddRes(rawMat);

            foreach (var rawMat in indToRawMat.Values)
                Material.CreateAndAddToCurResConfig
                (
                    name: ResAndIndustryAlgos.PrimitiveMaterialName(rawMatInd: rawMat.Ind),
                    iconName: TextureName.PrimitiveMaterialIconName(rawMatInd: rawMat.Ind),
                    rawMatAreaPropors: new(res: rawMat, amount: 1)
                );
            var primitiveMat0 = rawMatToPrimitiveMat[indToRawMat[0]];
            var primitiveMat1 = rawMatToPrimitiveMat[indToRawMat[1]];
            MaterialPalette.CreateAndAddToResConfig
            (
                name: "other mech.",
                color: ActiveUIManager.colorConfig.otherMechMatPaletteColor,
                productClass: ProductClass.mechanical,
                materialChoices: new()
                {
                    [MaterialPurpose.mechanical] = rawMatToPrimitiveMat[indToRawMat[0]]
                }
            ).UnwrapOrThrow();

            StartingMaterialPaletteChoices = MaterialPaletteChoices.Create
            (
                choices:
                [
                    MaterialPalette.CreateAndAddToResConfig
                    (
                        name: "def. mech.",
                        color: ActiveUIManager.colorConfig.defMechMatPaletteColor,
                        productClass: ProductClass.mechanical,
                        materialChoices: new()
                        {
                            [MaterialPurpose.mechanical] = primitiveMat1
                        }
                    ).UnwrapOrThrow(),
                    MaterialPalette.CreateAndAddToResConfig
                    (
                        name: "def. elec.",
                        color: ActiveUIManager.colorConfig.defElecMatPaletteColor,
                        productClass: ProductClass.electronics,
                        materialChoices: new()
                        {
                            [MaterialPurpose.electricalConductor] = primitiveMat0,
                            [MaterialPurpose.electricalInsulator] = primitiveMat1
                        }
                    ).UnwrapOrThrow(),
                    MaterialPalette.CreateAndAddToResConfig
                    (
                        name: "def. roof",
                        color: ActiveUIManager.colorConfig.defRoofMatPaletteColor,
                        productClass: ProductClass.roof,
                        materialChoices: new()
                        {
                            [MaterialPurpose.roofSurface] = primitiveMat0
                        }
                    ).UnwrapOrThrow()
                ]
            );
            foreach (var prodParams in Product.productParamsDict.Values)
                prodParams.GetProduct(materialPalette: StartingMaterialPaletteChoices[prodParams.productClass]);
        }

        public RawMaterial GetRawMatFromInd(ulong ind)
            => indToRawMat[ind];

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
                indToRawMat.Add(key: rawMaterial.Ind, value: rawMaterial);
            if (resource is Material material && material.RawMatComposition.Count is 1)
                rawMatToPrimitiveMat.Add(key: material.RawMatComposition.First().res, material);

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
