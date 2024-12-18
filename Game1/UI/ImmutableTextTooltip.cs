﻿using Game1.Shapes;
using static Game1.UI.ActiveUIManager;

namespace Game1.UI
{
    [Serializable]
    public sealed class ImmutableTextTooltip : ITooltip
    {
        public NearRectangle Shape
            => textBox.Shape;

        private readonly TextBox textBox;

        public ImmutableTextTooltip(string text)
            => textBox = new(text: text, backgroundColor: colorConfig.tooltipBackgroundColor);

        public void Update()
        { }

        public void Draw()
            => textBox.Draw();
    }
}
