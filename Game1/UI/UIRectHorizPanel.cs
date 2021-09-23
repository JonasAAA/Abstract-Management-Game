using Microsoft.Xna.Framework;
using System.Linq;

namespace Game1.UI
{
    public class UIRectHorizPanel<TChild> : UIRectPanel<TChild>
        where TChild : IUIElement<NearRectangle>
    {
        public UIRectHorizPanel(Color color)
            : base(color: color)
        { }

        protected override void PartOfRecalcSizeAndPos()
        {
            base.PartOfRecalcSizeAndPos();

            Shape.Width = 2 * MyRectangle.outlineWidth + children.Count switch
            {
                0 => 0,
                not 0 => children.Sum(child => child.Shape.Width)
            };

            Shape.Height = 2 * MyRectangle.outlineWidth + children.Count switch
            {
                0 => 0,
                not 0 => children.Max(child => child.Shape.Height)
            };

            float curWidthSum = 0;
            foreach (var child in children)
            {
                child.Shape.TopLeftCorner = Shape.TopLeftCorner + new Vector2(curWidthSum + MyRectangle.outlineWidth, MyRectangle.outlineWidth);
                curWidthSum += child.Shape.Width;
            }
        }
    }
}
