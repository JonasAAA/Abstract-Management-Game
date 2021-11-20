using Game1.Events;
using Game1.Shapes;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using static Game1.WorldManager;

namespace Game1.UI
{
    [DataContract]
    public class WorldUIElement : UIElement, IDeletable, IChoiceChangedListener<Overlay>
    {
        public IEvent<IDeletedListener> Deleted
            => deleted;
        [DataMember] public readonly Event<IActiveChangedListener> activeChanged;

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
                SetShapeColor();
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

        protected Color ActiveColor
        {
            get => activeColor;
            set
            {
                activeColor = value;
                SetShapeColor();
            }
        }
        protected Color InactiveColor
        {
            get => inactiveColor;
            set
            {
                inactiveColor = value;
                SetShapeColor();
            }
        }

        [DataMember] private Color activeColor, inactiveColor;
        [DataMember] private readonly HorizPos popupHorizPos;
        [DataMember] private readonly VertPos popupVertPos;
        [DataMember] private readonly Event<IDeletedListener> deleted;
        [DataMember] private bool active;

        [DataMember] private readonly Dictionary<Overlay, IHUDElement> popups;

        public WorldUIElement(Shape shape, Color activeColor, Color inactiveColor, HorizPos popupHorizPos, VertPos popupVertPos)
            : base(shape: shape)
        {
            activeChanged = new();
            this.activeColor = activeColor;
            this.inactiveColor = inactiveColor;
            this.popupHorizPos = popupHorizPos;
            this.popupVertPos = popupVertPos;
            active = false;
            SetShapeColor();
            deleted = new();

            popups = Enum.GetValues<Overlay>().ToDictionary
            (
                keySelector: overlay => overlay,
                elementSelector: overlay => (IHUDElement)null
            );
            CurOverlayChanged.Add(listener: this);
        }

        protected void SetPopup(IHUDElement HUDElement, Overlay overlay)
            => popups[overlay] = HUDElement;

        protected void SetPopup(IHUDElement HUDElement, IEnumerable<Overlay> overlays)
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

        private void SetShapeColor()
            => shape.Color = Active switch
            {
                true => activeColor,
                false => inactiveColor
            };

        public virtual void ChoiceChangedResponse(Overlay prevOverlay)
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
