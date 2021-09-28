using Microsoft.Xna.Framework;

namespace Game1.UI
{
    public class SelectButton<TShape> : OnOffButton<TShape>
        where TShape : NearRectangle
    {
        public override bool CanBeClicked
            => !On;

        public SelectButton(TShape shape, string text, bool on, Color selectedColor, Color deselectedColor)
            : base(shape: shape, text: text, on: on, selectedColor: selectedColor, deselectedColor: deselectedColor)
        { }

        public override void OnClick()
            => On = true;
    }
}
