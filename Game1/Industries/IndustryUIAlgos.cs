﻿using Game1.Delegates;
using Game1.UI;
using static Game1.WorldManager;
using static Game1.UI.ActiveUIManager;

namespace Game1.Industries
{
    public static class IndustryUIAlgos
    {
        [Serializable]
        private sealed record ItemChoiceSetter<TItem>(IItemChoiceSetter<ProductionChoice> ProductionChoiceSetter) : IItemChoiceSetter<TItem>
            where TItem : notnull
        {
            void IItemChoiceSetter<TItem>.SetChoice(TItem item)
                => ProductionChoiceSetter.SetChoice(item: new ProductionChoice(Choice: item));
        }

        public static IItemChoiceSetter<TItem> Convert<TItem>(this IItemChoiceSetter<ProductionChoice> productionChoiceSetter)
            where TItem : notnull
            => new ItemChoiceSetter<TItem>(ProductionChoiceSetter: productionChoiceSetter);

        public static IHUDElement CreateMatPaletteChoiceDropdown(IItemChoiceSetter<MaterialPalette> matPaletteChoiceSetter, IProductClass productClass, (IHUDElement empty, Func<MaterialPalette, IHUDElement> item)? additionalInfos = null)
            => Dropdown.CreateDropdown
            (
                dropdownButtonTooltip: new ImmutableTextTooltip(text: UIAlgorithms.StartMatPaletteChoiceForProductClassTooltip(productClass: productClass)),
                itemChoiceSetter: matPaletteChoiceSetter,
                itemsWithTooltips: CurResConfig.GetMatPalettes(productClass: productClass).Select
                (
                    matPalette =>
                    (
                        item: matPalette,
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
                dropdownButtonTooltip: new ImmutableTextTooltip(text: UIAlgorithms.StartMaterialChoice),
                itemChoiceSetter: materialChoiceSetter,
                itemsWithTooltips: CurResConfig.GetCurRes<Material>().Select
                (
                    material =>
                    (
                        item: material,
                        tooltip: new ImmutableTextTooltip(text: UIAlgorithms.ChooseMaterial(material: material)) as ITooltip
                    )
                ),
                additionalInfos: additionalInfos
            );
    
        public static IHUDElement CreateResourceChoiceDropdown(IItemChoiceSetter<IResource> resChoiceSetter, (IHUDElement empty, Func<IResource, IHUDElement> item)? additionalInfos = null)
            => Dropdown.CreateDropdown
            (
                dropdownButtonTooltip: new ImmutableTextTooltip(text: UIAlgorithms.StartResourceChoiceTooltip),
                itemChoiceSetter: resChoiceSetter,
                itemsWithTooltips: CurResConfig.AllCurRes.Select
                (
                    resource =>
                    (
                        item: resource,
                        tooltip: new ImmutableTextTooltip(text: UIAlgorithms.ChooseResource(resource: resource)) as ITooltip
                    )
                ),
                additionalInfos: additionalInfos
            );

        /// <summary>
        /// If <paramref name="func"/> is null, the graph will be empty
        /// </summary>
        public static FunctionGraph<Temperature, Propor> CreateTemperatureFunctionGraph(Func<Temperature, Propor>? func)
            => new
            (
                width: curUIConfig.standardUIElementWidth,
                height: curUIConfig.UILineHeight,
                lineColor: colorConfig.functionGraphLineColor,
                backgroundColor: colorConfig.functionGraphBackgroundColor,
                lineWidth: 1,
                minX: Temperature.zero,
                maxX: CurWorldConfig.maxTemperatureShownInGraphs,
                minY: Propor.empty,
                maxY: Propor.full,
                numXSamples: 100,
                func: func
            );

        public static VertProporBar CreateStandardVertProporBar(Propor propor)
            => new
            (
                width: curUIConfig.standardUIElementWidth / 10,
                height: curUIConfig.UILineHeight,
                propor: propor,
                barColor: colorConfig.barColor,
                backgroundColor: colorConfig.barBackgroundColor
            );

        public static IEnumerable<Type> GetKnownTypes()
            => from typeArgument in Dropdown.GetKnownTypeArgs()
               select typeof(ItemChoiceSetter<>).MakeGenericType(typeArgument);
    }
}
