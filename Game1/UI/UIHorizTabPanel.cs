﻿using Game1.Delegates;
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
        public sealed class TabEnabledChangedListener(UIHorizTabPanel<TTab> UIHorizTabPanel, string tabLabelText) : IEnabledChangedListener
        {
            void IEnabledChangedListener.EnabledChangedResponse()
                => UIHorizTabPanel.tabChoicePanel.SetChoicePersonallyEnabled
                (
                    choiceLabel: tabLabelText,
                    newPersonallyEnabled: UIHorizTabPanel.tabs[tabLabelText].Enabled
                );
        }

        protected sealed override Color Color
            => colorConfig.UIBackgroundColor;

        private readonly MultipleChoicePanel<string> tabChoicePanel;
        private readonly Dictionary<string, TTab> tabs;
        private readonly Dictionary<string, TabEnabledChangedListener> tabEnabledChangedListeners;
        private TTab ActiveTab
            => tabs[tabChoicePanel.SelectedChoiceLabel];

        public UIHorizTabPanel(UDouble tabLabelWidth, UDouble tabLabelHeight, IEnumerable<(string tabLabelText, ITooltip tabTooltip, TTab tab)> tabs)
            : base(shape: new MyRectangle())
        {
            this.tabs = [];
            var tabArray = tabs.ToArray();
            tabEnabledChangedListeners = [];
            foreach (var (tabLabelText, tabTooltip, tab) in tabArray)
                AddTab(tabLabelText: tabLabelText, tabTooltip: tabTooltip, tab: tab);

            tabChoicePanel = new
            (
                horizontal: true,
                choiceWidth: tabLabelWidth,
                choiceHeight: tabLabelHeight,
                choiceLabelsAndTooltips: from tab in tabArray
                                         select (label: tab.tabLabelText, tooltip: tab.tabTooltip)
            );

            // This must be before adding tabs as children because otherwise tabChoicePanel choices are personally disabled for whatever reason
            AddChild(child: tabChoicePanel);

            foreach (var tab in this.tabs.Values)
                AddChild(tab);
        }

        protected sealed override void PartOfRecalcSizeAndPos()
        {
            base.PartOfRecalcSizeAndPos();

            UDouble innerWidth = MyMathHelper.Max
            (
                left: tabChoicePanel.Shape.Width,
                right: tabs.Values.MaxOrDefault(tab => tab.Shape.Width)
            );
            UDouble tabHeight = tabs.Values.MaxOrDefault(tab => tab.Shape.Height);

            Shape.Width = 2 * CurGameConfig.rectOutlineWidth + innerWidth;
            Shape.Height = 2 * CurGameConfig.rectOutlineWidth + tabChoicePanel.Shape.Height + tabHeight;

            tabChoicePanel.Shape.MinWidth = innerWidth;
            foreach (var tab in tabs.Values)
                tab.Shape.MinWidth = innerWidth;

            foreach (var tab in tabs.Values)
                tab.Shape.MinHeight = tabHeight;

            // recalc children positions
            tabChoicePanel.Shape.TopLeftCorner = Shape.TopLeftCorner + new Vector2Bare(CurGameConfig.rectOutlineWidth);
            foreach (var tab in tabs.Values)
                tab.Shape.TopLeftCorner = Shape.TopLeftCorner + new Vector2Bare(CurGameConfig.rectOutlineWidth) + new Vector2Bare(0, tabChoicePanel.Shape.Height);
        }

        private void AddTab(string tabLabelText, ITooltip tabTooltip, TTab tab)
        {
            tabs.Add(tabLabelText, tab);

            tabEnabledChangedListeners[tabLabelText] = new TabEnabledChangedListener(UIHorizTabPanel: this, tabLabelText: tabLabelText);

            tab.EnabledChanged.Add(listener: tabEnabledChangedListeners[tabLabelText]);

            if (tabChoicePanel is not null)
            {
                tabChoicePanel.AddChoice(choiceLabel: tabLabelText, choiceTooltip: tabTooltip);
                AddChild(child: tab);
            }
        }

        public void ReplaceTab(string tabLabelText, TTab tab)
        {
            RemoveChild(child: tabs[tabLabelText]);
            tabs[tabLabelText].EnabledChanged.Remove(listener: tabEnabledChangedListeners[tabLabelText]);

            tabs[tabLabelText] = tab;
            tab.EnabledChanged.Add(listener: tabEnabledChangedListeners[tabLabelText]);
            AddChild(child: tab);
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
                Debug.Assert(tabs.Count is 0);
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
