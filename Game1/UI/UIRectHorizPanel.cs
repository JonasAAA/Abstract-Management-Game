using Microsoft.Xna.Framework;
using System.Linq;

namespace Game1.UI
{
    public class UIRectHorizPanel : UIRectPanel
    {
        public UIRectHorizPanel(Color color)
            : base(color: color)
            => Shape.CenterChanged += () =>
            {
                float curWidthSum = 0;
                foreach (var child in children)
                {
                    child.Shape.TopLeftCorner = Shape.TopLeftCorner + new Vector2(curWidthSum, 0);
                    curWidthSum += child.Shape.Width;
                }
            };

        protected override void SetNewChildCoords(UIElement<MyRectangle> child)
            => child.Shape.TopLeftCorner = new Vector2(Shape.Width, 0);

        protected override void RecalcWidth()
            => Shape.SetWidth
            (
                width: children switch
                {
                    null => 0,
                    not null => children.Sum(child => child.Shape.Width)
                },
                horizOrigin: MyRectangle.HorizOrigin.Left
            );

        protected override void RecalcHeight()
            => Shape.SetHeight
            (
                height: children switch
                {
                    null => 0,
                    not null => children.Max(child => child.Shape.Height)
                },
                vertOrigin: MyRectangle.VertOrigin.Top
            );
    }
}
