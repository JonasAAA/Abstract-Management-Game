using Game1.Delegates;
using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public class NumIncDecrPanel : HUDElement
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
                => NumIncDecrPanel.Number--;
        }

        public interface IParamsAndState : MyRectangle.IParams, UIRectVertPanel<IHUDElement>.IParams, TextBox.IParams
        {
            public int MinNumber { get; }
            public int MaxNumber { get; }
            public Color IncrAndDecrButtonColor { get; }

            public int Number { get; set; }

            string? TextBox.IParams.Text
                => Number.ToString();
        }

        private readonly record struct NumIncrOrDecrButtonShapeParams(IParamsAndState ParamsAndState, bool Increase) : Triangle.IParams
        {
            public Color Color
                => ParamsAndState.IncrAndDecrButtonColor;
        }

        private readonly record struct NumIncrOrDecrButtonParams(bool Increase) : Button.IParams
        {
            public string? Text
                => null;

            public string? Explanation
                => Increase ? "Increase importance to route a\nbigger proportion of resources through here" : "Decrease importance to route a \nsmaller proportion of resources through here";
        }

        private int Number
        {
            get => paramsAndState.Number;
            set
            {
                if (value < paramsAndState.MinNumber || value > paramsAndState.MaxNumber)
                    return;
                paramsAndState.Number = value;
            }
        }

        // TODO: delete
        //public int Number
        //{
        //    get => number;
        //    private set
        //    {
        //        if (value < minNum)
        //            throw new ArgumentException();
        //        if (number == value)
        //            return;

        //        number = value;
        //        textBox.Text = number.ToString();
        //        numberChanged.Raise(action: listener => listener.NumberChangedResponse());
        //    }
        //}

        //public Event<INumberChangedListener> numberChanged;

        private readonly IParamsAndState paramsAndState;
        private readonly UIRectVertPanel<IHUDElement> panel;
        private readonly TextBox textBox;

        public NumIncDecrPanel(UDouble incrDecrButtonHeight, IParamsAndState paramsAndState)
            : base(shape: new MyRectangle(parameters: paramsAndState), parameters: paramsAndState)
        {
            this.paramsAndState = paramsAndState;
            // TODO: delete
            //numberChanged = new();
            //if (number < minNum)
            //    throw new ArgumentException();
            //this.minNum = minNum;
            //this.number = number;
            panel = new
            (
                parameters: paramsAndState,
                childHorizPos: HorizPos.Middle
            );
            textBox = new(parameters: paramsAndState);
            UDouble width = (UDouble)textBox.MeasureText(text: "00").X;
            textBox.Shape.MinWidth = width;

            Button numIncrButton = new
            (
                shape: new Triangle
                (
                    width: width,
                    height: incrDecrButtonHeight,
                    parameters: new NumIncrOrDecrButtonShapeParams(ParamsAndState: paramsAndState, Increase: true),
                    direction: Triangle.Direction.Up
                ),
                parameters: new NumIncrOrDecrButtonParams(Increase: true)
            );
            numIncrButton.clicked.Add(listener: new NumIncrButtonClickedListener(NumIncDecrPanel: this));
            panel.AddChild(child: numIncrButton);

            panel.AddChild(child: textBox);

            Button numDecrButton = new
            (
                shape: new Triangle
                (
                    width: width,
                    height: incrDecrButtonHeight,
                    parameters: new NumIncrOrDecrButtonShapeParams(ParamsAndState: paramsAndState, Increase: false),
                    direction: Triangle.Direction.Down
                ),
                parameters: new NumIncrOrDecrButtonParams(Increase: false)
            );
            numDecrButton.clicked.Add(listener: new NumDecrButtonClickedListener(NumIncDecrPanel: this));
            panel.AddChild(child: numDecrButton);

            Shape.Width = panel.Shape.Width;
            Shape.Height = panel.Shape.Height;

            AddChild(child: panel);
        }

        protected override void PartOfRecalcSizeAndPos()
        {
            base.PartOfRecalcSizeAndPos();

            Shape.Width = panel.Shape.Width;
            Shape.Height = panel.Shape.Height;
            panel.Shape.Center = Shape.Center;
        }
    }
}
