using Game1.Delegates;
using Game1.Shapes;

namespace Game1.UI
{
    /// <summary>
    /// Useful for cases where would like to replace some HUDElement later on.
    /// Just wrap it in this, and assign a different value when appropriate, and all things should work with that as expected.
    /// </summary>
    [Serializable]
    public sealed class HUDElementWrapper : IHUDElement
    {
        public IHUDElement Value
        {
            get => value;
            set
            {
                var oldShape = this.value.Shape;
                this.value = value;
#warning this is hacky, and likely will not work correctly if the new shape is of a different size than the old one
                // The non-hacky solution would be to basically have 1-element container, so holding my own shape, which depends on the child
                this.value.Shape.Center = oldShape.Center;
            }
        }

        public NearRectangle Shape => Value.Shape;

        public Event<ISizeOrPosChangedListener> SizeOrPosChanged => Value.SizeOrPosChanged;

        public bool Enabled => Value.Enabled;

        public bool PersonallyEnabled { get => Value.PersonallyEnabled; set => Value.PersonallyEnabled = value; }
        public bool HasDisabledAncestor { get => Value.HasDisabledAncestor; set => Value.HasDisabledAncestor = value; }
        public bool MouseOn { get => Value.MouseOn; set => Value.MouseOn = value; }

        public bool CanBeClicked => Value.CanBeClicked;

        public Event<IEnabledChangedListener> EnabledChanged => Value.EnabledChanged;

        /// <summary>
        /// ONLY set this directly at the very start. For all other purposes, use Value;
        /// </summary>
        private IHUDElement value;

        public HUDElementWrapper(IHUDElement value)
            => this.value = value;

        public void RecalcSizeAndPos() => Value.RecalcSizeAndPos();
        public bool Contains(Vector2Bare mouseScreenPos) => Value.Contains(mouseScreenPos);
        public IUIElement? CatchUIElement(Vector2Bare mouseScreenPos) => Value.CatchUIElement(mouseScreenPos);
        public void Draw() => Value.Draw();
    }
}
