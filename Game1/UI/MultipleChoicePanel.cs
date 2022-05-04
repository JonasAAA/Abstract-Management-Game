using Game1.Delegates;
using Game1.Shapes;
using System.Diagnostics.CodeAnalysis;

namespace Game1.UI
{
    [Serializable]
    public class MultipleChoicePanel<TChoice> : HUDElement
        where TChoice : notnull
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
            [MemberNotNull(nameof(selectedChoiceLabel))]
            private set
            {
                var prevSelectedChoiceLabel = selectedChoiceLabel;
                selectedChoiceLabel = value;
                if (prevSelectedChoiceLabel is not null && !C.Equals(prevSelectedChoiceLabel, selectedChoiceLabel))
                    choiceChanged.Raise(action: listener => listener.ChoiceChangedResponse(prevChoice: prevSelectedChoiceLabel));
            }
        }

        private readonly UIRectPanel<SelectButton> choicePanel;
        private readonly Dictionary<TChoice, SelectButton> choices;
        private readonly UDouble choiceWidth, choiceHeight;
        private readonly Color selectedColor, deselectedColor;
        private TChoice selectedChoiceLabel;

        public MultipleChoicePanel(bool horizontal, UDouble choiceWidth, UDouble choiceHeight, Color selectedColor, Color deselectedColor, Color backgroundColor, IEnumerable<TChoice> choiceLabels)
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

            this.choiceWidth = choiceWidth;
            this.choiceHeight = choiceHeight;
            this.selectedColor = selectedColor;
            this.deselectedColor = deselectedColor;

            Shape.Color = backgroundColor;
            var choiceLabelsArray = choiceLabels.ToArray();
            if (choiceLabelsArray.Length is 0)
                throw new ArgumentException($"must provide at least one choice to start with");
            choices = new();
            foreach (var choiceLabel in choiceLabelsArray)
                AddChoice(choiceLabel: choiceLabel);

            SelectedChoiceLabel = choiceLabelsArray[0];

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
                text: choiceLabel.ToString() ?? throw new Exception("The label text must be not null"),
                selectedColor: selectedColor,
                deselectedColor: deselectedColor
            );

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
