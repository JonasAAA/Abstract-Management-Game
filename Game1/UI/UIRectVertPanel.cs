using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public class UIRectVertPanel<TChild> : UIRectPanel<TChild>
        where TChild : IHUDElement
    {
        [Serializable]
        public new class ImmutableParams : UIRectPanel<TChild>.ImmutableParams, IParams
        {
            public ImmutableParams(Color backgroundColor)
                : base(backgroundColor: backgroundColor)
            { }
        }

        public new interface IParams : UIRectPanel<TChild>.IParams
        { }

        private readonly HorizPos childHorizPos;

        public UIRectVertPanel(IParams parameters, HorizPos childHorizPos)
            : base(parameters: parameters)
        {
            this.childHorizPos = childHorizPos;
        }

        protected override void PartOfRecalcSizeAndPos()
        {
            base.PartOfRecalcSizeAndPos();

            Shape.Width = 2 * ActiveUIManager.RectOutlineWidth + children.Count switch
            {
                0 => (UDouble)0,
                not 0 => children.Max(child => child.Shape.Width)
            };
            
            Shape.Height = 2 * ActiveUIManager.RectOutlineWidth + children.Count switch
            {
                0 => 0,
                not 0 => children.Sum(child => child.Shape.Height)
            };

            UDouble curHeightSum = 0;
            foreach (var child in children)
            {
                child.Shape.SetPosition
                (
                    position: Shape.GetPosition(horizOrigin: childHorizPos, vertOrigin: VertPos.Top)
                        + new MyVector2(-(int)childHorizPos * ActiveUIManager.RectOutlineWidth, curHeightSum + ActiveUIManager.RectOutlineWidth),
                    horizOrigin: childHorizPos,
                    vertOrigin: VertPos.Top
                );
                curHeightSum += child.Shape.Height;
            }
        }
    }
}
