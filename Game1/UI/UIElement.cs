using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game1.UI
{
    public abstract class UIElement<TShape> : UIElement, IUIElement<TShape>
        where TShape : Shape
    {
        public TShape Shape { get; protected init; }

        protected UIElement()
            : this(shape: null)
        { }

        protected UIElement(TShape shape)
            => Shape = shape;

        protected override Shape GetShape()
            => Shape;
    }

    public abstract class UIElement : IUIElement
    {
        public bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled == value)
                    return;
                
                enabled = value;
                SetHasDisabledParentOfChildren();
                EnabledChanged?.Invoke();
            }
        }

        public bool HasDisabledAncestor
        {
            get => hasDisabledAncestor;
            set
            {
                if (hasDisabledAncestor == value)
                    return;

                hasDisabledAncestor = value;
                SetHasDisabledParentOfChildren();
                HasDisabledAncestorChanged?.Invoke();
            }
        }

        public bool MouseOn
        {
            get => mouseOn;
            set
            {
                if (mouseOn == value)
                    return;

                mouseOn = value;
                MouseOnChanged?.Invoke();
            }
        }

        public virtual bool CanBeClicked
            => false;

        public event Action EnabledChanged, HasDisabledAncestorChanged, MouseOnChanged;

        private bool enabled, hasDisabledAncestor, mouseOn;

        protected UIElement()
        {
            enabled = true;
            MouseOn = false;
            hasDisabledAncestor = false;
        }

        protected abstract Shape GetShape();

        protected virtual IEnumerable<IUIElement> GetChildren()
            => Enumerable.Empty<IUIElement>();

        public bool Contains(Vector2 position)
            => GetShape().Contains(position: position);

        public virtual IUIElement CatchUIElement(Vector2 mousePos)
        {
            if (!Contains(position: mousePos))
                return null;

            foreach (var child in GetChildren().Reverse())
            {
                var childCatchingUIElement = child.CatchUIElement(mousePos: mousePos);
                if (childCatchingUIElement is not null)
                    return childCatchingUIElement;
            }
            return GetShape().Transparent switch
            {
                true => null,
                false => this
            };
        }

        public virtual void OnClick()
        {
            if (!CanBeClicked)
                throw new InvalidOperationException();
        }

        public virtual void OnMouseDownWorldNotMe()
        { }

        public virtual void Draw()
        {
            GetShape().Draw
            (
                otherColor: IUIElement.mouseOnColor,
                otherColorProp: (CanBeClicked && MouseOn) switch
                {
                    true => .5f,
                    false => 0
                }
            );
            foreach (var child in GetChildren())
                child.Draw();
        }

        private void SetHasDisabledParentOfChildren()
        {
            if (!enabled || hasDisabledAncestor)
            {
                foreach (var child in GetChildren())
                    child.HasDisabledAncestor = true;
            }
            else
            {
                foreach (var child in GetChildren())
                    child.HasDisabledAncestor = false;
            }
        }
    }
}
