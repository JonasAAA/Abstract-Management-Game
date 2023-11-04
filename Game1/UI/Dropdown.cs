using Game1.Delegates;
using Game1.Shapes;
using static Game1.WorldManager;
using static Game1.GameConfig;

namespace Game1.UI
{
    public static class Dropdown
    {
        [Serializable]
        private sealed record ItemChoiceListener<TItem>(IItemChoiceSetter<TItem> ItemChoiceSetter, UIRectHorizPanel<IHUDElement> StartItemChoiceLine, Button StartItemChoice, TItem Item, IHUDElement? AdditionalInfo) : IClickedListener
            where TItem : notnull
        {
            void IClickedListener.ClickedResponse()
            {
                SetStartItemChoiceLineChildren(startItemChoiceLine: StartItemChoiceLine, startItemChoice: StartItemChoice, item: Item, additionalInfo: AdditionalInfo);
                ItemChoiceSetter.SetChoice(item: Item);
            }
        }

        private const VertPosEnum childVertPos = VertPosEnum.Middle;

        /// <summary>
        /// <paramref name="item"/> null means that nothing is chosen yet
        /// </summary>
        private static void SetStartItemChoiceLineChildren<TItem>(UIRectHorizPanel<IHUDElement> startItemChoiceLine, Button startItemChoice, TItem? item, IHUDElement? additionalInfo)
        {
            startItemChoice.Text = GetButtonText(item: item);
            startItemChoiceLine.Reinitialize
            (
                newChildren: new List<IHUDElement?>()
                {
                    startItemChoice,
                    additionalInfo
                }
            );
        }

        private static string GetButtonText<TItem>(TItem? item)
            => item?.ToString() ?? "+";

        // The copy is needed so that the additional info is displayed properly within dropdown AND when a choice is made
        [Serializable]
        private sealed record StartDropdownListener<TItem>(IItemChoiceSetter<TItem> ItemChoiceSetter, IEnumerable<(TItem item, IHUDElement? additionalInfo, IHUDElement? additionalInfoCopy, ITooltip tooltip)> ItemsWithTooltips, UIRectHorizPanel<IHUDElement> StartItemChoiceLine, Button StartItemChoice) : IClickedListener
            where TItem : notnull
        {
            void IClickedListener.ClickedResponse()
            {
                UIRectVertPanel<IHUDElement> choicePopup = new
                (
                    childHorizPos: HorizPosEnum.Middle,
                    children: ItemsWithTooltips.Select
                    (
                        args =>
                        {
                            var (item, additionalInfo, additionalInfoCopy, tooltip) = args;
                            
                            Button chooseItemButton = new
                            (
                                shape: new MyRectangle(width: CurGameConfig.wideUIElementWidth, height: CurGameConfig.UILineHeight),
                                tooltip: tooltip,
                                text: GetButtonText(item: item)
                            );
                            chooseItemButton.clicked.Add
                            (
                                listener: new ItemChoiceListener<TItem>
                                (
                                    ItemChoiceSetter: ItemChoiceSetter,
                                    StartItemChoiceLine: StartItemChoiceLine,
                                    StartItemChoice: StartItemChoice,
                                    Item: item,
                                    AdditionalInfo: additionalInfoCopy
                                )
                            );
                            return new UIRectHorizPanel<IHUDElement>
                            (
                                childVertPos: childVertPos,
                                children: new List<IHUDElement?>() { chooseItemButton, additionalInfo }
                            );
                        }
                    )
                );
                PosEnums popupOrigin = new(HorizPosEnum.Left, VertPosEnum.Middle);
                CurWorldManager.SetHUDPopup
                (
                    HUDElement: choicePopup,
                    HUDPos: StartItemChoice.Shape.GetSpecPos(origin: popupOrigin),
                    origin: popupOrigin
                );
            }
        }

        public static IHUDElement CreateDropdown<TItem>(ITooltip dropdownButtonTooltip, IItemChoiceSetter<TItem> itemChoiceSetter,
            IEnumerable<(TItem item, ITooltip tooltip)> itemsWithTooltips, (IHUDElement empty, Func<TItem, IHUDElement> item)? additionalInfos)
            where TItem : class
        {
            UIRectHorizPanel<IHUDElement> startItemChoiceLine = new(childVertPos: childVertPos, children: Enumerable.Empty<IHUDElement>());
            Button startItemChoice = new
            (
                shape: new MyRectangle(width: CurGameConfig.wideUIElementWidth, height: CurGameConfig.UILineHeight),
                tooltip: dropdownButtonTooltip
            );
            SetStartItemChoiceLineChildren<TItem>
            (
                startItemChoiceLine: startItemChoiceLine,
                startItemChoice: startItemChoice,
                item: null,
                additionalInfo: additionalInfos?.empty
            );
            startItemChoice.clicked.Add
            (
                listener: new StartDropdownListener<TItem>
                (
                    ItemChoiceSetter: itemChoiceSetter,
                    ItemsWithTooltips: itemsWithTooltips.Select
                    (
                        args =>
                        (
                            item: args.item,
                            additionalInfo: additionalInfos?.item(args.item),
                            additionalInfoCopy: additionalInfos?.item(args.item),
                            tooltip: args.tooltip
                        )
                    ),
                    StartItemChoiceLine: startItemChoiceLine,
                    StartItemChoice: startItemChoice
                )
            );

            return startItemChoiceLine;
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
