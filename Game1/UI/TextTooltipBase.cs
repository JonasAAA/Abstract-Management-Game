using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public abstract class TextTooltipBase : ITooltip
    {
        public NearRectangle Shape
            => textBox.Shape;

        protected abstract string Text { get; }

        private readonly TextBox textBox;

        protected TextTooltipBase()
            => textBox = new(backgroundColor: Color.LightPink);

        public void Update()
            => textBox.Text = Text;

        public void Draw()
            => textBox.Draw();
    }
}
