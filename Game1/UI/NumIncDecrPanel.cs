using Game1.Delegates;
using Game1.Shapes;
using static Game1.UI.ActiveUIManager;

namespace Game1.UI
{
    [Serializable]
    public sealed class NumIncDecrPanel : HUDElement
    {
        [Serializable]
        private readonly record struct NumIncrButtonClickedListener(NumIncDecrPanel NumIncDecrPanel) : IClickedListener
        {
            void IClickedListener.ClickedResponse()
                => NumIncDecrPanel.Number++;
        }

        [Serializable]
        private readonly record struct NumDecrButtonClickedListener(NumIncDecrPanel NumIncDecrPanel) : IClickedListener
        {
            void IClickedListener.ClickedResponse()
            {
                if (NumIncDecrPanel.Number > NumIncDecrPanel.minNum)
                    NumIncDecrPanel.Number--;
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

        protected override Color Color
            => curUIConfig.UIBackgroundColor;

        private int number;
        private readonly Button numDecrButton;
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

            panel = new(childHorizPos: HorizPos.Middle);
            textBox = new();
            //textBox.Text = number.ToString();
            UDouble width = (UDouble)textBox.MeasureText(text: "00").X;
            textBox.Shape.MinWidth = width;

            Button numIncrButton = new
            (
                shape: new Triangle
                (
                    width: width,
                    height: incrDecrButtonHeight,
                    direction: Triangle.Direction.Up
                ),
                tooltip: incrButtonTooltip,
                color: incrDecrButtonColor
            );
            numIncrButton.clicked.Add(listener: new NumIncrButtonClickedListener(NumIncDecrPanel: this));
            panel.AddChild(child: numIncrButton);

            panel.AddChild(child: textBox);

            numDecrButton = new
            (
                shape: new Triangle
                (
                    width: width,
                    height: incrDecrButtonHeight,
                    direction: Triangle.Direction.Down
                ),
                tooltip: decrButtonTooltip,
                color: incrDecrButtonColor
            );
            numDecrButton.clicked.Add(listener: new NumDecrButtonClickedListener(NumIncDecrPanel: this));
            panel.AddChild(child: numDecrButton);

            Shape.Width = panel.Shape.Width;
            Shape.Height = panel.Shape.Height;

            AddChild(child: panel);

            SetNumber(newNumber: number);
        }

        protected override void PartOfRecalcSizeAndPos()
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
