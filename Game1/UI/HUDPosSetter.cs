using Game1.Events;
using Game1.Shapes;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game1.UI
{
    [DataContract]
    public class HUDPosSetter
    {
        [DataContract]
        private record HUDElementSizeOrPosChangedListener([property: DataMember] HorizPos HorizPos, [property: DataMember] VertPos VertPos) : ISizeOrPosChangedListener
        {
            public void SizeOrPosChangedResponse(Shape shape)
            {
                if (shape is NearRectangle nearRectangle)
                {
                    Vector2 HUDCenter = new((float)(ActiveUIManager.ScreenWidth * .5), (float)(ActiveUIManager.ScreenHeight * .5));
                    nearRectangle.SetPosition
                    (
                        position: HUDCenter + new Vector2((int)HorizPos * HUDCenter.X, (int)VertPos * HUDCenter.Y),
                        horizOrigin: HorizPos,
                        vertOrigin: VertPos
                    );
                }
                else
                    throw new ArgumentException();
            }
        }

        [DataMember] private readonly Dictionary<IHUDElement, HUDElementSizeOrPosChangedListener> sizeOrPosChangedListeners;

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
