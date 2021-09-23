using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game1.UI
{
    public abstract class UIElement<TShape> : UIElement, IUIElement<TShape>
        where TShape : Shape
    {
        public TShape Shape { get; }

        protected UIElement(TShape shape)
            : base(shape: shape)
            => Shape = shape;
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

        public event Action SizeOrPosChanged
        {
            add => shape.SizeOrPosChanged += value;
            remove => shape.SizeOrPosChanged -= value;
        }

        public event Action EnabledChanged, HasDisabledAncestorChanged,MouseOnChanged;

        private readonly Shape shape;
        private bool enabled, hasDisabledAncestor, mouseOn, inRecalcSizeAndPos;
        private readonly SortedDictionary<int, List<IUIElement>> layerToChildren;
        private readonly Dictionary<IUIElement, int> childToLayer;
        private IEnumerable<IUIElement> Children
            => from childrenLayer in layerToChildren.Values
               from child in childrenLayer
               select child;

        protected UIElement(Shape shape)
        {
            this.shape = shape;
            SizeOrPosChanged += RecalcSizeAndPos;
            enabled = true;
            MouseOn = false;
            hasDisabledAncestor = false;
            inRecalcSizeAndPos = false;
            layerToChildren = new();
            childToLayer = new();
        }

        protected void AddChild(IUIElement child, int layer = 0)
        {
            child.SizeOrPosChanged += RecalcSizeAndPos;
            if (!layerToChildren.ContainsKey(layer))
                layerToChildren[layer] = new();
            layerToChildren[layer].Add(child);
            childToLayer.Add(child, layer);
            RecalcSizeAndPos();
        }

        protected void RemoveChild(IUIElement child)
        {
            int layer = childToLayer[child];
            child.SizeOrPosChanged -= RecalcSizeAndPos;
            if (!layerToChildren[layer].Remove(child) || !childToLayer.Remove(child))
                throw new ArgumentException();
            RecalcSizeAndPos();
        }

        public bool Contains(Vector2 position)
            => shape.Contains(position: position);

        public virtual IUIElement CatchUIElement(Vector2 mousePos)
        {
            if (!Contains(position: mousePos))
                return null;

            foreach (var child in Enumerable.Reverse(Children))
            {
                var childCatchingUIElement = child.CatchUIElement(mousePos: mousePos);
                if (childCatchingUIElement is not null)
                    return childCatchingUIElement;
            }
            return shape.Transparent switch
            {
                true => null,
                false => this
            };
        }

        public void RecalcSizeAndPos()
        {
            if (inRecalcSizeAndPos)
                return;
            inRecalcSizeAndPos = true;

            PartOfRecalcSizeAndPos();
            foreach (var child in Children)
                child.RecalcSizeAndPos();

            inRecalcSizeAndPos = false;
        }

        protected virtual void PartOfRecalcSizeAndPos()
        {
            if (!inRecalcSizeAndPos)
                throw new InvalidOperationException();
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
            shape.Draw
            (
                otherColor: IUIElement.mouseOnColor,
                otherColorProp: (CanBeClicked && MouseOn) switch
                {
                    true => .5f,
                    false => 0
                }
            );
            foreach (var child in Children)
                child.Draw();
        }

        private void SetHasDisabledParentOfChildren()
        {
            if (!enabled || hasDisabledAncestor)
            {
                foreach (var child in Children)
                    child.HasDisabledAncestor = true;
            }
            else
            {
                foreach (var child in Children)
                    child.HasDisabledAncestor = false;
            }
        }
    }
}
