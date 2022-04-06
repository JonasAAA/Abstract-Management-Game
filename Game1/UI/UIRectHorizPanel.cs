﻿using Game1.PrimitiveTypeWrappers;
using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public class UIRectHorizPanel<TChild> : UIRectPanel<TChild>
        where TChild : IHUDElement
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

            Shape.Width = 2 * ActiveUIManager.RectOutlineWidth + children.Count switch
            {
                0 => (UDouble)0,
                not 0 => children.Sum(child => child.Shape.Width)
            };

            Shape.Height = 2 * ActiveUIManager.RectOutlineWidth + children.Count switch
            {
                0 => 0,
                not 0 => children.Max(child => child.Shape.Height)
            };

            UDouble curWidthSum = 0;
            foreach (var child in children)
            {
                child.Shape.SetPosition
                (
                    position: Shape.GetPosition(horizOrigin: HorizPos.Left, vertOrigin: childVertPos)
                        + new MyVector2((double)ActiveUIManager.RectOutlineWidth + (double)curWidthSum, -(int)childVertPos * (double)ActiveUIManager.RectOutlineWidth),
                    horizOrigin: HorizPos.Left,
                    vertOrigin: childVertPos
                );
                curWidthSum += child.Shape.Width;
            }
        }
    }
}
