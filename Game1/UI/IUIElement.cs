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

        public bool Contains(MyVector2 position);

        public IUIElement? CatchUIElement(MyVector2 mousePos);

        public virtual void OnClick()
        { }

        public void Draw();
    }
}