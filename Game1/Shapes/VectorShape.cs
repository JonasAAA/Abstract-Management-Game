namespace Game1.Shapes
{
    [Serializable]
    public abstract class VectorShape : Shape
    {
        public interface IParams
        {
            public MyVector2 startPos { get; }
            public MyVector2 endPos { get; }
            public UDouble width { get; }
        }

        public MyVector2 startPos
            => parameters.startPos;

        public MyVector2 endPos
            => parameters.endPos;

        protected abstract Texture2D Texture { get; }

        protected readonly IParams parameters;

        // TODO: consider creating unsigned variables for texture width and height (or maybe extension methods for that)
        protected VectorShape(IParams parameters)
        {
            if (MyMathHelper.IsTiny(MyVector2.Distance(parameters.startPos, parameters.endPos)))
                throw new ArgumentException();
            this.parameters = parameters;
        }

        public override bool Contains(MyVector2 position)
        {
            MyVector2 relPos = position - parameters.startPos,
                direction = MyVector2.Normalized(parameters.endPos - parameters.startPos);
            MyVector2 orthDir = new(-direction.Y, direction.X);
            double distance = MyVector2.Distance(parameters.startPos, parameters.endPos),
                dirProp = MyVector2.Dot(relPos, direction) / distance;
            return (Propor.Create(value: dirProp), Propor.Create(part: MyMathHelper.Abs(MyVector2.Dot(relPos, orthDir)), whole: parameters.width * (UDouble).5)) switch
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
                position: (parameters.startPos + parameters.endPos) / 2,
                color: color,
                rotation: MyMathHelper.Rotation(vector: parameters.endPos - parameters.startPos),
                origin: new MyVector2(Texture.Width, Texture.Height) * .5,
                scaleX: MyVector2.Distance(parameters.startPos, parameters.endPos) / (UDouble)Texture.Width,
                scaleY: parameters.width / (UDouble)Texture.Height
            );
    }
}
