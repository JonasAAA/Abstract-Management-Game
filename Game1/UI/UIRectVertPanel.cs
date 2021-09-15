using Microsoft.Xna.Framework;
using System.Linq;

namespace Game1.UI
{
    public class UIRectVertPanel : UIRectPanel
    {
        public UIRectVertPanel(Color color)
            : base(color: color)
            => Shape.CenterChanged += () =>
            {
                float curHeightSum = 0;
                foreach (var child in children)
                {
                    child.Shape.TopLeftCorner = Shape.TopLeftCorner + new Vector2(0, curHeightSum);
                    curHeightSum += child.Shape.Height;
                }
            };

        protected override void SetNewChildCoords(UIElement<MyRectangle> child)
            => child.Shape.TopLeftCorner = Shape.TopLeftCorner + new Vector2(0, Shape.Height);

        protected override void RecalcWidth()
        {
            Shape.SetWidth
            (
                width: children switch
                {
                    null => 0,
                    not null => children.Max(child => child.Shape.Width)
                },
                horizOrigin: MyRectangle.HorizOrigin.Left
            );
        }

        protected override void RecalcHeight()
            => Shape.SetHeight
            (
                height: children switch
                {
                    null => 0,
                    not null => children.Sum(child => child.Shape.Height)
                },
                vertOrigin: MyRectangle.VertOrigin.Top
            );
    }
}
