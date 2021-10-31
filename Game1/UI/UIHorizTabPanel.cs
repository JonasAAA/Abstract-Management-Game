using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Game1.UI
{
    public class UIHorizTabPanel<TTab> : HUDElement<MyRectangle>
        where TTab : class, IHUDElement<MyRectangle>
    {
        private readonly MultipleChoicePanel tabChoicePanel;
        private readonly Dictionary<string, TTab> tabs;
        private readonly Dictionary<string, Action> tabEnabledActions;
        private TTab activeTab;

        public UIHorizTabPanel(float tabLabelWidth, float tabLabelHeight, Color color, Color inactiveTabLabelColor)
            : base(shape: new())
        {
            Shape.Color = color;

            tabChoicePanel = new
            (
                horizontal: true,
                choiceWidth: tabLabelWidth,
                choiceHeight: tabLabelHeight,
                selectedColor: color,
                deselectedColor: inactiveTabLabelColor,
                backgroundColor: inactiveTabLabelColor
            );
            tabs = new();
            tabEnabledActions = new();
            activeTab = null;
            AddChild(child: tabChoicePanel);
        }

        protected override void PartOfRecalcSizeAndPos()
        {
            base.PartOfRecalcSizeAndPos();

            float innerWidth = Math.Max(tabChoicePanel.Shape.Width, tabs.Count switch
            {
                0 => 0,
                not 0 => tabs.Values.Max(tab => tab.Shape.Width)
            });
            float tabHeight = tabs.Count switch
            {
                0 => 0,
                not 0 => tabs.Values.Max(tab => tab.Shape.Height)
            };

            Shape.Width = 2 * ActiveUIManager.UIConfig.rectOutlineWidth + innerWidth;
            Shape.Height = 2 * ActiveUIManager.UIConfig.rectOutlineWidth + tabChoicePanel.Shape.Height + tabHeight;

            tabChoicePanel.Shape.MinWidth = innerWidth;
            foreach (var tab in tabs.Values)
                tab.Shape.MinWidth = innerWidth;
            
            foreach (var tab in tabs.Values)
                tab.Shape.MinHeight = tabHeight;

            // recalc children positions
            tabChoicePanel.Shape.TopLeftCorner = Shape.TopLeftCorner + new Vector2(ActiveUIManager.UIConfig.rectOutlineWidth);
            foreach (var tab in tabs.Values)
                tab.Shape.TopLeftCorner = Shape.TopLeftCorner + new Vector2(ActiveUIManager.UIConfig.rectOutlineWidth) + new Vector2(0, tabChoicePanel.Shape.Height);
        }

        public void AddTab(string tabLabelText, TTab tab)
        {
            var choice = tabChoicePanel.AddChoice
            (
                choiceText: tabLabelText,
                select: () => activeTab = tab
            );

            tabs.Add(tabLabelText, tab);

            tabEnabledActions[tabLabelText] = () => choice.PersonallyEnabled = tab.Enabled;

            tab.EnabledChanged += tabEnabledActions[tabLabelText];

            AddChild(child: tab);
        }
        
        public void ReplaceTab(string tabLabelText, TTab tab)
        {
            RemoveChild(child: tabs[tabLabelText]);
            tabs[tabLabelText].EnabledChanged -= tabEnabledActions[tabLabelText];

            if (activeTab == tabs[tabLabelText])
                activeTab = tab;
            tabs[tabLabelText] = tab;
            
            tabEnabledActions[tabLabelText] = () => tabChoicePanel.GetChoice(choiceText: tabLabelText).PersonallyEnabled = tab.Enabled;

            tab.EnabledChanged += tabEnabledActions[tabLabelText];

            tabChoicePanel.ReplaceChoiceAction
            (
                choiceText: tabLabelText,
                newSelect: () => activeTab = tab
            );

            AddChild(child: tab);
        }

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
