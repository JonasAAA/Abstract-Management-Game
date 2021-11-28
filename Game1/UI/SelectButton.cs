using Game1.Shapes;
using Microsoft.Xna.Framework;
using System;

namespace Game1.UI
{
    [Serializable]
    public class SelectButton : OnOffButton
    {
        public override bool CanBeClicked
            => !On;

        public SelectButton(NearRectangle shape, string text, bool on, Color selectedColor, Color deselectedColor)
            : base(shape: shape, text: text, on: on, selectedColor: selectedColor, deselectedColor: deselectedColor)
        { }

        public override void OnClick()
            => On = true;
    }
}
