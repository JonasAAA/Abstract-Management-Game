using Game1.Shapes;
using static Game1.GameConfig;

namespace Game1.UI
{
    [Serializable]
    public sealed class UIRectHorizPanel<TChild> : UIRectPanel<TChild>
        where TChild : IHUDElement
    {
        private readonly VertPosEnum childVertPos;
        private readonly UDouble gap;

        /// <summary>
        /// null gap means default gap
        /// </summary>
        public UIRectHorizPanel(VertPosEnum childVertPos, IEnumerable<TChild?> children, Color? backgroundColor = null, UDouble? gap = null)
            : base(backgroundColor: backgroundColor)
        {
            this.childVertPos = childVertPos;
            this.gap = gap ?? CurGameConfig.defaultGapBetweenUIElements;
            // This most be done after setting the gap, otherwise, when adding the children, incorrect gap will be used.
            AddChildren(newChildren: children);
        }

        protected sealed override void PartOfRecalcSizeAndPos()
        {
            base.PartOfRecalcSizeAndPos();

            Shape.Width = 2 * CurGameConfig.rectOutlineWidth + children.Sum(child => child.Shape.Width) + gap * UDouble.CreateByClamp(children.Count - 1);

            Shape.Height = 2 * CurGameConfig.rectOutlineWidth + children.MaxOrDefault(child => child.Shape.Height);

            UDouble curWidthSum = 0;
            PosEnums childOrigin = new(HorizPosEnum.Left, childVertPos);
            foreach (var child in children)
            {
                child.Shape.SetPosition
                (
                    position: Shape.GetSpecPos(origin: childOrigin)
                        + new Vector2Bare(CurGameConfig.rectOutlineWidth + curWidthSum, -(int)childVertPos * CurGameConfig.rectOutlineWidth),
                    origin: childOrigin
                );
                curWidthSum += child.Shape.Width + gap;
            }
        }
    }
}
