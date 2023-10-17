﻿using Game1.Shapes;

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
        public UIRectHorizPanel(VertPosEnum childVertPos, IEnumerable<TChild?> children, UDouble? gap = null)
        {
            this.childVertPos = childVertPos;
            this.gap = gap ?? ActiveUIManager.DefaultGapBetweenUIElements;
            // This most be done after setting the gap, otherwise, when adding the children, incorrect gap will be used.
            AddChildren(newChildren: children);
        }

        protected sealed override void PartOfRecalcSizeAndPos()
        {
            base.PartOfRecalcSizeAndPos();

            Shape.Width = 2 * ActiveUIManager.RectOutlineWidth + children.Sum(child => child.Shape.Width) + gap * UDouble.CreateByClamp(children.Count - 1);

            Shape.Height = 2 * ActiveUIManager.RectOutlineWidth + children.MaxOrDefault(child => child.Shape.Height);

            UDouble curWidthSum = 0;
            PosEnums childOrigin = new(HorizPosEnum.Left, childVertPos);
            foreach (var child in children)
            {
                child.Shape.SetPosition
                (
                    position: Shape.GetSpecPos(origin: childOrigin)
                        + new Vector2Bare(ActiveUIManager.RectOutlineWidth + curWidthSum, -(int)childVertPos * ActiveUIManager.RectOutlineWidth),
                    origin: childOrigin
                );
                curWidthSum += child.Shape.Width + gap;
            }
        }
    }
}
