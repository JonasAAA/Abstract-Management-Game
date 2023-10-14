using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public sealed class VertProporBar : HUDElement
    {
        public Propor Propor { get; set; }

        protected override Color Color { get; }

        private readonly Color barColor;

        public VertProporBar(UDouble width, UDouble height, Propor propor, Color barColor, Color backgroundColor)
            : base(shape: new MyRectangle(width: width, height: height))
        {
            this.barColor = barColor;
            Color = backgroundColor;
            Propor = propor;
        }

        protected override void DrawChildren()
        {
            base.DrawChildren();
            new MyRectangle(width: Shape.Width, height: Shape.Height * Propor)
            {
                BottomLeftCorner = Shape.BottomLeftCorner
            }.Draw(color: barColor);
        } 
    }
}
