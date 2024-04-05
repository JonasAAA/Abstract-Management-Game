using Game1.Collections;
using Game1.ContentNames;
using Game1.GlobalTypes;
using Game1.UI;

namespace Game1.Resources
{
    [Serializable]
    public sealed class ResConfig
    {
        public MaterialPaletteChoices StartingMaterialPaletteChoices { get; private set; }

        public SortedResSet<IResource> AllCurRes
            => SortedResSet<IResource>.FromSortedUniqueResListUnsafe(sortedUniqueResList: resources.ToEfficientReadOnlyCollection());

        private readonly EnumDict<RawMaterialID, RawMaterial> IDToRawMat;
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
            IDToRawMat = new(selector: RawMaterial.Create);
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
            foreach (var rawMat in IDToRawMat.Values)
                AddRes(rawMat);

            foreach (var rawMat in IDToRawMat.Values)
                Material.CreateAndAddToCurResConfig
                (
                    name: ResAndIndustryAlgos.PrimitiveMaterialName(rawMatID: rawMat.RawMatID),
                    iconName: TextureName.PrimitiveMaterialIconName(rawMatID: rawMat.RawMatID),
                    rawMatAreaPropors: new(res: rawMat, amount: 1)
                );
            var primitiveMat0 = rawMatToPrimitiveMat[IDToRawMat[RawMaterialID.Firstium]];
            var primitiveMat1 = rawMatToPrimitiveMat[IDToRawMat[RawMaterialID.Secondium]];
            var primitiveMat2 = rawMatToPrimitiveMat[IDToRawMat[RawMaterialID.Thirdium]];
            var primitiveMat3 = rawMatToPrimitiveMat[IDToRawMat[RawMaterialID.Fourthium]];
            
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
                    ).UnwrapOrThrow()
                ]
            );

            MaterialPalette.CreateAndAddToResConfig
            (
                name: "other mech.",
                color: ActiveUIManager.colorConfig.otherMechMatPaletteColor,
                productClass: ProductClass.mechanical,
                materialChoices: new()
                {
                    [MaterialPurpose.mechanical] = primitiveMat3
                }
            ).UnwrapOrThrow();

            MaterialPalette.CreateAndAddToResConfig
            (
                name: "other elec.",
                color: ActiveUIManager.colorConfig.otherElecMatPaletteColor,
                productClass: ProductClass.electronics,
                materialChoices: new()
                {
                    [MaterialPurpose.electricalConductor] = primitiveMat2,
                    [MaterialPurpose.electricalInsulator] = primitiveMat3
                }
            ).UnwrapOrThrow();

            foreach (var prodParams in Product.productParamsDict.Values)
                prodParams.GetProduct(materialPalette: StartingMaterialPaletteChoices[prodParams.productClass]);
        }

        public RawMaterial GetRawMatFromID(RawMaterialID rawMatID)
            => IDToRawMat[rawMatID];

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
                Debug.Assert(IDToRawMat[rawMaterial.RawMatID] == rawMaterial);
                return;
            }
            if (resource is Material material && material.RawMatComposition.Count is 1)
                rawMatToPrimitiveMat.Add(key: material.RawMatComposition.First().res, material);

            ulong GetOrder(IResource resource)
                => resource switch
                {
                    RawMaterial rawMaterial => rawMatOrderOffset + rawMaterial.RawMatID.Ind(),
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
            return new(ok: UnitType.value);
        }

        public EfficientReadOnlyCollection<MaterialPalette> GetMatPalettes(ProductClass productClass)
            => new(list: materialPalettes[productClass]);
    }
}
