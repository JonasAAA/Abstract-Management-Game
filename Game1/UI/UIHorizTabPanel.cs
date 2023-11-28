using Game1.Delegates;
using Game1.Shapes;
using static Game1.UI.ActiveUIManager;
using static Game1.GameConfig;

namespace Game1.UI
{
    [Serializable]
    public sealed class UIHorizTabPanel<TTab> : HUDElement
        where TTab : class, IHUDElement
    {
        [Serializable]
        public sealed class TabEnabledChangedListener(UIHorizTabPanel<TTab> UIHorizTabPanel, TTab tab) : IEnabledChangedListener
        {
            void IEnabledChangedListener.EnabledChangedResponse()
                => UIHorizTabPanel.tabChoicePanel.SetChoicePersonallyEnabled
                (
                    choiceLabel: tab,
                    newPersonallyEnabled: tab.Enabled
                );
        }

        protected sealed override Color Color
            => colorConfig.UIBackgroundColor;

        private IReadOnlyCollection<TTab> Tabs
            => tabEnabledChangedListeners.Keys;
        private readonly Dictionary<TTab, TabEnabledChangedListener> tabEnabledChangedListeners;
        private readonly MultipleChoicePanel<TTab> tabChoicePanel;
        private TTab ActiveTab
            => tabChoicePanel.SelectedChoiceLabel;

        public UIHorizTabPanel(UDouble tabLabelWidth, UDouble tabLabelHeight, IEnumerable<(TTab tab, IHUDElement tabLabelVisual, ITooltip tabTooltip)> tabs)
            : base(shape: new MyRectangle())
        {
            var tabArray = tabs.ToArray();
            tabEnabledChangedListeners = [];
            foreach (var (tab, tabLabelVisual, tabTooltip) in tabArray)
                AddTab(tabLabelVisual: tabLabelVisual, tab: tab, tabTooltip: tabTooltip);

            tabChoicePanel = new
            (
                horizontal: true,
                choiceWidth: tabLabelWidth,
                choiceHeight: tabLabelHeight,
                choiceLabelsAndTooltips:
                    from tab in tabArray
                    select
                    (
                        label: tab.tab,
                        visual: tab.tabLabelVisual,
                        tooltip: tab.tabTooltip
                    )
            );

            // This must be before adding tabs as children because otherwise tabChoicePanel choices are personally disabled for whatever reason
            AddChild(child: tabChoicePanel);

            foreach (var tab in Tabs)
                AddChild(tab);
        }

        protected sealed override void PartOfRecalcSizeAndPos()
        {
            base.PartOfRecalcSizeAndPos();

            UDouble innerWidth = MyMathHelper.Max
            (
                left: tabChoicePanel.Shape.Width,
                right: Tabs.MaxOrDefault(tab => tab.Shape.Width)
            );
            UDouble tabHeight = Tabs.MaxOrDefault(tab => tab.Shape.Height);

            Shape.Width = 2 * CurGameConfig.rectOutlineWidth + innerWidth;
            Shape.Height = 2 * CurGameConfig.rectOutlineWidth + tabChoicePanel.Shape.Height + tabHeight;

            tabChoicePanel.Shape.MinWidth = innerWidth;
            foreach (var tab in Tabs)
                tab.Shape.MinWidth = innerWidth;

            foreach (var tab in Tabs)
                tab.Shape.MinHeight = tabHeight;

            // recalc children positions
            tabChoicePanel.Shape.TopLeftCorner = Shape.TopLeftCorner + new Vector2Bare(CurGameConfig.rectOutlineWidth);
            foreach (var tab in Tabs)
                tab.Shape.TopLeftCorner = Shape.TopLeftCorner + new Vector2Bare(CurGameConfig.rectOutlineWidth) + new Vector2Bare(0, tabChoicePanel.Shape.Height);
        }

        private void AddTab(TTab tab, IHUDElement tabLabelVisual, ITooltip tabTooltip)
        {
            tabEnabledChangedListeners.Add(key: tab, value: new TabEnabledChangedListener(UIHorizTabPanel: this, tab: tab));

            tab.EnabledChanged.Add(listener: tabEnabledChangedListeners[tab]);

            if (tabChoicePanel is not null)
            {
                tabChoicePanel.AddChoice(choiceLabel: tab, choiceVisual: tabLabelVisual, choiceTooltip: tabTooltip);
                AddChild(child: tab);
            }
        }

        public sealed override IUIElement? CatchUIElement(Vector2Bare mouseScreenPos)
        {
            if (!Shape.Contains(screenPos: mouseScreenPos))
                return null;
            var catchingUIElement = tabChoicePanel.CatchUIElement(mouseScreenPos: mouseScreenPos);
            if (catchingUIElement is not null)
                return catchingUIElement;

            if (ActiveTab is null)
            {
                Debug.Assert(Tabs.Count is 0);
                return this;
            }

            return ActiveTab.CatchUIElement(mouseScreenPos: mouseScreenPos) ?? this;
        }

        protected sealed override void DrawChildren()
        {
            tabChoicePanel.Draw();
            ActiveTab?.Draw();
        }
    }
}
