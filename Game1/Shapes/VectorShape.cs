namespace Game1.Shapes
{
    [Serializable]
    public abstract class VectorShape : WorldShape
    {
        public interface IParams
        {
            public MyVector2 StartPos { get; }
            public MyVector2 EndPos { get; }
            public Length Width { get; }
        }

        protected abstract Texture2D Texture { get; }

        protected readonly IParams parameters;

        // TODO: consider creating unsigned variables for texture width and height (or maybe extension methods for that)
        protected VectorShape(IParams parameters, WorldCamera worldCamera)
            : base(worldCamera)
        {
            if (MyMathHelper.IsTiny(MyVector2.Distance(parameters.StartPos, parameters.EndPos).valueInM))
                throw new ArgumentException();
            this.parameters = parameters;
        }

        public sealed override bool Contains(MyVector2 position)
        {
            MyVector2 relPos = position - parameters.StartPos;
            Vector2Bare direction = MyVector2.Normalized(parameters.EndPos - parameters.StartPos);
            Vector2Bare orthDir = new(-direction.Y, direction.X);
            Length distance = MyVector2.Distance(parameters.StartPos, parameters.EndPos);
            double dirProp = MyVector2.Dot(relPos, direction) / distance;
            return (Propor.Create(value: dirProp), Propor.Create(part: MyVector2.Dot(relPos, orthDir).Abs().valueInM, whole: parameters.Width.valueInM * UDouble.half)) switch
            {
                (Propor dirPropor, Propor orthDirPropor) => Contains(dirPropor: dirPropor, orthDirPropor: orthDirPropor),
                _ => false
            };
        }

        protected abstract bool Contains(Propor dirPropor, Propor orthDirPropor);

        public sealed override void Draw(Color color)
            => C.Draw
            (
                texture: Texture,
                position: (parameters.StartPos + parameters.EndPos) / 2,
                color: color,
                rotation: MyMathHelper.Rotation(vector: parameters.EndPos - parameters.StartPos),
                origin: new Vector2Bare(Texture.Width, Texture.Height) * .5,
                scaleX: MyVector2.Distance(parameters.StartPos, parameters.EndPos) / (UDouble)Texture.Width,
                scaleY: parameters.Width / (UDouble)Texture.Height
            );
    }
}
