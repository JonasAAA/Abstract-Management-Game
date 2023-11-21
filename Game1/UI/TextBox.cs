using Game1.ContentNames;
using Game1.Shapes;
using static Game1.UI.ActiveUIManager;

namespace Game1.UI
{
    [Serializable]
    public sealed class TextBox : HUDElement
    {
        private static readonly SpriteFont font = C.LoadFont(name: FontName.mainFont);

        public string? Text
        {
            get => text;
            set
            {
                if (text != value)
                {
                    text = value;
                    Vector2Bare textDims = text switch
                    {
                        null => Vector2Bare.zero,
                        not null => MeasureText(text: text),
                    };
                    Shape.Width = (UDouble)textDims.X;
                    Shape.Height = (UDouble)textDims.Y;
                }
            }
        }

        protected sealed override Color Color { get; }

        private readonly Color textColor;
        private string? text;
        private readonly UDouble scale;

        public TextBox(string? text = null, Color? backgroundColor = null, Color? textColor = null)
            : base(shape: new MyRectangle())
        {
            scale = UDouble.half;
            Color = backgroundColor ?? Color.Transparent;
            this.textColor = textColor ?? colorConfig.textColor;
            Text = text;
        }

        public Vector2Bare MeasureText(string text)
            => (Vector2Bare)font.MeasureString(text) * scale;

        protected sealed override void DrawChildren()
        {
            if (text is null)
                return;
            C.DrawString
            (
                spriteFont: font,
                text: text,
                position: Shape.TopLeftCorner,
                color: (PersonallyEnabled && !HasDisabledAncestor) switch
                {
                    true => textColor,
                    false => textColor * .5f
                },
                origin: Vector2Bare.zero,
                scale: scale
            );
        }
    }
}
