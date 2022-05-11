using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public class ImmutableTextTooltip : ITooltip
    {
        public NearRectangle Shape
            => textBox.Shape;

        private readonly TextBox textBox;

        public ImmutableTextTooltip(string text)
        {
            textBox = new(backgroundColor: Color.LightPink)
            {
                Text = text
            };
        }

        public void Update()
        { }

        public void Draw()
            => textBox.Draw();
    }
}
