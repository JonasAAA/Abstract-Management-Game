using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game1.UI
{
    public class UIElement<TShape> : UIElement, IUIElement<TShape>
        where TShape : Shape
    {
        public TShape Shape { get; }

        public UIElement(TShape shape, string explanation = defaultExplanation)
            : base(shape: shape, explanation: explanation)
        {
            Shape = shape;
        }
    }

    public class UIElement : IUIElement
    {
        protected const string defaultExplanation = "Explanation missing!";

        public bool Enabled
            => personallyEnabled && !hasDisabledAncestor;

        public bool PersonallyEnabled
        {
            get => personallyEnabled;
            set
            {
                if (personallyEnabled == value)
                    return;

                bool oldEnabled = Enabled;
                personallyEnabled = value;
                if (oldEnabled != Enabled)
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

                bool oldEnabled = Enabled;
                hasDisabledAncestor = value;
                if (oldEnabled != Enabled)
                    EnabledChanged?.Invoke();
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

        public string Explanation { get; }

        public event Action SizeOrPosChanged
        {
            add => shape.SizeOrPosChanged += value;
            remove => shape.SizeOrPosChanged -= value;
        }

        public event Action EnabledChanged, MouseOnChanged;

        protected readonly Shape shape;
        
        private bool personallyEnabled, hasDisabledAncestor, mouseOn, inRecalcSizeAndPos;
        private readonly SortedDictionary<ulong, List<IUIElement>> layerToChildren;
        private readonly Dictionary<IUIElement, ulong> childToLayer;
        
        public UIElement(Shape shape, string explanation = defaultExplanation)
        {
            this.shape = shape;
            Explanation = explanation;
            SizeOrPosChanged += RecalcSizeAndPos;
            personallyEnabled = true;
            MouseOn = false;
            hasDisabledAncestor = false;
            inRecalcSizeAndPos = false;
            layerToChildren = new();
            childToLayer = new();

            EnabledChanged += () =>
            {
                if (Enabled)
                {
                    foreach (var child in Children())
                        child.HasDisabledAncestor = false;
                }
                else
                {
                    MouseOn = false;
                    foreach (var child in Children())
                        child.HasDisabledAncestor = true;
                }
            };
        }

        protected IEnumerable<IUIElement> Children(ulong minLayer = 0, ulong maxLayer = ulong.MaxValue)
            => from childrenLayer in layerToChildren
               where minLayer <= childrenLayer.Key && childrenLayer.Key <= maxLayer
               from child in childrenLayer.Value
               select child;
            //=> from childrenLayer in layerToChildren.Values
            //   from child in childrenLayer
            //   select child;

        protected void AddChild(IUIElement child, ulong layer = 0)
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
            ulong layer = childToLayer[child];
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

            foreach (var child in Children().Reverse())
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
            foreach (var child in Children())
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
            foreach (var child in Children())
                child.Draw();
        }
    }
}
