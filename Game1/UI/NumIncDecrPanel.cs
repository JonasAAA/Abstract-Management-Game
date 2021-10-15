using Microsoft.Xna.Framework;
using System;

namespace Game1.UI
{
    public class NumIncDecrPanel : HUDElement<MyRectangle>
    {
        public int Number
        {
            get => number;
            private set
            {
                if (value < minNum)
                    throw new ArgumentException();
                if (number == value)
                    return;

                number = value;
                textBox.Text = number.ToString();
                NumberChanged?.Invoke();
            }
        }

        public event Action NumberChanged;

        private int number;
        private readonly int minNum;
        private readonly UIRectVertPanel<IHUDElement<NearRectangle>> panel;
        private readonly TextBox textBox;

        public NumIncDecrPanel(int minNum, int number, float incrDecrButtonHeight, Color shapeColor, Color incrDecrButtonColor)
            : base(shape: new())
        {
            if (number < minNum)
                throw new ArgumentException();
            this.minNum = minNum;
            this.number = number;
            panel = new
            (
                color: shapeColor,
                childHorizPos: HorizPos.Middle
            );
            textBox = new();
            textBox.Text = number.ToString();
            float width = textBox.MeasureText(text: "00").X;
            textBox.Shape.MinWidth = width;
            panel.AddChild
            (
                child: new Button<Triangle>
                (
                    shape: new
                    (
                        width: width,
                        height: incrDecrButtonHeight,
                        direction: Triangle.Direction.Up
                    )
                    {
                        Color = incrDecrButtonColor
                    },
                    action: () => Number++
                )
            );
            panel.AddChild(child: textBox);
            panel.AddChild
            (
                child: new Button<Triangle>
                (
                    shape: new
                    (
                        width: width,
                        height: incrDecrButtonHeight,
                        direction: Triangle.Direction.Down
                    )
                    {
                        Color = Color.Blue
                    },
                    action: () =>
                    {
                        if (Number > minNum)
                            Number--;
                    }
                )
            );

            Shape.Color = shapeColor;
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
