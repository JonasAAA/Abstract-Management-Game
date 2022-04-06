using Game1.ChangingValues;

namespace Game1.Shapes
{
    [Serializable]
    public abstract class VectorShape : Shape
    {
        public readonly MyVector2 startPos, endPos;

        protected abstract Texture2D Texture { get; }
        protected readonly IReadOnlyChangingUDouble width;

        // TODO: consider creating unsigned variables for texture width and height (or maybe extension methods for that)
        protected VectorShape(MyVector2 startPos, MyVector2 endPos, IReadOnlyChangingUDouble width)
        {
            if (MyMathHelper.IsTiny(MyVector2.Distance(startPos, endPos)))
                throw new ArgumentException();
            this.startPos = startPos;
            this.endPos = endPos;
            this.width = width;
        }

        public override bool Contains(MyVector2 position)
        {
            MyVector2 relPos = position - startPos,
                direction = MyVector2.Normalized(endPos - startPos);
            MyVector2 orthDir = new(-direction.Y, direction.X);
            double distance = (double)MyVector2.Distance(startPos, endPos),
                dirProp = MyVector2.Dot(relPos, direction) / distance;
            return (Propor.Create(value: dirProp), Propor.Create(part: MyMathHelper.Abs(MyVector2.Dot(relPos, orthDir)), whole: width.Value * (UDouble).5)) switch
            {
                (Propor dirPropor, Propor orthDirPropor) => Contains(dirPropor: dirPropor, orthDirPropor: orthDirPropor),
                _ => false
            };
        }

        protected abstract bool Contains(Propor dirPropor, Propor orthDirPropor);

        protected override void Draw(Color color)
            => C.Draw
            (
                texture: Texture,
                position: (startPos + endPos) / 2,
                color: color,
                rotation: MyMathHelper.Rotation(vector: endPos - startPos),
                origin: new MyVector2(Texture.Width, Texture.Height) * .5,
                scaleX: MyVector2.Distance(startPos, endPos) / (UDouble)Texture.Width,
                scaleY: width.Value / (UDouble)Texture.Height
            );
    }
}
