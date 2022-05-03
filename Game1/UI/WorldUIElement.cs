using Game1.Delegates;
using Game1.Shapes;

using static Game1.WorldManager;

namespace Game1.UI
{
    [Serializable]
    public class WorldUIElement : UIElement<IUIElement>, IDeletable, IChoiceChangedListener<IOverlay>
    {
        public IEvent<IDeletedListener> Deleted
            => deleted;
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
                // TODO: delete
                //SetShapeColor();
                if (active)
                    CurWorldManager.AddHUDElement
                    (
                        HUDElement: popups[CurWorldManager.Overlay],
                        horizPos: popupHorizPos,
                        vertPos: popupVertPos
                    );
                else
                    CurWorldManager.RemoveHUDElement
                    (
                        HUDElement: popups[CurWorldManager.Overlay]
                    );
                activeChanged.Raise(action: listener => listener.ActiveChangedResponse(worldUIElement: this));

                CurWorldManager.ArrowDrawingModeOn = false;
            } 
        }

        // TODO: delete
        //protected Color ActiveColor
        //{
        //    get => activeColor;
        //    set
        //    {
        //        activeColor = value;
        //        SetShapeColor();
        //    }
        //}
        //protected Color InactiveColor
        //{
        //    get => inactiveColor;
        //    set
        //    {
        //        inactiveColor = value;
        //        SetShapeColor();
        //    }
        //}

        // TODO: delete
        //private Color activeColor, inactiveColor;
        private readonly HorizPos popupHorizPos;
        private readonly VertPos popupVertPos;
        private readonly Event<IDeletedListener> deleted;
        private bool active;

        private readonly Dictionary<IOverlay, IHUDElement?> popups;

        public WorldUIElement(Shape shape, HorizPos popupHorizPos, VertPos popupVertPos)
            : base(shape: shape)
        {
            activeChanged = new();
            // TODO: delete
            //this.activeColor = activeColor;
            //this.inactiveColor = inactiveColor;
            this.popupHorizPos = popupHorizPos;
            this.popupVertPos = popupVertPos;
            active = false;
            // TODO: delete
            //SetShapeColor();
            deleted = new();

            popups = IOverlay.all.ToDictionary
            (
                keySelector: overlay => overlay,
                elementSelector: overlay => (IHUDElement?)null
            );
            CurOverlayChanged.Add(listener: this);
        }

        protected void SetPopup(IHUDElement HUDElement, IOverlay overlay)
            => popups[overlay] = HUDElement;

        protected void SetPopup(IHUDElement HUDElement, IEnumerable<IOverlay> overlays)
        {
            foreach (var overlay in overlays)
                SetPopup(HUDElement: HUDElement, overlay: overlay);
        }

        public override void OnClick()
        {
            if (Active)
                return;
            base.OnClick();

            Active = true;
        }

        // TODO: delete
        //private void SetShapeColor()
        //    => shape.Color = Active switch
        //    {
        //        true => activeColor,
        //        false => inactiveColor
        //    };

        public virtual void ChoiceChangedResponse(IOverlay prevOverlay)
        {
            if (!Active)
                return;
            if (popups[prevOverlay] == popups[CurWorldManager.Overlay])
                return;

            CurWorldManager.RemoveHUDElement(HUDElement: popups[prevOverlay]);
            CurWorldManager.AddHUDElement
            (
                HUDElement: popups[CurWorldManager.Overlay],
                horizPos: popupHorizPos,
                vertPos: popupVertPos
            );
        }

        protected virtual void Delete()
        {
            CurOverlayChanged.Remove(listener: this);
            deleted.Raise(action: listener => listener.DeletedResponse(deletable: this));
        }
    }
}
