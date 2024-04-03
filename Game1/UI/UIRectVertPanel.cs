using Game1.Shapes;
using static Game1.GlobalTypes.GameConfig;

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
        public UIRectVertPanel(HorizPosEnum childHorizPos, IEnumerable<TChild?> children, Color? backgroundColor = null, UDouble? gap = default)
            : base(backgroundColor: backgroundColor)
        {
            this.childHorizPos = childHorizPos;
            this.gap = gap ?? CurGameConfig.defaultGapBetweenUIElements;
            // This most be done after setting the gap, otherwise, when adding the children, incorrect gap will be used.
            AddChildren(newChildren: children);
        }

        protected sealed override void PartOfRecalcSizeAndPos()
        {
            base.PartOfRecalcSizeAndPos();

            Shape.Width = 2 * CurGameConfig.rectOutlineWidth + children.MaxOrDefault(child => child.Shape.Width);

            Shape.Height = 2 * CurGameConfig.rectOutlineWidth + children.Sum(child => child.Shape.Height) + gap * UDouble.CreateByClamp(children.Count - 1);

            UDouble curHeightSum = 0;
            PosEnums childOrigin = new(childHorizPos, VertPosEnum.Top);
            foreach (var child in children)
            {
                child.Shape.SetPosition
                (
                    position: Shape.GetSpecPos(origin: childOrigin)
                        + new Vector2Bare(-(int)childHorizPos * CurGameConfig.rectOutlineWidth, curHeightSum + CurGameConfig.rectOutlineWidth),
                    origin: childOrigin
                );
                curHeightSum += child.Shape.Height + gap;
            }
        }
    }
}
