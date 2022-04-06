using Game1.PrimitiveTypeWrappers;

namespace Game1.ChangingValues
{
    public interface IReadOnlyChangingULong : IReadOnlyChangingValue<ulong>
    {
        [Serializable]
        private readonly struct ScaleULong :
            ITransformer<(ulong, IReadOnlyChangingULong), ulong>,
            ITransformer<(UDouble, IReadOnlyChangingULong), UDouble>,
            ITransformer<(IReadOnlyChangingULong, ResAmounts), ResAmounts>
        {
            public ulong Transform((ulong, IReadOnlyChangingULong) param)
                => param.Item1 * param.Item2.Value;

            public UDouble Transform((UDouble, IReadOnlyChangingULong) param)
                => param.Item1 * param.Item2.Value;

            public ResAmounts Transform((IReadOnlyChangingULong, ResAmounts) param)
                => param.Item1.Value * param.Item2;
        }

        public static IReadOnlyChangingULong operator *(ulong scalar, IReadOnlyChangingULong readOnlyChangingULong)
            => new ScaleULong().TransformIntoReadOnlyChangingULong(param: (scalar, readOnlyChangingULong));

        public static IReadOnlyChangingULong operator *(IReadOnlyChangingULong readOnlyChangingUDouble, ulong scalar)
            => scalar * readOnlyChangingUDouble;

        public static IReadOnlyChangingUDouble operator *(UDouble scalar, IReadOnlyChangingULong readOnlyChangingULong)
            => new ScaleULong().TransformIntoReadOnlyChangingUDouble(param: (scalar, readOnlyChangingULong));

        public static IReadOnlyChangingUDouble operator *(IReadOnlyChangingULong readOnlyChangingULong, UDouble scalar)
            => scalar * readOnlyChangingULong;

        public static IReadOnlyChangingResAmounts operator *(IReadOnlyChangingULong changingScalar, ResAmounts resAmounts)
            => new ScaleULong().TransformIntoReadOnlyChangingResAmounts(param: (changingScalar, resAmounts));

        public static IReadOnlyChangingResAmounts operator *(ResAmounts resAmounts, IReadOnlyChangingULong changingScalar)
            => changingScalar * resAmounts;
    }
}
