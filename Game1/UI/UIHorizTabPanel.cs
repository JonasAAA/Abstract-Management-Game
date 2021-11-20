using Game1.Events;
using Game1.Shapes;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace Game1.UI
{
    [DataContract]
    public class UIHorizTabPanel<TTab> : HUDElement
        where TTab : class, IHUDElement
    {
        [DataContract]
        public record TabEnabledChangedListener([property: DataMember] UIHorizTabPanel<TTab> UIHorizTabPanel, [property: DataMember] string TabLabelText) : IEnabledChangedListener
        {
            public void EnabledChangedResponse()
                => UIHorizTabPanel.tabChoicePanel.SetChoicePersonallyEnabled
                (
                    choiceLabel: TabLabelText,
                    newPersonallyEnabled: UIHorizTabPanel.tabs[TabLabelText].Enabled
                );
        }

        [DataMember] private readonly MultipleChoicePanel<string> tabChoicePanel;
        [DataMember] private readonly Dictionary<string, TTab> tabs;
        [DataMember] private readonly Dictionary<string, TabEnabledChangedListener> tabEnabledChangedListeners;
        private TTab ActiveTab
            => tabChoicePanel.SelectedChoiceLabel switch
            {
                null => null,
                not null => tabs[tabChoicePanel.SelectedChoiceLabel]
            };

        public UIHorizTabPanel(float tabLabelWidth, float tabLabelHeight, Color color, Color inactiveTabLabelColor)
            : base(shape: new MyRectangle())
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
            tabEnabledChangedListeners = new();
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

            Shape.Width = 2 * ActiveUIManager.RectOutlineWidth + innerWidth;
            Shape.Height = 2 * ActiveUIManager.RectOutlineWidth + tabChoicePanel.Shape.Height + tabHeight;

            tabChoicePanel.Shape.MinWidth = innerWidth;
            foreach (var tab in tabs.Values)
                tab.Shape.MinWidth = innerWidth;
            
            foreach (var tab in tabs.Values)
                tab.Shape.MinHeight = tabHeight;

            // recalc children positions
            tabChoicePanel.Shape.TopLeftCorner = Shape.TopLeftCorner + new Vector2(ActiveUIManager.RectOutlineWidth);
            foreach (var tab in tabs.Values)
                tab.Shape.TopLeftCorner = Shape.TopLeftCorner + new Vector2(ActiveUIManager.RectOutlineWidth) + new Vector2(0, tabChoicePanel.Shape.Height);
        }

        public void AddTab(string tabLabelText, TTab tab)
        {
            tabChoicePanel.AddChoice(choiceLabel: tabLabelText);

            tabs.Add(tabLabelText, tab);

            tabEnabledChangedListeners[tabLabelText] = new TabEnabledChangedListener(UIHorizTabPanel: this, TabLabelText: tabLabelText);

            tab.EnabledChanged.Add(listener: tabEnabledChangedListeners[tabLabelText]);

            AddChild(child: tab);
        }
        
        public void ReplaceTab(string tabLabelText, TTab tab)
        {
            RemoveChild(child: tabs[tabLabelText]);
            tabs[tabLabelText].EnabledChanged.Remove(listener: tabEnabledChangedListeners[tabLabelText]);
            
            tabs[tabLabelText] = tab;
            tab.EnabledChanged.Add(listener: tabEnabledChangedListeners[tabLabelText]);
            AddChild(child: tab);
        }

        public override IUIElement CatchUIElement(Vector2 mousePos)
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

        public override void Draw()
        {
            Shape.Draw();
            tabChoicePanel.Draw();
            ActiveTab?.Draw();
        }
    }
}
