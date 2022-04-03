using Game1.PrimitiveTypeWrappers;
using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public class UIRectVertPanel<TChild> : UIRectPanel<TChild>
        where TChild : IHUDElement
    {
        private readonly HorizPos childHorizPos;

        public UIRectVertPanel(Color color, HorizPos childHorizPos)
            : base(color: color)
        {
            this.childHorizPos = childHorizPos;
        }

        protected override void PartOfRecalcSizeAndPos()
        {
            base.PartOfRecalcSizeAndPos();

            Shape.Width = 2 * ActiveUIManager.RectOutlineWidth + children.Count switch
            {
                0 => (UFloat)0,
                not 0 => children.Max(child => child.Shape.Width)
            };
            
            Shape.Height = 2 * ActiveUIManager.RectOutlineWidth + children.Count switch
            {
                0 => 0,
                not 0 => children.Sum(child => child.Shape.Height)
            };

            UFloat curHeightSum = 0;
            foreach (var child in children)
            {
                child.Shape.SetPosition
                (
                    position: Shape.GetPosition(horizOrigin: childHorizPos, vertOrigin: VertPos.Top)
                        + new Vector2(-(int)childHorizPos * ActiveUIManager.RectOutlineWidth, curHeightSum + ActiveUIManager.RectOutlineWidth),
                    horizOrigin: childHorizPos,
                    vertOrigin: VertPos.Top
                );
                curHeightSum += child.Shape.Height;
            }
        }
    }
}
