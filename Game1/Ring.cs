using Game1.Shapes;

namespace Game1
{
    [Serializable]
    public abstract class Ring : WorldShape
    {
        public interface IParamsWithInnerRadius
        {
            public MyVector2 Center { get; }

            public Length InnerRadius { get; }
        }

        public interface IParamsWithOuterRadius
        {
            public MyVector2 Center { get; }

            public Length OuterRadius { get; }
        }

        [Serializable]
        private readonly record struct ParamsWithOuterRadius(IParamsWithInnerRadius ParamsWithInnerRadius) : IParamsWithOuterRadius
        {
            public MyVector2 Center
                => ParamsWithInnerRadius.Center;
            public Length OuterRadius
                => GetOuterRadius(innerRadius: ParamsWithInnerRadius.InnerRadius);
        }

        private static readonly UDouble ringWidthToInnerRadiusRatio = UDouble.half;

        protected static Length GetInnerRadius(Length outerRadius)
            => outerRadius / (1 + ringWidthToInnerRadiusRatio);

        protected static Length GetOuterRadius(Length innerRadius)
            => innerRadius * (1 + ringWidthToInnerRadiusRatio);

        protected abstract Texture2D RingTexture { get; }

        public MyVector2 Center
            => parameters.Center;

        private Length InnerRadius
            => GetInnerRadius(outerRadius: parameters.OuterRadius);

        private readonly IParamsWithOuterRadius parameters;

        protected Ring(IParamsWithOuterRadius paramsWithOuterRadius, WorldCamera worldCamera)
            : base(worldCamera: worldCamera)
            => parameters = paramsWithOuterRadius;

        protected Ring(IParamsWithInnerRadius paramsWithInnerRadius, WorldCamera worldCamera)
            : this(paramsWithOuterRadius: new ParamsWithOuterRadius(ParamsWithInnerRadius: paramsWithInnerRadius), worldCamera: worldCamera)
        { }

        public sealed override bool Contains(MyVector2 position)
        {
            Length distance = MyVector2.Distance(position, Center);
            return InnerRadius < distance && distance < parameters.OuterRadius;
        }

        public sealed override void Draw(Color color)
            => C.Draw
            (
                texture: RingTexture,
                position: Center,
                color: color,
                rotation: 0,
                origin: new Vector2Bare(RingTexture.Width, RingTexture.Height) * .5,
                scale: 2 * parameters.OuterRadius / (UDouble)RingTexture.Width
            );
    }
}
