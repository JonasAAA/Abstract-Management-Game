using Game1.Delegates;
using Game1.Shapes;
using static Game1.WorldManager;

namespace Game1.UI
{
    public static class Dropdown
    {
        [Serializable]
        private sealed record ItemChoiceListener<TItem>(IItemChoiceSetter<TItem> ItemChoiceSetter, Button StartItemChoice, TItem Item) : IClickedListener
            where TItem : notnull
        {
            void IClickedListener.ClickedResponse()
            {
                StartItemChoice.Text = Item.ToString();
                ItemChoiceSetter.SetChoice(item: Item);
            }
        }

        [Serializable]
        private sealed record StartDropdownListener<TItem>(IItemChoiceSetter<TItem> ItemChoiceSetter, IEnumerable<(TItem item, ITooltip tooltip)> ItemsWithTooltips, Button StartItemChoice) : IClickedListener
            where TItem : notnull
        {
            void IClickedListener.ClickedResponse()
            {
                UIRectVertPanel<IHUDElement> matPaletteChoicePopup = new(childHorizPos: HorizPosEnum.Middle);
                PosEnums popupOrigin = new(HorizPosEnum.Middle, VertPosEnum.Middle);
                CurWorldManager.SetHUDPopup
                (
                    HUDElement: matPaletteChoicePopup,
                    HUDPos: StartItemChoice.Shape.GetSpecPos(origin: popupOrigin),
                    origin: popupOrigin
                );
                foreach (var (item, tooltip) in ItemsWithTooltips)
                {
                    Button chooseItemButton = new
                    (
                        shape: new MyRectangle(width: 200, height: 30),
                        tooltip: tooltip,
                        text: item.ToString()
                    );
                    matPaletteChoicePopup.AddChild(child: chooseItemButton);
                    chooseItemButton.clicked.Add
                    (
                        listener: new ItemChoiceListener<TItem>
                        (
                            ItemChoiceSetter: ItemChoiceSetter,
                            StartItemChoice: StartItemChoice,
                            Item: item
                        )
                    );
                }
            }
        }

        public static Button CreateDropdown<TItem>(ITooltip dropdownButtonTooltip, IItemChoiceSetter<TItem> ItemChoiceSetter, IEnumerable<(TItem item, ITooltip tooltip)> ItemsWithTooltips)
            where TItem : notnull
        {
            Button startItemChoice = new
            (
                shape: new MyRectangle(width: 200, height: 30),
                tooltip: dropdownButtonTooltip,
                text: "+"
            );
            startItemChoice.clicked.Add
            (
                listener: new StartDropdownListener<TItem>
                (
                    ItemChoiceSetter: ItemChoiceSetter,
                    ItemsWithTooltips: ItemsWithTooltips,
                    StartItemChoice: startItemChoice
                )
            );

            return startItemChoice;
        }

        public static List<Type> GetKnownTypeArgs()
            => new()
            {
                typeof(IResource),
                typeof(RawMaterial),
                typeof(Material),
                typeof(Product),
                typeof(MaterialPalette)
            };

        public static IEnumerable<Type> GetKnownTypes()
            => from genericType in new List<Type>() { typeof(ItemChoiceListener<>), typeof(StartDropdownListener<>) }
               from typeArgument in GetKnownTypeArgs()
               select genericType.MakeGenericType(typeArgument);
    }
}
