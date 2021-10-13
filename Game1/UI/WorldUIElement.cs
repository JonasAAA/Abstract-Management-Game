using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using static Game1.WorldManager;

namespace Game1.UI
{
    public class WorldUIElement : UIElement
    {
        public override bool CanBeClicked
            => !Active;

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

        private Color activeColor, inactiveColor;
        private readonly Dictionary<Overlay, IUIElement<NearRectangle>> popup;
        private readonly HorizPos popupHorizPos;
        private readonly VertPos popupVertPos;

        public WorldUIElement(Shape shape, bool active, Color activeColor, Color inactiveColor, HorizPos popupHorizPos, VertPos popupVertPos)
            : base(shape: shape)
        {
            Active = active;
            this.activeColor = activeColor;
            this.inactiveColor = inactiveColor;
            SetShapeColor();
            this.popupHorizPos = popupHorizPos;
            this.popupVertPos = popupVertPos;

            popup = Enum.GetValues<Overlay>().ToDictionary
            (
                keySelector: overlay => overlay,
                elementSelector: overlay => (IUIElement<NearRectangle>)null
            );

            CurOverlayChanged += oldOverlay =>
            {
                if (!Active)
                    return;
                if (popup[oldOverlay] == popup[CurOverlay])
                    return;

                ActiveUI.RemoveUIElement(UIElement: popup[oldOverlay]);
                ActiveUI.AddHUDElement
                (
                    UIElement: popup[CurOverlay],
                    horizPos: popupHorizPos,
                    vertPos: popupVertPos
                );
            };
        }

        protected void SetPopup(IUIElement<NearRectangle> UIElement, Overlay overlay)
            => popup[overlay] = UIElement;

        protected void SetPopup(IUIElement<NearRectangle> UIElement, IEnumerable<Overlay> overlays)
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
            ActiveUI.AddHUDElement
            (
                UIElement: popup[CurOverlay],
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
            ActiveUI.RemoveUIElement
            (
                UIElement: popup[CurOverlay]
            );
        }

        protected void OnDelete()
            => ActiveUI.RemoveUIElement(UIElement: popup[CurOverlay]);

        private void SetShapeColor()
            => shape.Color = Active switch
            {
                true => activeColor,
                false => inactiveColor
            };
    }
}
