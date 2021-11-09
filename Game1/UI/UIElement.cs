using Game1.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using static Game1.UI.ActiveUIManager;

namespace Game1.UI
{
    [DataContract]
    public class UIElement : IUIElement, ISizeOrPosChangedListener
    {
        protected const string defaultExplanation = "Explanation missing!";

        public Event<ISizeOrPosChangedListener> SizeOrPosChanged
            => shape.SizeOrPosChanged;
        [DataMember] public Event<IEnabledChangedListener> EnabledChanged { get; private init; }

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
                    MyEnabledChangedResponse();
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
                    MyEnabledChangedResponse();
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
            }
        }

        public virtual bool CanBeClicked
            => false;

        [DataMember] public string Explanation { get; private init; }

        [DataMember] protected readonly Shape shape;
        
        [DataMember] private bool personallyEnabled, hasDisabledAncestor, mouseOn, inRecalcSizeAndPos;

        [DataMember] private readonly SortedDictionary<ulong, List<IUIElement>> layerToChildren;
        [DataMember] private readonly Dictionary<IUIElement, ulong> childToLayer;

        public UIElement(Shape shape, string explanation = defaultExplanation)
        {
            EnabledChanged = new();
            this.shape = shape;
            Explanation = explanation;
            personallyEnabled = true;
            MouseOn = false;
            hasDisabledAncestor = false;
            inRecalcSizeAndPos = false;
            layerToChildren = new();
            childToLayer = new();

            SizeOrPosChanged.Add(listener: this);
        }

        protected IEnumerable<IUIElement> Children(ulong minLayer = 0, ulong maxLayer = ulong.MaxValue)
            => from childrenLayer in layerToChildren
               where minLayer <= childrenLayer.Key && childrenLayer.Key <= maxLayer
               from child in childrenLayer.Value
               select child;

        protected void AddChild(IUIElement child, ulong layer = 0)
        {
            if (!layerToChildren.ContainsKey(layer))
                layerToChildren[layer] = new();
            layerToChildren[layer].Add(child);
            childToLayer.Add(child, layer);
            child.SizeOrPosChanged.Add(listener: this);
            RecalcSizeAndPos();
        }

        protected void RemoveChild(IUIElement child)
        {
            ulong layer = childToLayer[child];
            child.SizeOrPosChanged.Remove(listener: this);
            if (!layerToChildren[layer].Remove(child) || !childToLayer.Remove(child))
                throw new ArgumentException();
            if (layerToChildren[layer].Count is 0)
                layerToChildren.Remove(layer);
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
                otherColor: CurUIConfig.mouseOnColor,
                otherColorProp: (CanBeClicked && MouseOn) switch
                {
                    true => .5f,
                    false => 0
                }
            );
            foreach (var child in Children())
                child.Draw();
        }

        public virtual void SizeOrPosChangedResponse(Shape shape)
            => RecalcSizeAndPos();

        private void MyEnabledChangedResponse()
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
            EnabledChanged.Raise(action: listener => listener.EnabledChangedResponse());
        }
    }
}
