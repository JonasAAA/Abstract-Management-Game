using Microsoft.Xna.Framework;
using System.Linq;

namespace Game1.UI
{
    public class UIRectHorizPanel : UIRectPanel
    {
        public override Vector2 TopLeftCorner
        {
            get => base.TopLeftCorner;
            set
            {
                base.TopLeftCorner = value;

                float curWidthSum = 0;
                foreach (var child in children)
                {
                    child.TopLeftCorner = TopLeftCorner + new Vector2(curWidthSum, 0);
                    curWidthSum += child.Width;
                }
            }
        }

        protected override void SetNewChildCoords(UIRectElement child)
            => child.TopLeftCorner = new Vector2(Width, 0);

        protected override void RecalcDimensions()
        {
            Width = children switch
            {
                null => 0,
                not null => children.Sum(child => child.Width)
            };

            Height = children switch
            {
                null => 0,
                not null => children.Max(child => child.Height)
            };
        }
    }
}
