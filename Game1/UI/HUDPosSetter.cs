using Game1.Delegates;
using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public sealed class HUDPosSetter
    {
        [Serializable]
        private readonly struct HUDElementSizeOrPosChangedListener : ISizeOrPosChangedListener
        {
            private readonly Vector2Bare HUDPos;
            private readonly PosEnums origin;

            public HUDElementSizeOrPosChangedListener(PosEnums position)
            {
                Vector2Bare HUDCenter = new(ActiveUIManager.screenWidth * .5, ActiveUIManager.screenHeight * .5);
                HUDPos = position.GetPosInRect(center: HUDCenter, width: ActiveUIManager.screenWidth, height: ActiveUIManager.screenHeight);
                origin = position;
            }

            public HUDElementSizeOrPosChangedListener(Vector2Bare HUDPos, PosEnums origin)
            {
                this.HUDPos = HUDPos;
                this.origin = origin;
            }

            public void SizeOrPosChangedResponse(Shape shape)
            {
                if (shape is NearRectangle nearRectangle)
                {
                    nearRectangle.SetPosition
                    (
                        position: HUDPos,
                        origin: origin
                    );
                }
                else
                    throw new ArgumentException();
            }
        }

        private readonly Dictionary<IHUDElement, HUDElementSizeOrPosChangedListener> sizeOrPosChangedListeners;

        public HUDPosSetter()
            => sizeOrPosChangedListeners = new();

        public void AddHUDElement(IHUDElement HUDElement, PosEnums position)
            => AddHUDElement
            (
                HUDElement: HUDElement,
                HUDElementSizeOrPosChangedListener: new HUDElementSizeOrPosChangedListener(position: position)
            );

        public void AddHUDElement(IHUDElement HUDElement, Vector2Bare HUDPos, PosEnums origin)
            => AddHUDElement
            (
                HUDElement: HUDElement,
                HUDElementSizeOrPosChangedListener: new HUDElementSizeOrPosChangedListener
                (
                    HUDPos: HUDPos,
                    origin: origin
                )
            );

        private void AddHUDElement(IHUDElement HUDElement, HUDElementSizeOrPosChangedListener HUDElementSizeOrPosChangedListener)
        {
            sizeOrPosChangedListeners[HUDElement] = HUDElementSizeOrPosChangedListener;
            sizeOrPosChangedListeners[HUDElement].SizeOrPosChangedResponse(shape: HUDElement.Shape);
            HUDElement.SizeOrPosChanged.Add(listener: sizeOrPosChangedListeners[HUDElement]);
        }

        public void RemoveHUDElement(IHUDElement HUDElement)
        {
            if (HUDElement is null)
                return;

            HUDElement.SizeOrPosChanged.Remove(listener: sizeOrPosChangedListeners[HUDElement]);
            sizeOrPosChangedListeners.Remove(HUDElement);
        }
    }
}
