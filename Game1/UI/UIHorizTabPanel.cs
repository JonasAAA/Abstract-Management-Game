﻿using Game1.Delegates;
using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public class UIHorizTabPanel<TTab> : HUDElement
        where TTab : class, IHUDElement
    {
        [Serializable]
        public readonly record struct TabEnabledChangedListener(UIHorizTabPanel<TTab> UIHorizTabPanel, string TabLabelText) : IEnabledChangedListener
        {
            public void EnabledChangedResponse()
                => UIHorizTabPanel.tabChoicePanel.SetChoicePersonallyEnabled
                (
                    choiceLabel: TabLabelText,
                    newPersonallyEnabled: UIHorizTabPanel.tabs[TabLabelText].Enabled
                );
        }

        private readonly MultipleChoicePanel<string> tabChoicePanel;
        private readonly Dictionary<string, TTab> tabs;
        private readonly Dictionary<string, TabEnabledChangedListener> tabEnabledChangedListeners;
        private TTab ActiveTab
            => tabs[tabChoicePanel.SelectedChoiceLabel];

        public UIHorizTabPanel(UDouble tabLabelWidth, UDouble tabLabelHeight, Color color, Color inactiveTabLabelColor, IEnumerable<(string tabLabelText, ITooltip tabTooltip, TTab tab)> tabs)
            : base(shape: new MyRectangle(color: Color.White))
        {
            Shape.Color = color;

            this.tabs = new();
            var tabArray = tabs.ToArray();
            tabEnabledChangedListeners = new();
            foreach (var (tabLabelText, tabTooltip, tab) in tabArray)
                AddTab(tabLabelText: tabLabelText, tabTooltip: tabTooltip, tab: tab);

            tabChoicePanel = new
            (
                horizontal: true,
                choiceWidth: tabLabelWidth,
                choiceHeight: tabLabelHeight,
                selectedColor: color,
                deselectedColor: inactiveTabLabelColor,
                backgroundColor: Color.White,
                choiceLabelsAndTooltips: from tab in tabArray
                                         select (label: tab.tabLabelText, tooltip: tab.tabTooltip)
            );

            // This must be before adding tabs as children because otherwise tabChoicePanel choices are personally disabled for whatever reason
            AddChild(child: tabChoicePanel);

            foreach (var tab in this.tabs.Values)
                AddChild(tab);
        }

        protected override void PartOfRecalcSizeAndPos()
        {
            base.PartOfRecalcSizeAndPos();

            UDouble innerWidth = MyMathHelper.Max(tabChoicePanel.Shape.Width, tabs.Count switch
            {
                0 => 0,
                not 0 => tabs.Values.Max(tab => tab.Shape.Width)
            });
            UDouble tabHeight = tabs.Count switch
            {
                0 => 0,
                not 0 => tabs.Values.Max(tab => tab.Shape.Height)
            };

            Shape.Width = 2 * ActiveUIManager.RectOutlineWidth + innerWidth;
            Shape.Height = 2 * ActiveUIManager.RectOutlineWidth + tabChoicePanel.Shape.Height + tabHeight;

            tabChoicePanel.Shape.MinWidth = innerWidth;
            foreach (var tab in tabs.Values)
                tab.Shape.MinWidth = innerWidth;

            foreach (var tab in tabs.Values)
                tab.Shape.MinHeight = tabHeight;

            // recalc children positions
            tabChoicePanel.Shape.TopLeftCorner = Shape.TopLeftCorner + new MyVector2(ActiveUIManager.RectOutlineWidth);
            foreach (var tab in tabs.Values)
                tab.Shape.TopLeftCorner = Shape.TopLeftCorner + new MyVector2(ActiveUIManager.RectOutlineWidth) + new MyVector2(0, tabChoicePanel.Shape.Height);
        }

        private void AddTab(string tabLabelText, ITooltip tabTooltip, TTab tab)
        {
            tabs.Add(tabLabelText, tab);

            tabEnabledChangedListeners[tabLabelText] = new TabEnabledChangedListener(UIHorizTabPanel: this, TabLabelText: tabLabelText);

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

        public override IUIElement? CatchUIElement(MyVector2 mousePos)
        {
            if (!Shape.Contains(position: mousePos))
                return null;
            var catchingUIElement = tabChoicePanel.CatchUIElement(mousePos: mousePos);
            if (catchingUIElement is not null)
                return catchingUIElement;

            if (ActiveTab is null)
            {
                Debug.Assert(tabs.Count is 0);
                return this;
            }

            return ActiveTab.CatchUIElement(mousePos: mousePos) ?? this;
        }

        protected override void DrawChildren()
        {
            tabChoicePanel.Draw();
            ActiveTab?.Draw();
        }
    }
}
