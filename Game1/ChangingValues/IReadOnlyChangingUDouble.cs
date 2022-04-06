namespace Game1.ChangingValues
{
    public interface IReadOnlyChangingUDouble : IReadOnlyChangingValue<UDouble>
    {
        [Serializable]
        private readonly struct ScaleUDoubleByUDouble : ITransformer<(IReadOnlyChangingUDouble, UDouble), UDouble>
        {
            public UDouble Transform((IReadOnlyChangingUDouble, UDouble) param)
                => param.Item1.Value * param.Item2;
        }

        [Serializable]
        private readonly struct RoundUDoubleDown : ITransformer<IReadOnlyChangingUDouble, ulong>
        {
            public ulong Transform(IReadOnlyChangingUDouble param)
                => (ulong)param.Value;
        }

        public static IReadOnlyChangingUDouble operator *(UDouble scalar, IReadOnlyChangingUDouble readOnlyChangingUDouble)
            => new ScaleUDoubleByUDouble().TransformIntoReadOnlyChangingUDouble(param: (readOnlyChangingUDouble, scalar));

        public static IReadOnlyChangingUDouble operator *(IReadOnlyChangingUDouble readOnlyChangingUDouble, UDouble scalar)
            => scalar * readOnlyChangingUDouble;

        public static IReadOnlyChangingUDouble operator /(IReadOnlyChangingUDouble readOnlyChangingUDouble, UDouble divisor)
            => readOnlyChangingUDouble * (1 / divisor);

        public IReadOnlyChangingULong RoundDown()
            => new RoundUDoubleDown().TransformIntoReadOnlyChangingULong(param: this);
    }
}
