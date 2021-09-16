using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Game1.UI
{
    public class UIHorizTabPanel : UIElement<MyRectangle>
    {
        private readonly MultipleChoicePanel tabChoicePanel;
        private readonly List<UIElement<MyRectangle>> tabs;
        private UIElement<MyRectangle> activeTab;

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
                mouseOnColor: Color.Lerp(color, inactiveTabLabelColor, .5f),
                inactiveColor: inactiveTabLabelColor,
                backgroundColor: inactiveTabLabelColor
            );
            tabs = new();
            activeTab = null;
        }

        public void AddTab(string tabLabelText, UIElement<MyRectangle> tab)
        {
            tabChoicePanel.AddChoice
            (
                choiceText: tabLabelText,
                select: () => activeTab = tab
            );

            tab.Shape.TopLeftCorner = Shape.TopLeftCorner + new Vector2(MyRectangle.outlineWidth) + new Vector2(0, tabChoicePanel.Shape.Height);
            tab.Shape.WidthChanged += RecalcWidth;
            tab.Shape.HeightChanged += RecalcHeight;

            tabs.Add(tab);

            RecalcWidth();
            RecalcHeight();
        }

        private void RecalcChildrenPos()
        {
            tabChoicePanel.Shape.TopLeftCorner = Shape.TopLeftCorner + new Vector2(MyRectangle.outlineWidth);
            foreach (var tab in tabs)
                tab.Shape.TopLeftCorner = Shape.TopLeftCorner + new Vector2(MyRectangle.outlineWidth) + new Vector2(0, tabChoicePanel.Shape.Height);
        }

        private void RecalcWidth()
        {
            float innerWidth = Math.Max(tabChoicePanel.Shape.Width, tabs.Count switch
            {
                0 => 0,
                not 0 => tabs.Max(tab => tab.Shape.Width)
            });

            Shape.Width = 2 * MyRectangle.outlineWidth + innerWidth;

            tabChoicePanel.Shape.MinWidth = innerWidth;
            foreach (var tab in tabs)
                tab.Shape.MinWidth = innerWidth;
        }

        private void RecalcHeight()
        {
            float tabHeight = tabs.Count switch
            {
                0 => 0,
                not 0 => tabs.Max(tab => tab.Shape.Height)
            };

            Shape.Height = 2 * MyRectangle.outlineWidth + tabChoicePanel.Shape.Height + tabHeight;
            foreach (var tab in tabs)
                tab.Shape.Height = tabHeight;
        }

        protected override IEnumerable<UIElement> GetChildren()
            => throw new InvalidOperationException();

        public override UIElement CatchUIElement(Vector2 mousePos)
        {
            if (!Contains(position: mousePos))
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
