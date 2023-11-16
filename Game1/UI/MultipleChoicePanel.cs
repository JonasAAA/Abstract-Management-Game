using Game1.Delegates;
using Game1.Shapes;
using System.Diagnostics.CodeAnalysis;
using static Game1.UI.ActiveUIManager;

namespace Game1.UI
{
    [Serializable]
    public sealed class MultipleChoicePanel<TChoiceLabel> : HUDElement
        where TChoiceLabel : notnull
    {
        // it is public to easily add it to knownTypes
        [Serializable]
        public sealed class ChoiceEventListener(MultipleChoicePanel<TChoiceLabel> multipleChoicePanel, TChoiceLabel choiceLabel) : IOnChangedListener, IEnabledChangedListener
        {
            void IEnabledChangedListener.EnabledChangedResponse()
            {
                var choice = multipleChoicePanel.choices[choiceLabel];

                if (choice.PersonallyEnabled || !choice.On)
                    return;

                foreach (var posChoice in multipleChoicePanel.choicePanel)
                    if (posChoice.PersonallyEnabled)
                    {
                        posChoice.On = true;
                        return;
                    }
                throw new ArgumentException("enabled choice doesn't exist");
            }

            void IOnChangedListener.OnChangedResponse()
            {
                if (!multipleChoicePanel.choices[choiceLabel].On)
                    return;

                multipleChoicePanel.choices[multipleChoicePanel.SelectedChoiceLabel].On = false;
                multipleChoicePanel.SelectedChoiceLabel = choiceLabel;
            }
        }

        public readonly Event<IChoiceChangedListener<TChoiceLabel>> choiceChanged;

        public TChoiceLabel SelectedChoiceLabel
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

        protected sealed override Color Color
            => colorConfig.UIBackgroundColor;

        private readonly UIRectPanel<SelectButton> choicePanel;
        private readonly Dictionary<TChoiceLabel, SelectButton> choices;
        private readonly UDouble choiceWidth, choiceHeight;
        private TChoiceLabel selectedChoiceLabel;

        public MultipleChoicePanel(bool horizontal, UDouble choiceWidth, UDouble choiceHeight, IEnumerable<(TChoiceLabel label, ITooltip tooltip)> choiceLabelsAndTooltips)
            : base(shape: new MyRectangle())
        {
            choiceChanged = new();
            choicePanel = horizontal switch
            {
                true => new UIRectHorizPanel<SelectButton>
                (
                    childVertPos: VertPosEnum.Top,
                    children: Enumerable.Empty<SelectButton>()
                ),
                false => new UIRectVertPanel<SelectButton>
                (
                    childHorizPos: HorizPosEnum.Left,
                    children: Enumerable.Empty<SelectButton>()
                )
            };

            this.choiceWidth = choiceWidth;
            this.choiceHeight = choiceHeight;

            var choiceLabelsAndTooltipsArray = choiceLabelsAndTooltips.ToArray();
            if (choiceLabelsAndTooltipsArray.Length is 0)
                throw new ArgumentException($"must provide at least one choice to start with");
            choices = [];
            foreach (var (choiceLabel, choiceTooltip) in choiceLabelsAndTooltipsArray)
                AddChoice(choiceLabel: choiceLabel, choiceTooltip: choiceTooltip);

            SelectedChoiceLabel = choiceLabelsAndTooltipsArray[0].label;

            AddChild(child: choicePanel);
        }

        protected sealed override void PartOfRecalcSizeAndPos()
        {
            base.PartOfRecalcSizeAndPos();

            Shape.Width = choicePanel.Shape.Width;
            Shape.Height = choicePanel.Shape.Height;
            choicePanel.Shape.Center = Shape.Center;
        }

        public void AddChoice(TChoiceLabel choiceLabel, ITooltip choiceTooltip)
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
                tooltip: choiceTooltip,
                text: choiceLabel.ToString() ?? throw new ArgumentException("The label text must be not null")
            );

            ChoiceEventListener choiceEventListener = new
            (
                multipleChoicePanel: this,
                choiceLabel: choiceLabel
            );
            choice.onChanged.Add(listener: choiceEventListener);
            choice.EnabledChanged.Add(listener: choiceEventListener);

            choices.Add(choiceLabel, choice);
            choicePanel.AddChild(child: choice);
        }

        public void SetChoicePersonallyEnabled(TChoiceLabel choiceLabel, bool newPersonallyEnabled)
            => choices[choiceLabel].PersonallyEnabled = newPersonallyEnabled;
    }
}
