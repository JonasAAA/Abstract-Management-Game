using Microsoft.Xna.Framework;
using System.Linq;
using System.Runtime.Serialization;
using static Game1.UI.ActiveUIManager;

namespace Game1.UI
{
    [DataContract]
    public class UIRectVertPanel<TChild> : UIRectPanel<TChild>
        where TChild : IHUDElement
    {
        [DataMember] private readonly HorizPos childHorizPos;

        public UIRectVertPanel(Color color, HorizPos childHorizPos)
            : base(color: color)
        {
            this.childHorizPos = childHorizPos;
        }

        protected override void PartOfRecalcSizeAndPos()
        {
            base.PartOfRecalcSizeAndPos();

            Shape.Width = 2 * CurUIConfig.rectOutlineWidth + children.Count switch
            {
                0 => 0,
                not 0 => children.Max(child => child.Shape.Width)
            };
            
            Shape.Height = 2 * CurUIConfig.rectOutlineWidth + children.Count switch
            {
                0 => 0,
                not 0 => children.Sum(child => child.Shape.Height)
            };

            float curHeightSum = 0;
            foreach (var child in children)
            {
                child.Shape.SetPosition
                (
                    position: Shape.GetPosition(horizOrigin: childHorizPos, vertOrigin: VertPos.Top)
                        + new Vector2(-(int)childHorizPos * CurUIConfig.rectOutlineWidth, curHeightSum + CurUIConfig.rectOutlineWidth),
                    horizOrigin: childHorizPos,
                    vertOrigin: VertPos.Top
                );
                curHeightSum += child.Shape.Height;
            }
        }
    }
}
