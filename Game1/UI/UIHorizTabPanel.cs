using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Game1.UI
{
    public class UIHorizTabPanel<TTab> : IUIElement<MyRectangle>
        where TTab : class, IUIElement<MyRectangle>
    {
        public MyRectangle Shape { get; }
        public Field<bool> Enabled { get; }

        private readonly MultipleChoicePanel tabChoicePanel;
        private readonly List<TTab> tabs;
        private TTab activeTab;

        public UIHorizTabPanel(float tabLabelWidth, float tabLabelHeight, float letterHeight, Color color, Color inactiveTabLabelColor)
        {
            Shape = new()
            {
                Color = color
            };
            Enabled = new(value: true);
            Shape.CenterChanged += RecalcChildrenPos;

            tabChoicePanel = new
            (
                horizontal: true,
                choiceWidth: tabLabelWidth,
                choiceHeight: tabLabelHeight,
                letterHeight: letterHeight,
                selectedColor: color,
                mouseOnColor: Color.Yellow,
                deselectedColor: inactiveTabLabelColor,
                backgroundColor: inactiveTabLabelColor
            );
            tabs = new();
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

            tabs.Add(tab);

            tab.Enabled.Changed += () => choice.Enabled.Set(tab.Enabled);

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

        IEnumerable<IUIElement> IUIElement.GetChildren()
            => throw new InvalidOperationException();

        IUIElement IUIElement.CatchUIElement(Vector2 mousePos)
        {
            if (!Shape.Contains(position: mousePos))
                return null;
            var catchingUIElement = ((IUIElement)tabChoicePanel).CatchUIElement(mousePos: mousePos);
            if (catchingUIElement is not null)
                return catchingUIElement;

            if (activeTab is null)
            {
                Debug.Assert(tabs.Count is 0);
                return this;
            }

            return activeTab.CatchUIElement(mousePos: mousePos) ?? this;
        }

        void IUIElement.Draw()
        {
            Shape.Draw();
            ((IUIElement)tabChoicePanel).Draw();
            activeTab?.Draw();
        }
    }
}
