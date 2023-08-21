using Game1.Delegates;
using Game1.Shapes;

using static Game1.WorldManager;

namespace Game1.UI
{
    [Serializable]
    // TChild is WorldUIElement to disallow text and similar UI elements which would should not scale when player zooms in/out
    // The correct approach here would be to have TChild IWorldUIElement (that being new interface) but I'm not sure if that'll work
    // with the save system (as UIElement<IUIElement> and UIElement<IWorldUIElement> would be indistinguishable types for the save system
    public abstract class WorldUIElement : UIElement<WorldUIElement>
        //, IChoiceChangedListener<IOverlay>
    {
        public readonly Event<IActiveChangedListener> activeChanged;

        public override bool CanBeClicked
            => !Active;

        public bool Active
        {
            get => active;
            set
            {
                if (active == value)
                    return;

                active = value;
                if (active)
                    CurWorldManager.AddHUDElement
                    (
                        HUDElement: Popup,
                        position: popupPos
                    );
                else
                    CurWorldManager.RemoveHUDElement
                    (
                        HUDElement: Popup
                    );
                //if (active)
                //    CurWorldManager.AddHUDElement
                //    (
                //        HUDElement: popups[CurWorldManager.Overlay],
                //        horizPos: popupHorizPos,
                //        vertPos: popupVertPos
                //    );
                //else
                //    CurWorldManager.RemoveHUDElement
                //    (
                //        HUDElement: popups[CurWorldManager.Overlay]
                //    );
                activeChanged.Raise(action: listener => listener.ActiveChangedResponse(worldUIElement: this));

                //CurWorldManager.ArrowDrawingModeRes = null;
            }
        }

        protected sealed override Color Color
            => Active switch
            {
                true => activeColor,
                false => inactiveColor
            };
        protected Color activeColor, inactiveColor;

        private readonly PosEnums popupPos;
        
        private bool active;

        protected IHUDElement? Popup { get; init; }
        //private readonly MyDict<IOverlay, IHUDElement?> popups;

        public WorldUIElement(Shape shape, Color activeColor, Color inactiveColor, PosEnums popupPos)
            : base(shape: shape)
        {
            activeChanged = new();
            this.activeColor = activeColor;
            this.inactiveColor = inactiveColor;
            this.popupPos = popupPos;
            active = false;
            //popups = IOverlay.all.ToDictionary
            //(
            //    elementSelector: overlay => (IHUDElement?)null
            //);
            //CurOverlayChanged.Add(listener: this);
        }

        //protected void SetPopup(IHUDElement HUDElement, IOverlay overlay)
        //    => popups[overlay] = HUDElement;

        //protected void SetPopup(IHUDElement HUDElement, IEnumerable<IOverlay> overlays)
        //{
        //    foreach (var overlay in overlays)
        //        SetPopup(HUDElement: HUDElement, overlay: overlay);
        //}

        public override void OnClick()
        {
            if (Active)
                return;
            base.OnClick();

            Active = true;
        }

        //public virtual void ChoiceChangedResponse(IOverlay prevOverlay)
        //{
        //    if (!Active)
        //        return;
        //    if (popups[prevOverlay] == popups[CurWorldManager.Overlay])
        //        return;

        //    CurWorldManager.RemoveHUDElement(HUDElement: popups[prevOverlay]);
        //    CurWorldManager.AddHUDElement
        //    (
        //        HUDElement: popups[CurWorldManager.Overlay],
        //        horizPos: popupHorizPos,
        //        vertPos: popupVertPos
        //    );
        //}

        protected virtual void Delete()
        { }
            //=> CurOverlayChanged.Remove(listener: this);
    }
}
