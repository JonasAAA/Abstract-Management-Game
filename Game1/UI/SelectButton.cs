using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public sealed class SelectButton : OnOffButton
    {
        public override bool CanBeClicked
            => !On;

        public SelectButton(NearRectangle shape, ITooltip tooltip, string text, bool on)
            : base(shape: shape, tooltip: tooltip, text: text, on: on)
        { }

        public override void OnClick()
            => On = true;
    }
}
