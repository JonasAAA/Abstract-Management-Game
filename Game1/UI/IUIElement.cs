using Game1.Delegates;

namespace Game1.UI
{
    public interface IUIElement
    {
        public bool Enabled { get; }

        public bool PersonallyEnabled { set; }

        public bool HasDisabledAncestor { set; }

        public bool MouseOn { get; set; }

        public bool CanBeClicked { get; }

        public Event<IEnabledChangedListener> EnabledChanged { get; }

        public bool Contains(Vector2Bare mouseScreenPos);

        public IUIElement? CatchUIElement(Vector2Bare mouseScreenPos);

        public virtual void OnClick()
        { }

        public void Draw();
    }
}