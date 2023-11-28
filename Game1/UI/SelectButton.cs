using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public sealed class SelectButton<TVisual> : OnOffButton<TVisual>
        where TVisual : IHUDElement
    {
        public sealed override bool CanBeClicked
            => !On;

        public SelectButton(NearRectangle shape, TVisual visual, ITooltip tooltip, bool on)
            : base(shape: shape, visual: visual, tooltip: tooltip, on: on)
        { }

        public sealed override void OnClick()
            => On = true;
    }
}
