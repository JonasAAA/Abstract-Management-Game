using Game1.Events;
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

        public override bool CanBeClicked
            => !Active;

        [DataMember]
        protected bool Active { get; private set; }
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

        [DataMember]
        private Color activeColor, inactiveColor;
        [DataMember]
        private readonly HorizPos popupHorizPos;
        [DataMember]
        private readonly VertPos popupVertPos;
        [DataMember]
        private readonly Event<IDeletedListener> deleted;

        [NonSerialized]
        private readonly Dictionary<Overlay, IHUDElement> popup;

        public WorldUIElement(Shape shape, Color activeColor, Color inactiveColor, HorizPos popupHorizPos, VertPos popupVertPos)
            : base(shape: shape)
        {
            this.activeColor = activeColor;
            this.inactiveColor = inactiveColor;
            SetShapeColor();
            this.popupHorizPos = popupHorizPos;
            this.popupVertPos = popupVertPos;
            Active = false;
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
            SetShapeColor();
            ActiveUIManager.AddHUDElement
            (
                HUDElement: popup[CurOverlay],
                horizPos: popupHorizPos,
                vertPos: popupVertPos
            );
        }

        public override void OnMouseDownWorldNotMe()
        {
            if (!Active)
                return;
            base.OnMouseDownWorldNotMe();

            Active = false;
            SetShapeColor();
            ActiveUIManager.RemoveHUDElement
            (
                HUDElement: popup[CurOverlay]
            );
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
            if (popup[prevOverlay] == popup[CurOverlay])
                return;

            ActiveUIManager.RemoveHUDElement(HUDElement: popup[prevOverlay]);
            ActiveUIManager.AddHUDElement
            (
                HUDElement: popup[CurOverlay],
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
