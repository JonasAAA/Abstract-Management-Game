using Game1.Shapes;
using static Game1.UI.ActiveUIManager;

namespace Game1.UI
{
    [Serializable]
    public sealed class LazyTextBox : HUDElement
    {
        private static readonly SpriteFont font = C.LoadFont(name: "Fonts/MainFont");

        protected sealed override Color Color { get; }

        private readonly Color textColor;
        private readonly ILazyText lazyText;
        private readonly UDouble scale;

        public LazyTextBox(ILazyText lazyText, Color? backgroundColor = null, Color? textColor = null)
            : base(shape: new MyRectangle())
        {
            this.lazyText = lazyText;
            scale = UDouble.half;
            
            Color = backgroundColor ?? Color.Transparent;
            this.textColor = textColor ?? colorConfig.textColor;
            GetTextAndUpdateSize();
        }

        private string GetTextAndUpdateSize()
        {
#warning This update happens one frame too late
            // Would probably be better to introduce UpdateUI method to IUIElement, and do these text, animation, etc. updates in there
            string text = lazyText.GetText();
            var textDims = (Vector2Bare)font.MeasureString(text) * scale;
            Shape.Width = (UDouble)textDims.X;
            Shape.Height = (UDouble)textDims.Y;
            return text;
        }

        protected sealed override void DrawChildren()
            => C.DrawString
            (
                spriteFont: font,
                text: GetTextAndUpdateSize(),
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
