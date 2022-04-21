using Game1.Delegates;
using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public class HUDPosSetter
    {
        [Serializable]
        private readonly record struct HUDElementSizeOrPosChangedListener(HorizPos HorizPos, VertPos VertPos) : ISizeOrPosChangedListener
        {
            public void SizeOrPosChangedResponse(Shape shape)
            {
                if (shape is NearRectangle nearRectangle)
                {
                    MyVector2 HUDCenter = new(ActiveUIManager.screenWidth * .5, ActiveUIManager.screenHeight * .5);
                    nearRectangle.SetPosition
                    (
                        position: HUDCenter + new MyVector2((int)HorizPos * HUDCenter.X, (int)VertPos * HUDCenter.Y),
                        horizOrigin: HorizPos,
                        vertOrigin: VertPos
                    );
                }
                else
                    throw new ArgumentException();
            }
        }

        private readonly Dictionary<IHUDElement, HUDElementSizeOrPosChangedListener> sizeOrPosChangedListeners;

        public HUDPosSetter()
            => sizeOrPosChangedListeners = new();

        public void AddHUDElement(IHUDElement HUDElement, HorizPos horizPos, VertPos vertPos)
        {
            if (HUDElement is null)
                return;

            sizeOrPosChangedListeners[HUDElement] = new HUDElementSizeOrPosChangedListener(HorizPos: horizPos, VertPos: vertPos);
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
