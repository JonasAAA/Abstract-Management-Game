using Game1.Events;
using Microsoft.Xna.Framework;

namespace Game1.UI
{
    public interface IUIElement
    {
        public bool Enabled { get; }

        public bool PersonallyEnabled { set; }

        public bool HasDisabledAncestor { set; }

        public bool MouseOn { get; set; }

        public bool CanBeClicked { get; }

        public string Explanation { get; }

        public Event<IEnabledChangedListener> EnabledChanged { get; }

        public bool Contains(Vector2 position);

        public IUIElement CatchUIElement(Vector2 mousePos);

        public virtual void OnClick()
        { }

        public void Draw();
    }
}