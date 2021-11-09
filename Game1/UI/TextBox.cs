using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
using static Game1.UI.ActiveUIManager;

namespace Game1.UI
{
    [DataContract]
    public class TextBox : HUDElement
    {
        private static readonly SpriteFont font;

        static TextBox()
            => font = C.LoadFont(name: "font");

        [DataMember] public Color TextColor { private get; set; }
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

        [DataMember] private string text;
        [DataMember] private readonly float scale;

        public TextBox()
            : base(shape: new MyRectangle())
        {
            Shape.Color = Color.Transparent;
            scale = CurUIConfig.letterHeight / font.MeasureString("F").Y;
            TextColor = Color.Black;
            Text = null;
        }

        public Vector2 MeasureText(string text)
            => font.MeasureString(text) * scale;

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
