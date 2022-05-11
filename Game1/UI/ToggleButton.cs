using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public class ToggleButton : OnOffButton
    {
        public ToggleButton(NearRectangle shape, ITooltip tooltip, string text, bool on)
            : base(shape: shape, tooltip: tooltip, text: text, on: on)
        { }

        public override void OnClick()
            => On = !On;
    }
}
