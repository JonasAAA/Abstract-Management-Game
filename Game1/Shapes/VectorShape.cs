namespace Game1.Shapes
{
    [Serializable]
    public abstract class VectorShape : Shape
    {
        public interface IParams
        {
            public MyVector2 StartPos { get; }
            public MyVector2 EndPos { get; }
            public UDouble Width { get; }
        }

        protected abstract Texture2D Texture { get; }

        protected readonly IParams parameters;

        // TODO: consider creating unsigned variables for texture width and height (or maybe extension methods for that)
        protected VectorShape(IParams parameters, Color color)
            : base(color: color)
        {
            if (MyMathHelper.IsTiny(MyVector2.Distance(parameters.StartPos, parameters.EndPos)))
                throw new ArgumentException();
            this.parameters = parameters;
        }

        public override bool Contains(MyVector2 position)
        {
            MyVector2 relPos = position - parameters.StartPos,
                direction = MyVector2.Normalized(parameters.EndPos - parameters.StartPos);
            MyVector2 orthDir = new(-direction.Y, direction.X);
            double distance = MyVector2.Distance(parameters.StartPos, parameters.EndPos),
                dirProp = MyVector2.Dot(relPos, direction) / distance;
            return (Propor.Create(value: dirProp), Propor.Create(part: MyMathHelper.Abs(MyVector2.Dot(relPos, orthDir)), whole: parameters.Width * (UDouble).5)) switch
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
                position: (parameters.StartPos + parameters.EndPos) / 2,
                color: color,
                rotation: MyMathHelper.Rotation(vector: parameters.EndPos - parameters.StartPos),
                origin: new MyVector2(Texture.Width, Texture.Height) * .5,
                scaleX: MyVector2.Distance(parameters.StartPos, parameters.EndPos) / (UDouble)Texture.Width,
                scaleY: parameters.Width / (UDouble)Texture.Height
            );
    }
}
