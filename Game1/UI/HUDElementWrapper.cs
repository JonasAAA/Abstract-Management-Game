using Game1.Shapes;
using static Game1.UI.ActiveUIManager;

namespace Game1.UI
{
    [Serializable]
    public sealed class HUDElementWrapper : HUDElement
    {
        public IHUDElement? Value
        {
            get => value;
            set
            {
                if (this.value is not null)
                    RemoveChild(child: this.value);
                this.value = value;
                if (this.value is not null)
                    AddChild(child: this.value);
            }
        }

        protected sealed override Color Color { get; }

        private IHUDElement? value;

        public HUDElementWrapper(IHUDElement? value, Color? backgroundColor = null)
            : base(shape: new MyRectangle())
        {
            this.value = null;
            Value = value;
            Color = backgroundColor ?? colorConfig.UIBackgroundColor;
        }

        protected sealed override void PartOfRecalcSizeAndPos()
        {
            base.PartOfRecalcSizeAndPos();

            Shape.Width = Value?.Shape.Width ?? 0;
            Shape.Height = Value?.Shape.Height ?? 0;
            if (Value is not null)
                Value.Shape.Center = Shape.Center;
        }
    }
}
