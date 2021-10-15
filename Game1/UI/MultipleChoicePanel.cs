using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Game1.UI
{
    public class MultipleChoicePanel : HUDElement<MyRectangle>
    {
        private readonly UIRectPanel<SelectButton<MyRectangle>> choicePanel;
        private readonly Dictionary<string, SelectButton<MyRectangle>> choices;
        private readonly Dictionary<string, Action> choiceActions;
        private readonly float choiceWidth, choiceHeight;
        private readonly Color selectedColor, deselectedColor;
        private SelectButton<MyRectangle> selectedChoice;

        public MultipleChoicePanel(bool horizontal, float choiceWidth, float choiceHeight, Color selectedColor, Color deselectedColor, Color backgroundColor)
            : base(shape: new MyRectangle())
        {
            choicePanel = horizontal switch
            {
                true => new UIRectHorizPanel<SelectButton<MyRectangle>>
                (
                    color: backgroundColor,
                    childVertPos: VertPos.Top
                ),
                false => new UIRectVertPanel<SelectButton<MyRectangle>>
                (
                    color: backgroundColor,
                    childHorizPos: HorizPos.Left
                )
            };
            
            Shape.Color = backgroundColor;
            choices = new();
            choiceActions = new();

            this.choiceWidth = choiceWidth;
            this.choiceHeight = choiceHeight;
            this.selectedColor = selectedColor;
            this.deselectedColor = deselectedColor;
            selectedChoice = null;

            AddChild(child: choicePanel);
        }

        protected override void PartOfRecalcSizeAndPos()
        {
            base.PartOfRecalcSizeAndPos();

            Shape.Width = choicePanel.Shape.Width;
            Shape.Height = choicePanel.Shape.Height;
            choicePanel.Shape.Center = Shape.Center;
        }

        public SelectButton<MyRectangle> AddChoice(string choiceText, Action select)
        {
            SelectButton<MyRectangle> choice = new
            (
                shape: new
                (
                    width: choiceWidth,
                    height: choiceHeight
                ),
                on: choicePanel.Count is 0,
                text: choiceText,
                selectedColor: selectedColor,
                deselectedColor: deselectedColor
            );

            if (choicePanel.Count is 0)
            {
                selectedChoice = choice;
                select();
            }

            void choiceAction()
            {
                if (!choice.On)
                    return;

                selectedChoice.On = false;
                selectedChoice = choice;
                select();
            }

            choice.OnChanged += choiceAction;

            choice.EnabledChanged += () =>
            {
                if (choice.PersonallyEnabled || !choice.On)
                    return;
                
                foreach (var choice in choicePanel)
                    if (choice.PersonallyEnabled)
                    {
                        choice.On = true;
                        return;
                    }
                throw new Exception("enabled choice doesn't exist");
            };

            choices.Add(choiceText, choice);
            choiceActions.Add(choiceText, choiceAction);
            choicePanel.AddChild(child: choice);

            return choice;
        }
    
        public void ReplaceChoiceAction(string choiceText, Action newSelect)
        {
            var choice = choices[choiceText];
            choice.OnChanged -= choiceActions[choiceText];

            if (selectedChoice == choice)
                newSelect();

            void choiceAction()
            {
                if (!choice.On)
                    return;

                selectedChoice.On = false;
                selectedChoice = choice;
                newSelect();
            }

            choice.OnChanged += choiceAction;
            choiceActions[choiceText] = choiceAction;
        }

        public SelectButton<MyRectangle> GetChoice(string choiceText)
            => choices[choiceText];
    }
}
