using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public sealed class UIRectVertPanel<TChild> : UIRectPanel<TChild>
        where TChild : IHUDElement
    {
        private readonly HorizPosEnum childHorizPos;

        public UIRectVertPanel(HorizPosEnum childHorizPos)
            => this.childHorizPos = childHorizPos;

        protected sealed override void PartOfRecalcSizeAndPos()
        {
            base.PartOfRecalcSizeAndPos();

            Shape.Width = 2 * ActiveUIManager.RectOutlineWidth + children.MaxOrDefault(child => child.Shape.Width);

            Shape.Height = 2 * ActiveUIManager.RectOutlineWidth + children.Sum(child => child.Shape.Height);

            UDouble curHeightSum = 0;
            PosEnums childOrigin = new(childHorizPos, VertPosEnum.Top);
            foreach (var child in children)
            {
                child.Shape.SetPosition
                (
                    position: Shape.GetPosition(origin: childOrigin)
                        + new MyVector2(-(int)childHorizPos * ActiveUIManager.RectOutlineWidth, curHeightSum + ActiveUIManager.RectOutlineWidth),
                    origin: childOrigin
                );
                curHeightSum += child.Shape.Height;
            }
        }
    }
}
