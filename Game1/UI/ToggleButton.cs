using Game1.Shapes;
using Microsoft.Xna.Framework;
using System;

namespace Game1.UI
{
    [Serializable]
    public class ToggleButton : OnOffButton
    {
        public ToggleButton(NearRectangle shape, string text, bool on, Color selectedColor, Color deselectedColor)
            : base(shape: shape, text: text, on: on, selectedColor: selectedColor, deselectedColor: deselectedColor)
        { }

        public override void OnClick()
            => On = !On;
    }
}
