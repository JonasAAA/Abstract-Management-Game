using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public class ToggleButton : OnOffButton
    {
        public ToggleButton(NearRectangle shape, ITooltip tooltip, string text, bool on, Color selectedColor, Color deselectedColor)
            : base(shape: shape, tooltip: tooltip, text: text, on: on, selectedColor: selectedColor, deselectedColor: deselectedColor)
        { }

        public override void OnClick()
            => On = !On;
    }
}
