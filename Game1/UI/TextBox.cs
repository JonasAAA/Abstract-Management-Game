using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game1.UI
{
    public class TextBox : HUDElement<MyRectangle>
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
                    Vector2 textDims = text switch
                    {
                        null => Vector2.Zero,
                        not null => MeasureText(text: text),
                    };
                    Shape.Width = textDims.X;
                    Shape.Height = textDims.Y;
                }
            }
        }
        public Vector2 MeasureText(string text)
            => font.MeasureString(text) * scale;

        private string text;
        private readonly SpriteFont font;
        private readonly float scale;

        public TextBox()
            : base(shape: new())
        {
            Shape.Color = Color.Transparent;
            font = C.LoadFont(name: "font");
            scale = ActiveUIManager.UIConfig.letterHeight / font.MeasureString("F").Y;
            TextColor = Color.Black;
            Text = null;
        }

        public override void Draw()
        {
            if (Text is null)
                return;
            base.Draw();
            C.DrawString
            (
                spriteFont: font,
                text: text,
                position: Shape.TopLeftCorner,
                color: (PersonallyEnabled && !HasDisabledAncestor) switch
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
