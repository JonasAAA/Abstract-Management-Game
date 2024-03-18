using Game1.Delegates;
using Game1.UI;
using Game1.Shapes;
using static Game1.WorldManager;
using static Game1.UI.ActiveUIManager;
using static Game1.GameConfig;
using Game1.Collections;
using Game1.Industries;
using Game1.ContentNames;

namespace Game1
{
    public static class ResAndIndustryUIAlgos
    {
        [Serializable]
        private sealed class ItemChoiceSetter<TItem>(IItemChoiceSetter<ProductionChoice> productionChoiceSetter) : IItemChoiceSetter<TItem>
            where TItem : notnull
        {
            void IItemChoiceSetter<TItem>.SetChoice(TItem item)
                => productionChoiceSetter.SetChoice(item: new ProductionChoice(Choice: item));
        }

        public static IItemChoiceSetter<TItem> Convert<TItem>(this IItemChoiceSetter<ProductionChoice> productionChoiceSetter)
            where TItem : notnull
            => new ItemChoiceSetter<TItem>(productionChoiceSetter: productionChoiceSetter);

        public static IHUDElement CreateMatPaletteChoiceDropdown(IItemChoiceSetter<MaterialPalette> matPaletteChoiceSetter, ProductClass productClass, (IHUDElement empty, Func<MaterialPalette, IHUDElement> item)? additionalInfos = null)
            => Dropdown.CreateDropdown
            (
                width: CurGameConfig.iconWidth,
                dropdownButtonTooltip: new ImmutableTextTooltip(text: UIAlgorithms.StartMatPaletteChoiceForProductClassTooltip(productClass: productClass)),
                itemChoiceSetter: matPaletteChoiceSetter,
                itemsWithTooltips: CurResConfig.GetMatPalettes(productClass: productClass).Select
                (
                    matPalette =>
                    (
                        item: matPalette,
                        // Conversion is needed as otherwise Select can't infer the type of the tuple
                        visual: (Func<IHUDElement>)(() => new ImageHUDElement(image: matPalette.image)),
                        tooltip: new ImmutableTextTooltip
                        (
                            text: UIAlgorithms.ChooseMatPaletteForProductClass
                            (
                                materialPalette: matPalette,
                                productClass: productClass
                            )
                        ) as ITooltip
                    )
                ),
                additionalInfos: additionalInfos
            );

        public static IHUDElement CreateMaterialChoiceDropdown(IItemChoiceSetter<Material> materialChoiceSetter, (IHUDElement empty, Func<Material, IHUDElement> item)? additionalInfos = null)
            => Dropdown.CreateDropdown
            (
                width: CurGameConfig.iconWidth,
                dropdownButtonTooltip: new ImmutableTextTooltip(text: UIAlgorithms.StartMaterialChoice),
                itemChoiceSetter: materialChoiceSetter,
                itemsWithTooltips: CurResConfig.GetCurRes<Material>().Select
                (
                    material =>
                    (
                        item: material,
                        visual: (Func<IHUDElement>)(() => new ImageHUDElement(image: material.Icon)),
                        tooltip: new ImmutableTextTooltip(text: UIAlgorithms.ChooseMaterial(material: material)) as ITooltip
                    )
                ),
                additionalInfos: additionalInfos
            );

        public static IHUDElement CreateResourceChoiceDropdown(IItemChoiceSetter<IResource> resChoiceSetter, (IHUDElement empty, Func<IResource, IHUDElement> item)? additionalInfos = null)
            => Dropdown.CreateDropdown
            (
                width: CurGameConfig.iconWidth,
                dropdownButtonTooltip: new ImmutableTextTooltip(text: UIAlgorithms.StartResourceChoiceTooltip),
                itemChoiceSetter: resChoiceSetter,
                itemsWithTooltips: CurResConfig.AllCurRes.Select
                (
                    resource =>
                    (
                        item: resource,
                        visual: (Func<IHUDElement>)(() => new ImageHUDElement(image: resource.Icon)),
                        tooltip: new ImmutableTextTooltip(text: UIAlgorithms.ChooseResource(resource: resource)) as ITooltip
                    )
                ),
                additionalInfos: additionalInfos
            );

        /// <summary>
        /// If <paramref name="func"/> is null, the graph will be empty
        /// </summary>
        public static FunctionGraphImage<Temperature, Propor> CreateTemperatureFunctionGraph(Func<Temperature, Propor>? func)
            => new
            (
                width: CurGameConfig.standardUIElementWidth,
                height: CurGameConfig.UILineHeight,
                lineColor: colorConfig.functionGraphLineColor,
                backgroundColor: colorConfig.functionGraphBackgroundColor,
                lineWidth: 1,
                minX: Temperature.zero,
                maxX: CurWorldConfig.maxTemperatureShownInGraphs,
                minY: Propor.empty,
                maxY: Propor.full,
                numXSamples: CurGameConfig.pointNumInSmallFunctionGraphs,
                func: func
            );

        public static readonly IImage emptyProdNeededElectricityFunctionGraph = CreateGravityFunctionGraph(func: null);
        public static readonly IImage emptyProdThroughputFunctionGraph = CreateTemperatureFunctionGraph(func: null);

        /// <summary>
        /// If <paramref name="func"/> is null, the graph will be empty
        /// </summary>
        public static FunctionGraphImage<SurfaceGravity, Propor> CreateGravityFunctionGraph(Func<SurfaceGravity, Propor>? func)
            => new
            (
                width: CurGameConfig.standardUIElementWidth,
                height: CurGameConfig.UILineHeight,
                lineColor: colorConfig.functionGraphLineColor,
                backgroundColor: colorConfig.functionGraphBackgroundColor,
                lineWidth: 1,
                minX: SurfaceGravity.zero,
                maxX: CurWorldConfig.maxGravityShownInGraphs,
                minY: Propor.empty,
                maxY: Propor.full,
                numXSamples: CurGameConfig.pointNumInSmallFunctionGraphs,
                func: func
            );

        public static IHUDElement CreateNeededElectricityAndThroughputPanel(IImage neededElectricity, IImage throughput)
            => new UIRectHorizPanel<IHUDElement>
            (
                childVertPos: VertPosEnum.Top,
                children: new List<IHUDElement>()
                {
                    new ImageHUDElement(image: neededElectricity),
                    new ImageHUDElement(image: throughput)
                }
            );

        public static VertProporBar CreateStandardVertProporBar(Propor propor)
            => new
            (
                width: CurGameConfig.standardUIElementWidth / 10,
                height: CurGameConfig.UILineHeight,
                propor: propor,
                barColor: colorConfig.barColor,
                backgroundColor: colorConfig.barBackgroundColor
            );

        public static IHUDElement ResAmountsHUDElement<TRes>(ResAmounts<TRes> resAmounts)
            where TRes : class, IResource
            => ResAmountsHUDElementImpl
            (
                resAmounts: resAmounts,
                resAmountText: resAmount => $"{resAmount.BlockAmount():0.0}"
            );

        public static IHUDElement ResAmountsPercentageHUDElement<TRes>(ResAmounts<TRes> resAmounts)
            where TRes : class, IResource
        {
            var totalArea = resAmounts.Area();
            return ResAmountsHUDElementImpl
            (
                resAmounts: resAmounts,
                resAmountText: resAmount => $"{Propor.Create(resAmount.Area().valueInMetSq, totalArea.valueInMetSq)!.Value.ToPercents()}"
            );
        }

        private static IHUDElement ResAmountsHUDElementImpl<TRes>(ResAmounts<TRes> resAmounts, Func<ResAmount<TRes>, string> resAmountText)
            where TRes : class, IResource
        {
            if (resAmounts.IsEmpty)
                return new TextBox(text: "None");
            return new UIRectVertPanel<IHUDElement>
            (
                childHorizPos: HorizPosEnum.Left,
                children: resAmounts.Select
                (
                    resAmount => new UIRectHorizPanel<IHUDElement>
                    (
                        childVertPos: VertPosEnum.Middle,
                        children:
                        [
                            new ImageHUDElement(image: resAmount.res.Icon),
                            new TextBox(text: resAmountText(resAmount))
                        ]
                    )
                )
            );
        }

        private static readonly Image arrowDownImage = new(name: TextureName.arrowDown, height: CurGameConfig.smallIconHeight * 2 / 3);

        public static IHUDElement CreateIndustryFunctionVisual(this IndustryFunctionVisualParams industryFunctionVisualParams)
            => new UIRectVertPanel<IHUDElement>
            (
                childHorizPos: HorizPosEnum.Middle,
                children:
                [
                    new UIRectHorizPanel<IHUDElement>
                    (
                        childVertPos: VertPosEnum.Middle,
                        children:
                            from icon in industryFunctionVisualParams.InputIcons
                            select new ImageHUDElement(icon),
                        backgroundColor: Color.Transparent,
                        gap: 0
                    ),
                    new ImageHUDElement(image: arrowDownImage, backgroundColor: Color.Transparent),
                    new UIRectHorizPanel<IHUDElement>
                    (
                        childVertPos: VertPosEnum.Middle,
                        children:
                            from icon in industryFunctionVisualParams.OutputIcons
                            select new ImageHUDElement(icon),
                        backgroundColor: Color.Transparent,
                        gap: 0
                    )
                ],
                backgroundColor: Color.Transparent,
                gap: 0
            );

        public static IEnumerable<Type> GetKnownTypes()
            => from typeArgument in Dropdown.GetKnownTypeArgs()
               select typeof(ItemChoiceSetter<>).MakeGenericType(typeArgument);
    }
}
