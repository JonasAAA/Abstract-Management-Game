using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public sealed class UIRectHorizPanel<TChild> : UIRectPanel<TChild>
        where TChild : IHUDElement
    {
        private readonly VertPos childVertPos;

        public UIRectHorizPanel(VertPos childVertPos)
            => this.childVertPos = childVertPos;

        protected override void PartOfRecalcSizeAndPos()
        {
            base.PartOfRecalcSizeAndPos();

            Shape.Width = 2 * ActiveUIManager.RectOutlineWidth + children.Sum(child => child.Shape.Width);

            Shape.Height = 2 * ActiveUIManager.RectOutlineWidth + children.MaxOrDefault(child => child.Shape.Height);

            UDouble curWidthSum = 0;
            foreach (var child in children)
            {
                child.Shape.SetPosition
                (
                    position: Shape.GetPosition(horizOrigin: HorizPos.Left, vertOrigin: childVertPos)
                        + new MyVector2(ActiveUIManager.RectOutlineWidth + curWidthSum, -(int)childVertPos * ActiveUIManager.RectOutlineWidth),
                    horizOrigin: HorizPos.Left,
                    vertOrigin: childVertPos
                );
                curWidthSum += child.Shape.Width;
            }
        }
    }
}
