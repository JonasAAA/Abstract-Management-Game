using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Game1.UI
{
    public class UIHorizTabPanel<TTab> : UIElement<MyRectangle>
        where TTab : class, IUIElement<MyRectangle>
    {
        private readonly MultipleChoicePanel tabChoicePanel;
        private readonly Dictionary<string, TTab> tabs;
        private readonly Dictionary<string, Action> tabEnabledActions;
        private TTab activeTab;

        public UIHorizTabPanel(float tabLabelWidth, float tabLabelHeight, float letterHeight, Color color, Color inactiveTabLabelColor)
            : base(shape: new())
        {
            Shape.Color = color;
            Shape.CenterChanged += RecalcChildrenPos;

            tabChoicePanel = new
            (
                horizontal: true,
                choiceWidth: tabLabelWidth,
                choiceHeight: tabLabelHeight,
                letterHeight: letterHeight,
                selectedColor: color,
                deselectedColor: inactiveTabLabelColor,
                backgroundColor: inactiveTabLabelColor
            );
            tabs = new();
            tabEnabledActions = new();
            activeTab = null;
        }

        public void AddTab(string tabLabelText, TTab tab)
        {
            var choice = tabChoicePanel.AddChoice
            (
                choiceText: tabLabelText,
                select: () => activeTab = tab
            );

            tab.Shape.TopLeftCorner = Shape.TopLeftCorner + new Vector2(MyRectangle.outlineWidth) + new Vector2(0, tabChoicePanel.Shape.Height);
            tab.Shape.WidthChanged += RecalcWidth;
            tab.Shape.HeightChanged += RecalcHeight;

            tabs.Add(tabLabelText, tab);

            tabEnabledActions[tabLabelText] = () => choice.Enabled = tab.Enabled;

            tab.EnabledChanged += tabEnabledActions[tabLabelText];

            RecalcWidth();
            RecalcHeight();
        }
        
        public void ReplaceTab(string tabLabelText, TTab tab)
        {
            tabs[tabLabelText].Shape.WidthChanged -= RecalcWidth;
            tabs[tabLabelText].Shape.HeightChanged -= RecalcHeight;
            tabs[tabLabelText].EnabledChanged -= tabEnabledActions[tabLabelText];

            if (activeTab == tabs[tabLabelText])
                activeTab = tab;
            tabs[tabLabelText] = tab;

            tabEnabledActions[tabLabelText] = () => tabChoicePanel.GetChoice(choiceText: tabLabelText).Enabled = tab.Enabled;

            tab.Shape.TopLeftCorner = Shape.TopLeftCorner + new Vector2(MyRectangle.outlineWidth) + new Vector2(0, tabChoicePanel.Shape.Height);
            tab.Shape.WidthChanged += RecalcWidth;
            tab.Shape.HeightChanged += RecalcHeight;
            tab.EnabledChanged += tabEnabledActions[tabLabelText];

            tabChoicePanel.ReplaceChoiceAction
            (
                choiceText: tabLabelText,
                newSelect: () => activeTab = tab
            );

            RecalcWidth();
            RecalcHeight();

            //tab.Shape.TopLeftCorner = Shape.TopLeftCorner + new Vector2(MyRectangle.outlineWidth) + new Vector2(0, tabChoicePanel.Shape.Height);
            //RecalcChildrenPos();
        }

        private void RecalcChildrenPos()
        {
            tabChoicePanel.Shape.TopLeftCorner = Shape.TopLeftCorner + new Vector2(MyRectangle.outlineWidth);
            foreach (var tab in tabs.Values)
                tab.Shape.TopLeftCorner = Shape.TopLeftCorner + new Vector2(MyRectangle.outlineWidth) + new Vector2(0, tabChoicePanel.Shape.Height);
        }

        private void RecalcWidth()
        {
            float innerWidth = Math.Max(tabChoicePanel.Shape.Width, tabs.Count switch
            {
                0 => 0,
                not 0 => tabs.Values.Max(tab => tab.Shape.Width)
            });

            Shape.Width = 2 * MyRectangle.outlineWidth + innerWidth;

            tabChoicePanel.Shape.MinWidth = innerWidth;
            foreach (var tab in tabs.Values)
                tab.Shape.MinWidth = innerWidth;

            RecalcChildrenPos();
        }

        private void RecalcHeight()
        {
            float tabHeight = tabs.Count switch
            {
                0 => 0,
                not 0 => tabs.Values.Max(tab => tab.Shape.Height)
            };

            Shape.Height = 2 * MyRectangle.outlineWidth + tabChoicePanel.Shape.Height + tabHeight;
            foreach (var tab in tabs.Values)
                tab.Shape.Height = tabHeight;

            RecalcChildrenPos();
        }

        protected override IEnumerable<IUIElement> GetChildren()
            => throw new InvalidOperationException();

        public override IUIElement CatchUIElement(Vector2 mousePos)
        {
            if (!Shape.Contains(position: mousePos))
                return null;
            var catchingUIElement = tabChoicePanel.CatchUIElement(mousePos: mousePos);
            if (catchingUIElement is not null)
                return catchingUIElement;

            if (activeTab is null)
            {
                Debug.Assert(tabs.Count is 0);
                return this;
            }

            return activeTab.CatchUIElement(mousePos: mousePos) ?? this;
        }

        public override void Draw()
        {
            Shape.Draw();
            tabChoicePanel.Draw();
            activeTab?.Draw();
        }
    }
}
