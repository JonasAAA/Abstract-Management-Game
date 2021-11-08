﻿using Game1.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace Game1.UI
{
    public class UIHorizTabPanel<TTab> : HUDElement/*<MyRectangle>*/
        where TTab : class, IHUDElement/*<MyRectangle>*/
    {
        private readonly MultipleChoicePanel<string> tabChoicePanel;
        private readonly Dictionary<string, TTab> tabs;
        //private readonly Dictionary<string, Action> tabEnabledActions;
        private readonly Dictionary<string, ThisAsEnabledChangedListener> thisAsEnabledChangedListenersByTabLabelText;
        //private TTab activeTab;
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
            thisAsEnabledChangedListenersByTabLabelText = new();
            //activeTab = null;
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
            /*var choice = */tabChoicePanel.AddChoice
            (
                choiceLabel: tabLabelText
                //select: () => activeTab = tab
            );

            tabs.Add(tabLabelText, tab);

            thisAsEnabledChangedListenersByTabLabelText[tabLabelText] = new ThisAsEnabledChangedListener(UIHorizTabPanel: this, TabLabelText: tabLabelText);

            //tabEnabledActions[tabLabelText] = () => choice.PersonallyEnabled = tab.Enabled;

            tab.EnabledChanged.Add(listener: thisAsEnabledChangedListenersByTabLabelText[tabLabelText]);
            //tab.EnabledChanged += tabEnabledActions[tabLabelText];

            AddChild(child: tab);
        }
        
        public void ReplaceTab(string tabLabelText, TTab tab)
        {
            RemoveChild(child: tabs[tabLabelText]);
            tabs[tabLabelText].EnabledChanged.Remove(listener: thisAsEnabledChangedListenersByTabLabelText[tabLabelText]);
            //tabs[tabLabelText].EnabledChanged -= tabEnabledActions[tabLabelText];

            //if (activeTab == tabs[tabLabelText])
            //    activeTab = tab;
            tabs[tabLabelText] = tab;

            //tabEnabledActions[tabLabelText] = () => tabChoicePanel.GetChoice(choiceText: tabLabelText).PersonallyEnabled = tab.Enabled;

            tab.EnabledChanged.Add(listener: thisAsEnabledChangedListenersByTabLabelText[tabLabelText]);
            //tab.EnabledChanged += tabEnabledActions[tabLabelText];

            //tabChoicePanel.ReplaceChoiceAction
            //(
            //    choiceText: tabLabelText,
            //    newSelect: () => activeTab = tab
            //);

            AddChild(child: tab);
        }

        [DataContract]
        private record ThisAsEnabledChangedListener([property:DataMember] UIHorizTabPanel<TTab> UIHorizTabPanel, [property: DataMember] string TabLabelText) : IEnabledChangedListener
        {
            public void EnabledChangedResponse(IUIElement UIElement)
                => UIHorizTabPanel.tabChoicePanel.SetChoicePersonallyEnabled(choiceLabel: TabLabelText, newPersonallyEnabled: UIElement.Enabled);
                //=> UIHorizTabPanel.tabChoicePanel.GetChoice(choiceLabel: TabLabelText).PersonallyEnabled = UIElement.Enabled;
        }

        public override IUIElement CatchUIElement(Vector2 mousePos)
        {
            if (!Shape.Contains(position: mousePos))
                return null;
            var catchingUIElement = tabChoicePanel.CatchUIElement(mousePos: mousePos);
            if (catchingUIElement is not null)
                return catchingUIElement;

            //if (activeTab is null)
            if (ActiveTab is null)
            {
                Debug.Assert(tabs.Count is 0);
                return this;
            }

            return ActiveTab.CatchUIElement(mousePos: mousePos) ?? this;
            //return activeTab.CatchUIElement(mousePos: mousePos) ?? this;
        }

        public override void Draw()
        {
            Shape.Draw();
            tabChoicePanel.Draw();
            ActiveTab?.Draw();
            //activeTab?.Draw();
        }
    }
}
