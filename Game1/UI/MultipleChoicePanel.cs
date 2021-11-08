using Game1.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game1.UI
{
    public class MultipleChoicePanel<TChoice> : HUDElement/*<MyRectangle>*/, IEnabledChangedListener
    {
        [DataContract]
        private record ChoiceOnChangedListener([property: DataMember] MultipleChoicePanel<TChoice> MultipleChoicePanel, TChoice ChoiceLabel) : IOnChangedListener
        {
            void IOnChangedListener.OnChangedResponse()
            {
                if (!MultipleChoicePanel.choices[ChoiceLabel].On)
                    return;

                MultipleChoicePanel.choices[MultipleChoicePanel.SelectedChoiceLabel].On = false;
                MultipleChoicePanel.SelectedChoiceLabel = ChoiceLabel;
            }
        }

        [DataMember]
        public readonly Event<IChoiceChangedListener<TChoice>> choiceChanged;

        public TChoice SelectedChoiceLabel
        {
            get => selectedChoiceLabel;
            private set
            {
                if (!C.Equals(selectedChoiceLabel, value))
                {
                    var prevSelectedChoiceLabel = selectedChoiceLabel;
                    selectedChoiceLabel = value;
                    choiceChanged.Raise(action: listener => listener.ChoiceChangedResponse(prevChoice: prevSelectedChoiceLabel));
                }
            }
        }

        private readonly UIRectPanel<SelectButton/*<MyRectangle>*/> choicePanel;
        private readonly Dictionary<TChoice, SelectButton/*<MyRectangle>*/> choices;
        //private readonly Dictionary<string, IOnChangedListener> choiceOnChangedListeners;
        //private readonly Dictionary<string, Action> choiceActions;
        private readonly float choiceWidth, choiceHeight;
        private readonly Color selectedColor, deselectedColor;
        private TChoice selectedChoiceLabel;
        //private SelectButton/*<MyRectangle>*/ selectedChoice;

        public MultipleChoicePanel(bool horizontal, float choiceWidth, float choiceHeight, Color selectedColor, Color deselectedColor, Color backgroundColor)
            : base(shape: new MyRectangle())
        {
            choiceChanged = new();
            choicePanel = horizontal switch
            {
                true => new UIRectHorizPanel<SelectButton/*<MyRectangle>*/>
                (
                    color: backgroundColor,
                    childVertPos: VertPos.Top
                ),
                false => new UIRectVertPanel<SelectButton/*<MyRectangle>*/>
                (
                    color: backgroundColor,
                    childHorizPos: HorizPos.Left
                )
            };
            
            Shape.Color = backgroundColor;
            selectedChoiceLabel = default;
            choices = new();
            //choiceOnChangedListeners = new();
            //choiceActions = new();

            this.choiceWidth = choiceWidth;
            this.choiceHeight = choiceHeight;
            this.selectedColor = selectedColor;
            this.deselectedColor = deselectedColor;
            //selectedChoice = null;

            AddChild(child: choicePanel);
        }

        protected override void PartOfRecalcSizeAndPos()
        {
            base.PartOfRecalcSizeAndPos();

            Shape.Width = choicePanel.Shape.Width;
            Shape.Height = choicePanel.Shape.Height;
            choicePanel.Shape.Center = Shape.Center;
        }

        public /*SelectButton<MyRectangle>*/ void AddChoice(TChoice choiceLabel /*Action select*/)
        {
            if (choices.ContainsKey(choiceLabel))
                throw new ArgumentException();

            SelectButton/*<MyRectangle>*/ choice = new
            (
                shape: new MyRectangle
                (
                    width: choiceWidth,
                    height: choiceHeight
                ),
                on: choicePanel.Count is 0,
                text: choiceLabel.ToString(),
                selectedColor: selectedColor,
                deselectedColor: deselectedColor
            );

            if (choicePanel.Count is 0)
            {
                //selectedChoice = choice;
                SelectedChoiceLabel = choiceLabel;
                //select();
            }

            //void choiceAction()
            //{
            //    if (!choice.On)
            //        return;

            //    choices[SelectedChoiceText].On = false;
            //    //selectedChoice.On = false;
            //    //selectedChoice = choice;
            //    SelectedChoiceText = choiceText;
            //    //select();
            //}

            choice.onChanged.Add
            (
                listener: new ChoiceOnChangedListener
                (
                    MultipleChoicePanel: this,
                    ChoiceLabel: choiceLabel
                )
            );
            //choice.onChanged += choiceAction;

            choice.EnabledChanged.Add(listener: this);
            //choice.EnabledChanged += () =>
            //{
            //    if (choice.PersonallyEnabled || !choice.On)
            //        return;
                
            //    foreach (var choice in choicePanel)
            //        if (choice.PersonallyEnabled)
            //        {
            //            choice.On = true;
            //            return;
            //        }
            //    throw new Exception("enabled choice doesn't exist");
            //};

            choices.Add(choiceLabel, choice);
            //choiceOnChangedListeners.Add(choiceText, choiceOnChangedListener);
            //choiceActions.Add(choiceText, choiceAction);
            choicePanel.AddChild(child: choice);

            //return choice;
        }
    
        //public void ReplaceChoiceAction(string choiceText/*, Action newSelect*/)
        //{
        //    var choice = choices[choiceText];
        //    choice.onChanged.Remove(listener: choiceOnChangedListeners[choiceText]);
        //    //choice.onChanged -= choiceActions[choiceText];

        //    //if (selectedChoice == choice)
        //    if (choices[SelectedChoiceText] == choice)
        //        newSelect();

        //    void choiceAction()
        //    {
        //        if (!choice.On)
        //            return;

        //        //selectedChoice.On = false;
        //        choices[SelectedChoiceText].On = false;
        //        //selectedChoice = choice;
        //        SelectedChoiceText = choiceText;
        //        newSelect();
        //    }

        //    choice.onChanged += choiceAction;
        //    choiceActions[choiceText] = choiceAction;
        //}

        public void SetChoicePersonallyEnabled(TChoice choiceLabel, bool newPersonallyEnabled)
            => choices[choiceLabel].PersonallyEnabled = newPersonallyEnabled;

        //public SelectButton/*<MyRectangle>*/ GetChoice(TChoice choiceLabel)
        //    => choices[choiceLabel];

        void IEnabledChangedListener.EnabledChangedResponse(IUIElement UIElement)
        {
            if (UIElement is SelectButton/*<MyRectangle>*/ choice)
            {
                if (choice.PersonallyEnabled || !choice.On)
                    return;

                foreach (var posChoice in choicePanel)
                    if (posChoice.PersonallyEnabled)
                    {
                        posChoice.On = true;
                        return;
                    }
                throw new Exception("enabled choice doesn't exist");
            }
            else
                throw new ArgumentException();
        }
    }
}
