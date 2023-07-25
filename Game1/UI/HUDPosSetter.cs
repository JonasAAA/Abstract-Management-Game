using Game1.Delegates;
using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public sealed class HUDPosSetter
    {
        //[Serializable]
        //private readonly record struct HUDElementSizeOrPosChangedListener(HorizPos HorizPos, VertPos VertPos) : ISizeOrPosChangedListener
        //{
        //    public void SizeOrPosChangedResponse(Shape shape)
        //    {
        //        if (shape is NearRectangle nearRectangle)
        //        {
        //            MyVector2 HUDCenter = new(ActiveUIManager.screenWidth * .5, ActiveUIManager.screenHeight * .5);
        //            nearRectangle.SetPosition
        //            (
        //                position: HUDCenter + new MyVector2((int)HorizPos * HUDCenter.X, (int)VertPos * HUDCenter.Y),
        //                horizOrigin: HorizPos,
        //                vertOrigin: VertPos
        //            );
        //        }
        //        else
        //            throw new ArgumentException();
        //    }
        //}

        [Serializable]
        private readonly struct HUDElementSizeOrPosChangedListener : ISizeOrPosChangedListener
        {
            private readonly MyVector2 HUDPos;
            private readonly HorizPos horizOrigin;
            private readonly VertPos vertOrigin;

            public HUDElementSizeOrPosChangedListener(HorizPos horizPos, VertPos vertPos)
            {
                MyVector2 HUDCenter = new(ActiveUIManager.screenWidth * .5, ActiveUIManager.screenHeight * .5);
                HUDPos = HUDCenter + new MyVector2((int)horizPos * HUDCenter.X, (int)vertPos * HUDCenter.Y);
                horizOrigin = horizPos;
                vertOrigin = vertPos;
            }

            public HUDElementSizeOrPosChangedListener(MyVector2 HUDPos, HorizPos horizOrigin, VertPos vertOrigin)
            {
                this.HUDPos = HUDPos;
                this.horizOrigin = horizOrigin;
                this.vertOrigin = vertOrigin;
            }

            public void SizeOrPosChangedResponse(Shape shape)
            {
                if (shape is NearRectangle nearRectangle)
                {
                    nearRectangle.SetPosition
                    (
                        position: HUDPos,
                        horizOrigin: horizOrigin,
                        vertOrigin: vertOrigin
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
            => AddHUDElement
            (
                HUDElement: HUDElement,
                HUDElementSizeOrPosChangedListener: new HUDElementSizeOrPosChangedListener
                (
                    horizPos: horizPos,
                    vertPos: vertPos
                )
            );

        public void AddHUDElement(IHUDElement HUDElement, MyVector2 HUDPos, HorizPos horizOrigin, VertPos vertOrigin)
            => AddHUDElement
            (
                HUDElement: HUDElement,
                HUDElementSizeOrPosChangedListener: new HUDElementSizeOrPosChangedListener
                (
                    HUDPos: HUDPos,
                    horizOrigin: horizOrigin,
                    vertOrigin: vertOrigin
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
