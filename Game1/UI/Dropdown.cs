using Game1.Delegates;
using Game1.Shapes;
using static Game1.WorldManager;
using static Game1.GameConfig;

namespace Game1.UI
{
    public static class Dropdown
    {
        [Serializable]
        private sealed class ItemChoiceListener<TItem>(IItemChoiceSetter<TItem> itemChoiceSetter, UIRectHorizPanel<IHUDElement> startItemChoiceLine, Button<IHUDElement> startItemChoice, TItem item, IHUDElement visual, IHUDElement? additionalInfo) : IClickedListener
            where TItem : notnull
        {
            void IClickedListener.ClickedResponse()
            {
                SetStartItemChoiceLineChildren(startItemChoiceLine: startItemChoiceLine, startItemChoice: startItemChoice, visual: visual, additionalInfo: additionalInfo);
                itemChoiceSetter.SetChoice(item: item);
            }
        }

        private const VertPosEnum childVertPos = VertPosEnum.Middle;

        /// <summary>
        /// <paramref name="item"/> null means that nothing is chosen yet
        /// </summary>
        private static void SetStartItemChoiceLineChildren(UIRectHorizPanel<IHUDElement> startItemChoiceLine, Button<IHUDElement> startItemChoice, IHUDElement? visual, IHUDElement? additionalInfo)
        {
            startItemChoice.Visual = GetButtonVisual(visual: visual);
            startItemChoiceLine.Reinitialize
            (
                newChildren: new List<IHUDElement?>()
                {
                    startItemChoice,
                    additionalInfo
                }
            );
        }

        private static IHUDElement GetButtonVisual(IHUDElement? visual)
            => visual ?? new TextBox(text: "+", textColor: ActiveUIManager.colorConfig.buttonTextColor);

        // The copy is needed so that the additional info is displayed properly within dropdown AND when a choice is made
        [Serializable]
        private sealed class StartDropdownListener<TItem>(IItemChoiceSetter<TItem> ItemChoiceSetter, IEnumerable<(TItem item, IHUDElement visual, IHUDElement visualCopy, IHUDElement? additionalInfo, IHUDElement? additionalInfoCopy, ITooltip tooltip)> itemsWithTooltips, UIRectHorizPanel<IHUDElement> startItemChoiceLine, Button<IHUDElement> startItemChoice) : IClickedListener
            where TItem : notnull
        {
            void IClickedListener.ClickedResponse()
            {
                UIRectVertPanel<IHUDElement> choicePopup = new
                (
                    childHorizPos: HorizPosEnum.Middle,
                    children: itemsWithTooltips.Select
                    (
                        args =>
                        {
                            var (item, visual, visualCopy, additionalInfo, additionalInfoCopy, tooltip) = args;
                            
                            Button<IHUDElement> chooseItemButton = new
                            (
                                shape: new MyRectangle(width: CurGameConfig.wideUIElementWidth, height: CurGameConfig.UILineHeight),
                                visual: GetButtonVisual(visual: visual),
                                tooltip: tooltip
                            );
                            chooseItemButton.clicked.Add
                            (
                                listener: new ItemChoiceListener<TItem>
                                (
                                    itemChoiceSetter: ItemChoiceSetter,
                                    startItemChoiceLine: startItemChoiceLine,
                                    startItemChoice: startItemChoice,
                                    item: item,
                                    visual: visualCopy,
                                    additionalInfo: additionalInfoCopy
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
                    HUDPos: startItemChoice.Shape.GetSpecPos(origin: popupOrigin),
                    origin: popupOrigin
                );
            }
        }

        public static IHUDElement CreateDropdown<TItem>(UDouble width, ITooltip dropdownButtonTooltip, IItemChoiceSetter<TItem> itemChoiceSetter,
            IEnumerable<(TItem item, Func<IHUDElement> visual, ITooltip tooltip)> itemsWithTooltips, (IHUDElement empty, Func<TItem, IHUDElement> item)? additionalInfos)
            where TItem : class
        {
            UIRectHorizPanel<IHUDElement> startItemChoiceLine = new(childVertPos: childVertPos, children: Enumerable.Empty<IHUDElement>());
            Button<IHUDElement> startItemChoice = new
            (
                shape: new MyRectangle(width: width, height: CurGameConfig.UILineHeight),
                visual: GetButtonVisual(visual: null),
                tooltip: dropdownButtonTooltip
            );
            SetStartItemChoiceLineChildren
            (
                startItemChoiceLine: startItemChoiceLine,
                startItemChoice: startItemChoice,
                visual: null,
                additionalInfo: additionalInfos?.empty
            );
            startItemChoice.clicked.Add
            (
                listener: new StartDropdownListener<TItem>
                (
                    ItemChoiceSetter: itemChoiceSetter,
                    itemsWithTooltips: itemsWithTooltips.Select
                    (
                        args =>
                        (
                            item: args.item,
                            visual: args.visual(),
                            visualCopy: args.visual(),
                            additionalInfo: additionalInfos?.item(args.item),
                            additionalInfoCopy: additionalInfos?.item(args.item),
                            tooltip: args.tooltip
                        )
                    ),
                    startItemChoiceLine: startItemChoiceLine,
                    startItemChoice: startItemChoice
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
