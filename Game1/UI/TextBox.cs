using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game1.UI
{
    public class TextBox : UIElement<MyRectangle>
    {
        public Color TextColor { private get; set; }
        public string Text
        {
            get => text;
            set
            {
                if (text != value)
                {
                    text = value;
                    Vector2 textDims = font.MeasureString(text) * scale;
                    Shape.Width = textDims.X;
                    Shape.Height = textDims.Y;
                }
            }
        }

        private string text;
        private readonly SpriteFont font;
        private readonly float scale;

        public TextBox(float letterHeight)
            : base(shape: new MyRectangle())
        {
            Shape.Color = Color.Transparent;
            font = C.Content.Load<SpriteFont>("font");
            scale = letterHeight / font.MeasureString("F").Y;
            TextColor = Color.Black;
            Text = "";
        }

        public override void Draw()
        {
            base.Draw();
            C.DrawString
            (
                spriteFont: font,
                text: text,
                position: Shape.TopLeftCorner,
                color: TextColor,
                origin: Vector2.Zero,
                scale: scale
            );
        }
    }
}
