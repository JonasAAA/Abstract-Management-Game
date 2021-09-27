using Microsoft.Xna.Framework;
using System.Linq;

namespace Game1.UI
{
    public class UIRectHorizPanel<TChild> : UIRectPanel<TChild>
        where TChild : IUIElement<NearRectangle>
    {
        private readonly VertPos childVertPos;

        public UIRectHorizPanel(Color color, VertPos childVertPos)
            : base(color: color)
        {
            this.childVertPos = childVertPos;
        }

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
                child.Shape.SetPosition
                (
                    position: Shape.GetPosition(horizOrigin: HorizPos.Left, vertOrigin: childVertPos)
                        + new Vector2(MyRectangle.outlineWidth + curWidthSum, -(int)childVertPos * MyRectangle.outlineWidth),
                    horizOrigin: HorizPos.Left,
                    vertOrigin: childVertPos
                );
                curWidthSum += child.Shape.Width;
            }
        }
    }
}
