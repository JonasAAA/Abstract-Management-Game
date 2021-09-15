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
                    Shape.SetWidth
                    (
                        width: textDims.X,
                        horizOrigin: MyRectangle.HorizOrigin.Middle
                    );
                    Shape.SetHeight
                    (
                        height: textDims.Y,
                        vertOrigin: MyRectangle.VertOrigin.Middle
                    );
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
            C.SpriteBatch.DrawString
            (
                spriteFont: font,
                text: text,
                position: Shape.TopLeftCorner,
                color: TextColor,
                rotation: 0,
                origin: Vector2.Zero,
                scale: scale,
                effects: SpriteEffects.None,
                layerDepth: 0
            );
        }
    }
}
