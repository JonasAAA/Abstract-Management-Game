using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Game1.UI
{
    public class MultipleChoicePanel : IUIElement<MyRectangle>
    {
        public MyRectangle Shape
            => choicePanel.Shape;

        public Field<bool> Enabled { get; }

        private readonly UIRectPanel<ToggleButton<MyRectangle>> choicePanel;
        private readonly float choiceWidth, choiceHeight, letterHeight;
        private readonly Color selectedColor, mouseOnColor, deselectedColor;
        private ToggleButton<MyRectangle> selectedChoice;

        public MultipleChoicePanel(bool horizontal, float choiceWidth, float choiceHeight, float letterHeight, Color selectedColor, Color mouseOnColor, Color deselectedColor, Color backgroundColor)
        {
            if (horizontal)
                choicePanel = new UIRectHorizPanel<ToggleButton<MyRectangle>>(color: backgroundColor);
            else
                choicePanel = new UIRectVertPanel<ToggleButton<MyRectangle>>(color: backgroundColor);
            Shape.Color = backgroundColor;

            Enabled = new(value: true);

            this.choiceWidth = choiceWidth;
            this.choiceHeight = choiceHeight;
            this.letterHeight = letterHeight;
            this.selectedColor = selectedColor;
            this.mouseOnColor = mouseOnColor;
            this.deselectedColor = deselectedColor;
            selectedChoice = null;
        }

        IEnumerable<IUIElement> IUIElement.GetChildren()
        {
            yield return choicePanel;
        }
        
        public ToggleButton<MyRectangle> AddChoice(string choiceText, Action select)
        {
            ToggleButton<MyRectangle> choice = new
            (
                shape: new
                (
                    width: choiceWidth,
                    height: choiceHeight
                ),
                letterHeight: letterHeight,
                on: choicePanel.Count is 0,
                text: choiceText,
                mouseOnColor: mouseOnColor,
                selectedColor: selectedColor,
                deselectedColor: deselectedColor
            );

            if (choicePanel.Count is 0)
            {
                selectedChoice = choice;
                select();
            }

            choice.OnChanged += () =>
            {
                if (!choice.On)
                    return;
                
                selectedChoice.On = false;
                selectedChoice = choice;
                select();
            };

            choice.Enabled.Changed += () =>
            {
                if (choice.Enabled || !choice.On)
                    return;
                
                foreach (var choice in choicePanel)
                    if (choice.Enabled)
                    {
                        choice.On = true;
                        return;
                    }
                throw new Exception("enabled choice doesn't exist");
            };

            choicePanel.AddChild(child: choice);

            return choice;
        }
    }
}
