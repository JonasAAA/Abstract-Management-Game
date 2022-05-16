using Game1.Shapes;

namespace Game1
{
    [Serializable]
    public abstract class Ring : Shape
    {
        public interface IParamsWithInnerRadius
        {
            public MyVector2 Center { get; }

            public UDouble InnerRadius { get; }
        }

        public interface IParamsWithOuterRadius
        {
            public MyVector2 Center { get; }

            public UDouble OuterRadius { get; }
        }

        [Serializable]
        private readonly record struct ParamsWithOuterRadius(IParamsWithInnerRadius ParamsWithInnerRadius) : IParamsWithOuterRadius
        {
            public MyVector2 Center
                => ParamsWithInnerRadius.Center;
            public UDouble OuterRadius
                => GetOuterRadius(innerRadius: ParamsWithInnerRadius.InnerRadius);
        }

        private static readonly UDouble ringWidthToInnerRadiusRatio = (UDouble).5;

        protected static UDouble GetInnerRadius(UDouble outerRadius)
            => outerRadius / (1 + ringWidthToInnerRadiusRatio);

        protected static UDouble GetOuterRadius(UDouble innerRadius)
            => innerRadius * (1 + ringWidthToInnerRadiusRatio);

        protected abstract Texture2D RingTexture { get; }

        public MyVector2 Center
            => parameters.Center;

        private UDouble InnerRadius
            => GetInnerRadius(outerRadius: parameters.OuterRadius);

        private readonly IParamsWithOuterRadius parameters;

        protected Ring(IParamsWithOuterRadius paramsWithOuterRadius)
            => parameters = paramsWithOuterRadius;

        protected Ring(IParamsWithInnerRadius paramsWithInnerRadius)
            : this(paramsWithOuterRadius: new ParamsWithOuterRadius(ParamsWithInnerRadius: paramsWithInnerRadius))
        { }

        public sealed override bool Contains(MyVector2 position)
        {
            UDouble distance = MyVector2.Distance(position, Center);
            return InnerRadius < distance && distance < parameters.OuterRadius;
        }

        public sealed override void Draw(Color color)
            => C.Draw
            (
                texture: RingTexture,
                position: Center,
                color: color,
                rotation: 0,
                origin: new MyVector2(RingTexture.Width, RingTexture.Height) * .5,
                scale: 2 * parameters.OuterRadius / (UDouble)RingTexture.Width
            );
    }
}
