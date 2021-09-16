using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Game1.UI
{
    public class MultipleChoicePanel : UIElement<MyRectangle>
    {
        private class Choice : UIElement<MyRectangle>
        {
            public event Action Select;

            private bool selected;
            private readonly Color selectedColor, mouseOnColor, inactiveColor;
            private readonly TextBox textBox;

            public Choice(float width, float height, float letterHeight, Color selectedColor, Color mouseOnColor, Color inactiveColor, bool selected, string text)
                : base(shape: new(width: width, height: height))
            {
                this.selected = selected;
                Shape.Color = selected switch
                {
                    true => selectedColor,
                    false => inactiveColor
                };
                this.selectedColor = selectedColor;
                this.mouseOnColor = mouseOnColor;
                this.inactiveColor = inactiveColor;

                textBox = new(letterHeight: letterHeight)
                {
                    Text = text
                };
                Shape.CenterChanged += () => textBox.Shape.Center = Shape.Center;
            }

            protected override IEnumerable<UIElement> GetChildren()
            {
                yield return textBox;
            }

            public override void OnClick()
            {
                base.OnClick();
                selected = true;
                Shape.Color = selectedColor;
                Select?.Invoke();
            }

            public void Deselect()
            {
                selected = false;
                Shape.Color = inactiveColor;
            }

            public override void OnMouseEnter()
            {
                base.OnMouseEnter();
                if (!selected)
                    Shape.Color = mouseOnColor;
            }

            public override void OnMouseLeave()
            {
                base.OnMouseLeave();
                if (!selected)
                    Shape.Color = inactiveColor;
            }
        }

        public override MyRectangle Shape
            => choicePanel.Shape;

        private readonly UIRectPanel<Choice> choicePanel;
        private readonly float choiceWidth, choiceHeight, letterHeight;
        private readonly Color selectedColor, mouseOnColor, inactiveColor;
        private Choice selectedChoice;

        public MultipleChoicePanel(bool horizontal, float choiceWidth, float choiceHeight, float letterHeight, Color selectedColor, Color mouseOnColor, Color inactiveColor, Color backgroundColor)
            : base(shape: new())
        {
            if (horizontal)
                choicePanel = new UIRectHorizPanel<Choice>(color: backgroundColor);
            else
                choicePanel = new UIRectVertPanel<Choice>(color: backgroundColor);
            Shape.Color = backgroundColor;

            this.choiceWidth = choiceWidth;
            this.choiceHeight = choiceHeight;
            this.letterHeight = letterHeight;
            this.selectedColor = selectedColor;
            this.mouseOnColor = mouseOnColor;
            this.inactiveColor = inactiveColor;
            selectedChoice = null;
        }

        protected override IEnumerable<UIElement> GetChildren()
        {
            yield return choicePanel;
        }
        
        public void AddChoice(string choiceText, Action select)
        {
            Choice choice = new
            (
                width: choiceWidth,
                height: choiceHeight,
                letterHeight: letterHeight,
                selectedColor: selectedColor,
                mouseOnColor: mouseOnColor,
                inactiveColor: inactiveColor,
                selected: choicePanel.Empty,
                text: choiceText
            );

            if (choicePanel.Empty)
            {
                selectedChoice = choice;
                select();
            }

            choice.Select += () =>
            {
                selectedChoice?.Deselect();
                selectedChoice = choice;
            };

            choice.Select += select;

            choicePanel.AddChild(child: choice);
        }
    }
}
