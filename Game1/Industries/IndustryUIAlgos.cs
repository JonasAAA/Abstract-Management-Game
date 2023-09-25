using Game1.Delegates;
using Game1.UI;
using static Game1.WorldManager;

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

        public static Button CreateMatPaletteChoiceDropdown(IItemChoiceSetter<MaterialPalette> matPaletteChoiceSetter, IProductClass productClass)
            => Dropdown.CreateDropdown
            (
                dropdownButtonTooltip: new ImmutableTextTooltip(text: UIAlgorithms.StartMatPaletteChoiceForProductClassTooltip(productClass: productClass)),
                ItemChoiceSetter: matPaletteChoiceSetter,
                ItemsWithTooltips: CurResConfig.GetMatPalettes(productClass: productClass).Select
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
                )
            );

        public static Button CreateMaterialChoiceDropdown(IItemChoiceSetter<Material> materialChoiceSetter)
            => Dropdown.CreateDropdown
            (
                dropdownButtonTooltip: new ImmutableTextTooltip(text: UIAlgorithms.StartMaterialChoice),
                ItemChoiceSetter: materialChoiceSetter,
                ItemsWithTooltips: CurResConfig.GetCurRes<Material>().Select
                (
                    material =>
                    (
                        item: material,
                        tooltip: new ImmutableTextTooltip(text: UIAlgorithms.ChooseMaterial(material: material)) as ITooltip
                    )
                )
            );
    
        public static Button CreateRresourceChoiceDropdown(IItemChoiceSetter<IResource> resChoiceSetter)
            => Dropdown.CreateDropdown
            (
                dropdownButtonTooltip: new ImmutableTextTooltip(text: UIAlgorithms.StartResourceChoiceTooltip),
                ItemChoiceSetter: resChoiceSetter,
                ItemsWithTooltips: CurResConfig.AllCurRes.Select
                (
                    resource =>
                    (
                        item: resource,
                        tooltip: new ImmutableTextTooltip(text: UIAlgorithms.ChooseResource(resource: resource)) as ITooltip
                    )
                )
            );
    }
}
