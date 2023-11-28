using Game1.Delegates;
using Game1.Shapes;
using static Game1.UI.ActiveUIManager;

namespace Game1.UI
{
    [Serializable]
    public sealed class NumIncDecrPanel : HUDElement
    {
        [Serializable]
        private sealed class NumIncrButtonClickedListener(NumIncDecrPanel numIncDecrPanel) : IClickedListener
        {
            void IClickedListener.ClickedResponse()
                => numIncDecrPanel.Number++;
        }

        [Serializable]
        private sealed class NumDecrButtonClickedListener(NumIncDecrPanel numIncDecrPanel) : IClickedListener
        {
            void IClickedListener.ClickedResponse()
            {
                if (numIncDecrPanel.Number > numIncDecrPanel.minNum)
                    numIncDecrPanel.Number--;
            }
        }

        public int Number
        {
            get => number;
            private set
            {
                if (value < minNum)
                    throw new ArgumentException();
                if (number == value)
                    return;

                SetNumber(newNumber: value);
                numberChanged.Raise(action: listener => listener.NumberChangedResponse());
            }
        }

        public Event<INumberChangedListener> numberChanged;

        protected sealed override Color Color
            => colorConfig.UIBackgroundColor;

        private int number;
        private readonly Button<TextBox> numDecrButton;
        private readonly int minNum;
        private readonly UIRectVertPanel<IHUDElement> panel;
        private readonly TextBox textBox;

        public NumIncDecrPanel(int minNum, int number, UDouble incrDecrButtonHeight, ITooltip incrButtonTooltip, ITooltip decrButtonTooltip, Color incrDecrButtonColor)
            : base(shape: new MyRectangle())
        {
            numberChanged = new();
            if (number < minNum)
                throw new ArgumentException();
            this.minNum = minNum;

            
            textBox = new();
            //textBox.Text = number.ToString();
            var width = (UDouble)textBox.MeasureText(text: "00").X;
            textBox.Shape.MinWidth = width;

            Button<TextBox> numIncrButton = new
            (
                shape: new Triangle
                (
                    width: width,
                    height: incrDecrButtonHeight,
                    direction: Triangle.Direction.Up
                ),
                visual: new(),
                tooltip: incrButtonTooltip,
                color: incrDecrButtonColor
            );
            numIncrButton.clicked.Add(listener: new NumIncrButtonClickedListener(numIncDecrPanel: this));

            numDecrButton = new
            (
                shape: new Triangle
                (
                    width: width,
                    height: incrDecrButtonHeight,
                    direction: Triangle.Direction.Down
                ),
                visual: new(),
                tooltip: decrButtonTooltip,
                color: incrDecrButtonColor
            );
            numDecrButton.clicked.Add(listener: new NumDecrButtonClickedListener(numIncDecrPanel: this));

            panel = new
            (
                childHorizPos: HorizPosEnum.Middle,
                children: new List<IHUDElement>()
                {
                    numIncrButton,
                    textBox,
                    numDecrButton
                }
            );

            Shape.Width = panel.Shape.Width;
            Shape.Height = panel.Shape.Height;

            AddChild(child: panel);

            SetNumber(newNumber: number);
        }

        protected sealed override void PartOfRecalcSizeAndPos()
        {
            base.PartOfRecalcSizeAndPos();

            Shape.Width = panel.Shape.Width;
            Shape.Height = panel.Shape.Height;
            panel.Shape.Center = Shape.Center;
        }

        private void SetNumber(int newNumber)
        {
            number = newNumber;
            textBox.Text = number.ToString();
            numDecrButton.PersonallyEnabled = number > minNum;
        }
    }
}
