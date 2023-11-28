using Game1.Delegates;
using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public abstract class BaseButton<TVisual> : HUDElement, IWithTooltip
        where TVisual : IHUDElement
    {
        public ITooltip Tooltip { get; }

        public readonly Event<IClickedListener> clicked;

        public override bool CanBeClicked
            => true;

        public TVisual Visual
        {
            get => visual;
            set => ReplaceChild(ref visual, value);
        }

        private TVisual visual;

        protected BaseButton(NearRectangle shape, TVisual visual, ITooltip tooltip)
            : base(shape: shape)
        {
            this.visual = visual;
            AddChild(child: visual);
            clicked = new();
            
            Tooltip = tooltip;
        }

        public override IUIElement? CatchUIElement(Vector2Bare mouseScreenPos)
            // Override this so that button catches mouse, not the visual element
            => Contains(mouseScreenPos: mouseScreenPos) ? this : null;

        protected override void PartOfRecalcSizeAndPos()
        {
            base.PartOfRecalcSizeAndPos();
            if (visual is not null)
                visual.Shape.Center = Shape.Center;
        }

        public override void OnClick()
        {
            base.OnClick();
            clicked.Raise(action: listener => listener.ClickedResponse());
        }
    }
}
