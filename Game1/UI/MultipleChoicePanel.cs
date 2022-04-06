using Game1.Delegates;
using Game1.PrimitiveTypeWrappers;
using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public class MultipleChoicePanel<TChoice> : HUDElement
    {
        // it is public to easily add it to knownTypes
        [Serializable]
        public readonly record struct ChoiceEventListener(MultipleChoicePanel<TChoice> MultipleChoicePanel, TChoice ChoiceLabel) : IOnChangedListener, IEnabledChangedListener
        {
            void IEnabledChangedListener.EnabledChangedResponse()
            {
                var choice = MultipleChoicePanel.choices[ChoiceLabel];

                if (choice.PersonallyEnabled || !choice.On)
                    return;

                foreach (var posChoice in MultipleChoicePanel.choicePanel)
                    if (posChoice.PersonallyEnabled)
                    {
                        posChoice.On = true;
                        return;
                    }
                throw new Exception("enabled choice doesn't exist");
            }

            void IOnChangedListener.OnChangedResponse()
            {
                if (!MultipleChoicePanel.choices[ChoiceLabel].On)
                    return;

                MultipleChoicePanel.choices[MultipleChoicePanel.SelectedChoiceLabel].On = false;
                MultipleChoicePanel.SelectedChoiceLabel = ChoiceLabel;
            }
        }

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

        private readonly UIRectPanel<SelectButton> choicePanel;
        private readonly Dictionary<TChoice, SelectButton> choices;
        private readonly UDouble choiceWidth, choiceHeight;
        private readonly Color selectedColor, deselectedColor;
        private TChoice selectedChoiceLabel;

        public MultipleChoicePanel(bool horizontal, UDouble choiceWidth, UDouble choiceHeight, Color selectedColor, Color deselectedColor, Color backgroundColor)
            : base(shape: new MyRectangle())
        {
            choiceChanged = new();
            choicePanel = horizontal switch
            {
                true => new UIRectHorizPanel<SelectButton>
                (
                    color: backgroundColor,
                    childVertPos: VertPos.Top
                ),
                false => new UIRectVertPanel<SelectButton>
                (
                    color: backgroundColor,
                    childHorizPos: HorizPos.Left
                )
            };
            
            Shape.Color = backgroundColor;
            selectedChoiceLabel = default;
            choices = new();

            this.choiceWidth = choiceWidth;
            this.choiceHeight = choiceHeight;
            this.selectedColor = selectedColor;
            this.deselectedColor = deselectedColor;

            AddChild(child: choicePanel);
        }

        protected override void PartOfRecalcSizeAndPos()
        {
            base.PartOfRecalcSizeAndPos();

            Shape.Width = choicePanel.Shape.Width;
            Shape.Height = choicePanel.Shape.Height;
            choicePanel.Shape.Center = Shape.Center;
        }

        public void AddChoice(TChoice choiceLabel)
        {
            if (choices.ContainsKey(choiceLabel))
                throw new ArgumentException();

            SelectButton choice = new
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
                SelectedChoiceLabel = choiceLabel;

            ChoiceEventListener choiceEventListener = new
            (
                MultipleChoicePanel: this,
                ChoiceLabel: choiceLabel
            );
            choice.onChanged.Add(listener: choiceEventListener);
            choice.EnabledChanged.Add(listener: choiceEventListener);

            choices.Add(choiceLabel, choice);
            choicePanel.AddChild(child: choice);
        }

        public void SetChoicePersonallyEnabled(TChoice choiceLabel, bool newPersonallyEnabled)
            => choices[choiceLabel].PersonallyEnabled = newPersonallyEnabled;
    }
}
