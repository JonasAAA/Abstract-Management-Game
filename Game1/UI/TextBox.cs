using Game1.Shapes;
using static Game1.UI.ActiveUIManager;

namespace Game1.UI
{
    [Serializable]
    public class TextBox : HUDElement
    {
        // TODO: delete if unused
        public class ImmutableParams : IParams
        {
            public string? Text { get; }
            public Color BackgroundColor { get; }
            public Color TextColor { get; }

            public ImmutableParams(string? text, Color backgroundColor, Color? textColor = null)
            {
                Text = text;
                BackgroundColor = backgroundColor;
                TextColor = textColor ?? curUIConfig.defaultTextColor;
            }
        }

        public interface IParams : IPanelParams
        {
            public Color TextColor
                => curUIConfig.defaultTextColor;
            public string? Text { get; }
        }

        private readonly record struct ShapeParams(IParams Parameters) : MyRectangle.IParams
        {
            public Color Color
                => Parameters.BackgroundColor;
        }

        private static readonly SpriteFont font;

        static TextBox()
            => font = C.LoadFont(name: "font");

        public Color TextColor { private get; set; }

        // TODO: delete
        //public string? Text
        //{
        //    get => text;
        //    set
        //    {
        //        if (text != value)
        //        {
        //            text = value;
        //            MyVector2 textDims = text switch
        //            {
        //                null => MyVector2.zero,
        //                not null => MeasureText(text: text),
        //            };
        //            Shape.Width = (UDouble)textDims.X;
        //            Shape.Height = (UDouble)textDims.Y;
        //        }
        //    }
        //}

        //private string? text;
        private readonly UDouble scale;
        private readonly IParams parameters;
        private string? prevText;

        public TextBox(IParams parameters)
            : base(shape: new MyRectangle(parameters: new ShapeParams(Parameters: parameters)))
        {
            // TODO: delete
            //Shape.Color = Color.Transparent;
            // TODO: look up where font.MeasureString(...) is called, there should probably be a static readonly variable
            // storing what the height of a capital letter is
            this.parameters = parameters;
            scale = curUIConfig.letterHeight / (UDouble)font.MeasureString("F").Y;
            TextColor = Color.Black;
            prevText = null;
            //Text = null;
        }

        public MyVector2 MeasureText(string text)
            => (MyVector2)font.MeasureString(text) * scale;

        public override void Draw()
        {
            var curText = parameters.Text;
            if (curText != prevText)
            {
                MyVector2 textDims = curText switch
                {
                    null => MyVector2.zero,
                    not null => MeasureText(text: curText),
                };
                Shape.Width = (UDouble)textDims.X;
                Shape.Height = (UDouble)textDims.Y;
            }
            prevText = curText;
            if (curText is null)
                return;
            base.Draw();
            C.DrawString
            (
                spriteFont: font,
                text: curText,
                position: Shape.TopLeftCorner,
                color: Enabled switch
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
