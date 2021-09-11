using Microsoft.Xna.Framework;
using System.Linq;

namespace Game1.UI
{
    public class UIVertPanel : UIPanel
    {
        public override Vector2 TopLeftCorner
        {
            get => base.TopLeftCorner;
            set
            {
                base.TopLeftCorner = value;

                float curHeightSum = 0;
                foreach (var child in children)
                {
                    child.TopLeftCorner = TopLeftCorner + new Vector2(0, curHeightSum);
                    curHeightSum += child.Height;
                }
            }
        }

        protected override void RecalcDimensions()
        {
            Width = children switch
            {
                null => 0,
                not null => children.Max(child => child.Width)
            };

            Height = children switch
            {
                null => 0,
                not null => children.Sum(child => child.Height)
            };
        }
    }
}
