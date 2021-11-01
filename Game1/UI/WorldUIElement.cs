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
    public class WorldUIElement : UIElement, ICurOverlayChangedListener
    {
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

        [NonSerialized]
        private Dictionary<Overlay, IHUDElement<NearRectangle>> popup;

        public WorldUIElement(Shape shape, Color activeColor, Color inactiveColor, HorizPos popupHorizPos, VertPos popupVertPos)
            : base(shape: shape)
        {
            this.activeColor = activeColor;
            this.inactiveColor = inactiveColor;
            SetShapeColor();
            this.popupHorizPos = popupHorizPos;
            this.popupVertPos = popupVertPos;
            Active = false;

            popup = Enum.GetValues<Overlay>().ToDictionary
            (
                keySelector: overlay => overlay,
                elementSelector: overlay => (IHUDElement<NearRectangle>)null
            );
        }

        protected override void InitUninitialized()
        {
            base.InitUninitialized();

            //popup = Enum.GetValues<Overlay>().ToDictionary
            //(
            //    keySelector: overlay => overlay,
            //    elementSelector: overlay => (IHUDElement<NearRectangle>)null
            //);

            CurOverlayChanged.Add(listener: this);
        }

        protected void SetPopup(IHUDElement<NearRectangle> UIElement, Overlay overlay)
            => popup[overlay] = UIElement;

        protected void SetPopup(IHUDElement<NearRectangle> UIElement, IEnumerable<Overlay> overlays)
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

        public virtual void OverlayChangedResponse(Overlay oldOverlay)
        {
            if (!Active)
                return;
            if (popup[oldOverlay] == popup[CurOverlay])
                return;

            ActiveUIManager.RemoveHUDElement(HUDElement: popup[oldOverlay]);
            ActiveUIManager.AddHUDElement
            (
                HUDElement: popup[CurOverlay],
                horizPos: popupHorizPos,
                vertPos: popupVertPos
            );
        }
    }
}
