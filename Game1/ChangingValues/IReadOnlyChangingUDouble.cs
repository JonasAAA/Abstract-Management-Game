namespace Game1.ChangingValues
{
    public interface IReadOnlyChangingUDouble : IReadOnlyChangingValue<UDouble>, ISquareRootable<IReadOnlyChangingUDouble>
    {
        [Serializable]
        private readonly struct ScaleUDoubleByUDouble : ITransformer<(IReadOnlyChangingUDouble, UDouble), UDouble>, ITransformer<(IReadOnlyChangingUDouble, IReadOnlyChangingUDouble), UDouble>
        {
            public UDouble Transform((IReadOnlyChangingUDouble, UDouble) param)
                => param.Item1.Value * param.Item2;

            public UDouble Transform((IReadOnlyChangingUDouble, IReadOnlyChangingUDouble) param)
                => param.Item1.Value * param.Item2.Value;
        }

        [Serializable]
        private readonly struct RoundUDoubleDown : ITransformer<IReadOnlyChangingUDouble, ulong>
        {
            public ulong Transform(IReadOnlyChangingUDouble param)
                => (ulong)param.Value;
        }

        [Serializable]
        private readonly struct RoundUDouble : ITransformer<IReadOnlyChangingUDouble, ulong>
        {
            public ulong Transform(IReadOnlyChangingUDouble param)
                => Convert.ToUInt64(param.Value);
        }

        [Serializable]
        private readonly struct SquareRootUDouble : ITransformer<IReadOnlyChangingUDouble, UDouble>
        {
            public UDouble Transform(IReadOnlyChangingUDouble param)
                => MyMathHelper.Sqrt(param.Value);
        }

        public static IReadOnlyChangingUDouble operator *(UDouble scalar, IReadOnlyChangingUDouble readOnlyChangingUDouble)
            => new ScaleUDoubleByUDouble().TransformIntoReadOnlyChangingUDouble(param: (readOnlyChangingUDouble, scalar));

        public static IReadOnlyChangingUDouble operator *(IReadOnlyChangingUDouble readOnlyChangingUDouble, UDouble scalar)
            => scalar * readOnlyChangingUDouble;

        public static IReadOnlyChangingUDouble operator *(IReadOnlyChangingUDouble value1, IReadOnlyChangingUDouble value2)
            => new ScaleUDoubleByUDouble().TransformIntoReadOnlyChangingUDouble(param: (value1, value2));

        public static IReadOnlyChangingUDouble operator /(IReadOnlyChangingUDouble readOnlyChangingUDouble, UDouble divisor)
            => readOnlyChangingUDouble * (1 / divisor);

        public IReadOnlyChangingULong RoundDown()
            => new RoundUDoubleDown().TransformIntoReadOnlyChangingULong(param: this);

        public IReadOnlyChangingULong Round()
            => new RoundUDouble().TransformIntoReadOnlyChangingULong(param: this);

        IReadOnlyChangingUDouble ISquareRootable<IReadOnlyChangingUDouble>.Sqrt()
            => new SquareRootUDouble().TransformIntoReadOnlyChangingUDouble(param: this);
    }
}
