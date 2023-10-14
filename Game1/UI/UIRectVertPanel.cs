using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public sealed class UIRectVertPanel<TChild> : UIRectPanel<TChild>
        where TChild : IHUDElement
    {
        private readonly HorizPosEnum childHorizPos;
        private readonly UDouble gap;

        /// <summary>
        /// null gap means default gap
        /// </summary>
        public UIRectVertPanel(HorizPosEnum childHorizPos, IEnumerable<TChild?> children, UDouble? gap = default)
        {
            this.childHorizPos = childHorizPos;
            this.gap = gap ?? ActiveUIManager.DefaultGapBetweenUIElements;
            // This most be done after setting the gap, otherwise, when adding the children, incorrect gap will be used.
            AddChildren(newChildren: children);
        }

        protected sealed override void PartOfRecalcSizeAndPos()
        {
            base.PartOfRecalcSizeAndPos();

            Shape.Width = 2 * ActiveUIManager.RectOutlineWidth + children.MaxOrDefault(child => child.Shape.Width);

            Shape.Height = 2 * ActiveUIManager.RectOutlineWidth + children.Sum(child => child.Shape.Height) + gap * UDouble.CreateByClamp(children.Count - 1);

            UDouble curHeightSum = 0;
            PosEnums childOrigin = new(childHorizPos, VertPosEnum.Top);
            foreach (var child in children)
            {
                child.Shape.SetPosition
                (
                    position: Shape.GetSpecPos(origin: childOrigin)
                        + new MyVector2(-(int)childHorizPos * ActiveUIManager.RectOutlineWidth, curHeightSum + ActiveUIManager.RectOutlineWidth),
                    origin: childOrigin
                );
                curHeightSum += child.Shape.Height + gap;
            }
        }
    }
}
