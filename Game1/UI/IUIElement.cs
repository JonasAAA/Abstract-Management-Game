using Microsoft.Xna.Framework;
using System;

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

        public event Action SizeOrPosChanged, EnabledChanged, MouseOnChanged;

        public void Initialize();

        public bool Contains(Vector2 position);

        public IUIElement CatchUIElement(Vector2 mousePos);

        public void RecalcSizeAndPos();

        public virtual void OnClick()
        { }

        public virtual void OnMouseDownWorldNotMe()
        { }

        public void Draw();
    }
}