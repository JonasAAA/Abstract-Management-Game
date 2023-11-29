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
                    (Shape.Width, Shape.Height) = text switch
                    {
                        null => (width: UDouble.zero, height: UDouble.zero),
                        not null => MeasureText(text: text),
                    };
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

        public (UDouble width, UDouble height) MeasureText(string text)
        {
            var dims = font.MeasureString(text) * (float)scale;
            return (width: (UDouble)dims.X, height: (UDouble)dims.Y);
        }

        protected sealed override void DrawChildren()
        {
            if (text is null)
                return;
            C.DrawString
            (
                spriteFont: font,
                text: text,
                //position: Shape.Center,
                position: Shape.TopLeftCorner,
                color: (PersonallyEnabled && !HasDisabledAncestor) switch
                {
                    true => textColor,
                    false => textColor * .5f
                },
                //origin: new Vector2Bare(x: Shape.Width, y: Shape.Height) * UDouble.half / scale,
                origin: Vector2Bare.zero,
                scale: scale
            );
        }
    }
}
