using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game1.UI
{
    public class TextBox : IUIElement<MyRectangle>
    {
        public MyRectangle Shape { get; }

        public Field<bool> Enabled { get; }

        public Color TextColor { private get; set; }
        public string Text
        {
            get => text;
            set
            {
                if (text != value)
                {
                    text = value;
                    Vector2 textDims = font.MeasureString(text.Trim()) * scale;
                    Shape.Width = textDims.X;
                    Shape.Height = textDims.Y;
                }
            }
        }

        private string text;
        private readonly SpriteFont font;
        private readonly float scale;

        public TextBox(float letterHeight)
        {
            Shape = new()
            {
                Color = Color.Transparent
            };
            Enabled = new(value: true);
            font = C.Content.Load<SpriteFont>("font");
            scale = letterHeight / font.MeasureString("F").Y;
            TextColor = Color.Black;
            Text = "";
        }

        void IUIElement.Draw()
        {
            IUIElement.DefaultDraw(UIElement: this);
            //base.Draw();
            C.DrawString
            (
                spriteFont: font,
                text: text.Trim(),
                position: Shape.TopLeftCorner,
                color: (bool)Enabled switch
                {
                    true => TextColor,
                    false => TextColor * .5f
                },
                origin: Vector2.Zero,
                scale: scale
            );
        }
    }
}
