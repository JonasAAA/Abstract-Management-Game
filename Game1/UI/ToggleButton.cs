﻿using Microsoft.Xna.Framework;

namespace Game1.UI
{
    public class ToggleButton<TShape> : OnOffButton<TShape>
        where TShape : NearRectangle
    {
        public ToggleButton(TShape shape, float letterHeight, string text, bool on, Color selectedColor, Color deselectedColor)
            : base(shape: shape, letterHeight: letterHeight, text: text, on: on, selectedColor: selectedColor, deselectedColor: deselectedColor)
        { }

        public override void OnClick()
            => On = !On;
    }
}
