using Game1.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using static Game1.WorldManager;
using static Game1.UI.ActiveUIManager;

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
                        HUDElement: popup[CurWorldManager.Overlay],
                        horizPos: popupHorizPos,
                        vertPos: popupVertPos
                    );
                else
                    CurWorldManager.RemoveHUDElement
                    (
                        HUDElement: popup[CurWorldManager.Overlay]
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

        [DataMember] private readonly Dictionary<Overlay, IHUDElement> popup;

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

            popup = Enum.GetValues<Overlay>().ToDictionary
            (
                keySelector: overlay => overlay,
                elementSelector: overlay => (IHUDElement)null
            );
            CurOverlayChanged.Add(listener: this);
        }

        protected void SetPopup(IHUDElement UIElement, Overlay overlay)
            => popup[overlay] = UIElement;

        protected void SetPopup(IHUDElement UIElement, IEnumerable<Overlay> overlays)
        {
            foreach (var overlay in overlays)
                SetPopup(UIElement: UIElement, overlay: overlay);
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
            if (popup[prevOverlay] == popup[CurWorldManager.Overlay])
                return;

            CurWorldManager.RemoveHUDElement(HUDElement: popup[prevOverlay]);
            CurWorldManager.AddHUDElement
            (
                HUDElement: popup[CurWorldManager.Overlay],
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
