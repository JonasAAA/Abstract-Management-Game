using Game1.Shapes;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Runtime.Serialization;

namespace Game1.UI
{
    [DataContract]
    public class UIRectHorizPanel<TChild> : UIRectPanel<TChild>
        where TChild : IHUDElement
    {
        [DataMember] private readonly VertPos childVertPos;

        public UIRectHorizPanel(Color color, VertPos childVertPos)
            : base(color: color)
        {
            this.childVertPos = childVertPos;
        }

        protected override void PartOfRecalcSizeAndPos()
        {
            base.PartOfRecalcSizeAndPos();

            Shape.Width = 2 * ActiveUIManager.RectOutlineWidth + children.Count switch
            {
                0 => 0,
                not 0 => children.Sum(child => child.Shape.Width)
            };

            Shape.Height = 2 * ActiveUIManager.RectOutlineWidth + children.Count switch
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
                        + new Vector2(ActiveUIManager.RectOutlineWidth + curWidthSum, -(int)childVertPos * ActiveUIManager.RectOutlineWidth),
                    horizOrigin: HorizPos.Left,
                    vertOrigin: childVertPos
                );
                curWidthSum += child.Shape.Width;
            }
        }
    }
}
