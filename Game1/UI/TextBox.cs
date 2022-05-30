using Game1.Shapes;
using static Game1.UI.ActiveUIManager;

namespace Game1.UI
{
    [Serializable]
    public sealed class TextBox : HUDElement
    {
        private static readonly SpriteFont font;

        static TextBox()
            => font = C.LoadFont(name: "font");

        public string? Text
        {
            get => text;
            set
            {
                if (text != value)
                {
                    text = value;
                    MyVector2 textDims = text switch
                    {
                        null => MyVector2.zero,
                        not null => MeasureText(text: text),
                    };
                    Shape.Width = (UDouble)textDims.X;
                    Shape.Height = (UDouble)textDims.Y;
                }
            }
        }

        protected override Color Color { get; }

        private readonly Color textColor;
        private string? text;
        private readonly UDouble scale;

        public TextBox(Color? backgroundColor = null, Color? textColor = null)
            : base(shape: new MyRectangle())
        {
            scale = curUIConfig.letterHeight / (UDouble)font.MeasureString("F").Y;
            Color = backgroundColor ?? Color.Transparent;
            this.textColor = textColor ?? curUIConfig.defaultTextColor;
            Text = null;
        }

        public MyVector2 MeasureText(string text)
            => (MyVector2)font.MeasureString(text) * scale;

        protected override void DrawChildren()
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
                origin: MyVector2.zero,
                scale: scale
            );
        }
    }
}
