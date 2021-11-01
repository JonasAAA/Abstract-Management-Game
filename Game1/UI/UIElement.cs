using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Game1.UI
{
    [DataContract]
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

        [DataMember]
        public string Explanation { get; private init; }

        public event Action SizeOrPosChanged
        {
            add => shape.SizeOrPosChanged += value;
            remove => shape.SizeOrPosChanged -= value;
        }

        [field:NonSerialized]
        public event Action EnabledChanged, MouseOnChanged;

        [DataMember]
        protected readonly Shape shape;
        
        [DataMember]
        private bool personallyEnabled, hasDisabledAncestor, mouseOn, inRecalcSizeAndPos;

        [NonSerialized]
        private SortedDictionary<ulong, List<IUIElement>> layerToChildren;
        [NonSerialized]
        private Dictionary<IUIElement, ulong> childToLayer;

        // DO NOT serialize this
        [NonSerialized]
        private bool initialized;
        
        public UIElement(Shape shape, string explanation = defaultExplanation)
        {
            this.shape = shape;
            Explanation = explanation;
            personallyEnabled = true;
            MouseOn = false;
            hasDisabledAncestor = false;
            inRecalcSizeAndPos = false;
            initialized = false;
            // should be moved to Initialize()
            layerToChildren = new();
            childToLayer = new();
        }

        public void Initialize()
        {
            //layerToChildren = new();
            //childToLayer = new();
            foreach (var child in Children().Clone())
            {
                child.Initialize();
                SubscribeToChildEvents(child: child);
            }

            if (!initialized)
                InitUninitialized();

            initialized = true;
        }

        protected virtual void InitUninitialized()
        {
            if (initialized)
                throw new InvalidOperationException();

            SizeOrPosChanged += RecalcSizeAndPos;
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


        protected void AddChild(IUIElement child, ulong layer = 0)
        {
            if (!layerToChildren.ContainsKey(layer))
                layerToChildren[layer] = new();
            layerToChildren[layer].Add(child);
            childToLayer.Add(child, layer);
            SubscribeToChildEvents(child: child);
            RecalcSizeAndPos();
        }

        private void SubscribeToChildEvents(IUIElement child)
        {
            child.SizeOrPosChanged -= RecalcSizeAndPos;
            child.SizeOrPosChanged += RecalcSizeAndPos;
        }

        protected void RemoveChild(IUIElement child)
        {
            if (!initialized)
                throw new InvalidOperationException();
            ulong layer = childToLayer[child];
            child.SizeOrPosChanged -= RecalcSizeAndPos;
            if (!layerToChildren[layer].Remove(child) || !childToLayer.Remove(child))
                throw new ArgumentException();
            RecalcSizeAndPos();
        }

        public bool Contains(Vector2 position)
        {
            if (!initialized)
                throw new InvalidOperationException();
            return shape.Contains(position: position);
        }

        public virtual IUIElement CatchUIElement(Vector2 mousePos)
        {
            if (!initialized)
                throw new InvalidOperationException();

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
            if (!initialized)
                throw new InvalidOperationException();
            if (!CanBeClicked)
                throw new InvalidOperationException();
        }

        public virtual void OnMouseDownWorldNotMe()
        {
            if (!initialized)
                throw new InvalidOperationException();
        }

        public virtual void Draw()
        {
            if (!initialized)
                throw new InvalidOperationException();

            shape.Draw
            (
                otherColor: ActiveUIManager.UIConfig.mouseOnColor,
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
