using Microsoft.Xna.Framework;
using System;

namespace Game1.UI
{
    public interface IUIElement<out TShape> : IUIElement
        where TShape : Shape
    {
        public TShape Shape { get; }
    }

    public interface IUIElement
    {
        //this should go to a confing file
        //protected static readonly Color mouseOnColor;

        //static IUIElement()
        //    => mouseOnColor = Color.Yellow;

        public bool Enabled { get; }

        public bool PersonallyEnabled { set; }

        public bool HasDisabledAncestor { set; }

        public bool MouseOn { get; set; }

        public bool CanBeClicked { get; }

        public string Explanation { get; }

        public event Action SizeOrPosChanged, EnabledChanged, MouseOnChanged;

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