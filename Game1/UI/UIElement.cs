using Game1.Delegates;
using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public class UIElement<TChild> : IUIElement
        where TChild : IUIElement
    {
        protected const string defaultExplanation = "Explanation missing!";

        public Event<IEnabledChangedListener> EnabledChanged { get; }

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

        public string Explanation { get; }

        protected readonly Shape shape;
        
        private bool personallyEnabled, hasDisabledAncestor, mouseOn;

        private readonly SortedDictionary<ulong, List<TChild>> layerToChildren;
        private readonly Dictionary<TChild, ulong> childToLayer;

        public UIElement(Shape shape, string explanation = defaultExplanation)
        {
            EnabledChanged = new();
            this.shape = shape;
            Explanation = explanation;
            personallyEnabled = true;
            MouseOn = false;
            hasDisabledAncestor = false;
            layerToChildren = new();
            childToLayer = new();
        }

        protected IEnumerable<TChild> Children(ulong minLayer = 0, ulong maxLayer = ulong.MaxValue)
            => from childrenLayer in layerToChildren
               where minLayer <= childrenLayer.Key && childrenLayer.Key <= maxLayer
               from child in childrenLayer.Value
               select child;

        protected virtual void AddChild(TChild child, ulong layer = 0)
        {
            if (!layerToChildren.ContainsKey(layer))
                layerToChildren[layer] = new();
            layerToChildren[layer].Add(child);
            childToLayer.Add(child, layer);
            // TODO
            //later would probably need to add some reaction to position changes to WorldUIElement
            //(maybe WorldUIElements use only shapes which have their positions defined by something like IReadOnlyChangingFloat)
            //child.SizeOrPosChanged.Add(listener: this);
        }

        protected virtual void RemoveChild(TChild child)
        {
            ulong layer = childToLayer[child];
            if (!layerToChildren[layer].Remove(child) || !childToLayer.Remove(child))
                throw new ArgumentException();
            if (layerToChildren[layer].Count is 0)
                layerToChildren.Remove(layer);
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

        public virtual void OnClick()
        {
            if (!CanBeClicked)
                throw new InvalidOperationException();
        }

        public virtual void Draw()
        {
            shape.Draw
            (
                otherColor: ActiveUIManager.CurUIConfig.mouseOnColor,
                otherColorProp: (CanBeClicked && MouseOn) switch
                {
                    true => .5f,
                    false => 0
                }
            );
            foreach (var child in Children())
                child.Draw();
        }

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
