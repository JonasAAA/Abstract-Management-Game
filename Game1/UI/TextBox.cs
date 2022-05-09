using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public class TextBox : HUDElement
    {
        private static readonly SpriteFont font;

        static TextBox()
            => font = C.LoadFont(name: "font");

        public Color TextColor { private get; set; }
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

        private string? text;
        private readonly UDouble scale;

        public TextBox()
            : base(shape: new MyRectangle(color: Color.White))
        {
            Shape.Color = Color.Transparent;
            // TODO: look up where font.MeasureString(...) is called, there should probably be a static readonly variable
            // storing what the height of a capital letter is
            scale = ActiveUIManager.curUIConfig.letterHeight / (UDouble)font.MeasureString("F").Y;
            TextColor = Color.Black;
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
                    true => TextColor,
                    false => TextColor * .5f
                },
                origin: MyVector2.zero,
                scale: scale
            );
        }
    }
}
