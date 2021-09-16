using Microsoft.Xna.Framework;
using System.Linq;

namespace Game1.UI
{
    public class UIRectHorizPanel<TChild> : UIRectPanel<TChild>
        where TChild : UIElement<MyRectangle>
    {
        public UIRectHorizPanel(Color color)
            : base(color: color)
        { }

        protected override void SetNewChildCoords(TChild child)
            => child.Shape.TopLeftCorner = new Vector2(Shape.Width - MyRectangle.outlineWidth, MyRectangle.outlineWidth);

        protected override void RecalcChildrenPos()
        {
            float curWidthSum = 0;
            foreach (var child in children)
            {
                child.Shape.TopLeftCorner = Shape.TopLeftCorner + new Vector2(curWidthSum + MyRectangle.outlineWidth, MyRectangle.outlineWidth);
                curWidthSum += child.Shape.Width;
            }
        }

        protected override void RecalcWidth()
            => Shape.Width = 2 * MyRectangle.outlineWidth + children.Count switch
            {
                0 => 0,
                not 0 => children.Sum(child => child.Shape.Width)
            };

        protected override void RecalcHeight()
            => Shape.Height = 2 * MyRectangle.outlineWidth + children.Count switch
            {
                0 => 0,
                not 0 => children.Max(child => child.Shape.Height)
            };
    }
}
