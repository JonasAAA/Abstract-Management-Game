using Game1.PrimitiveTypeWrappers;

namespace Game1.ChangingValues
{
    public interface IReadOnlyChangingULong : IReadOnlyChangingValue<ulong>
    {
        [Serializable]
        private readonly struct ScaleULong :
            ITransformer<(ulong, IReadOnlyChangingULong), ulong>,
            ITransformer<(UFloat, IReadOnlyChangingULong), UFloat>,
            ITransformer<(IReadOnlyChangingULong, ResAmounts), ResAmounts>
        {
            public ulong Transform((ulong, IReadOnlyChangingULong) param)
                => param.Item1 * param.Item2.Value;

            public UFloat Transform((UFloat, IReadOnlyChangingULong) param)
                => param.Item1 * param.Item2.Value;

            public ResAmounts Transform((IReadOnlyChangingULong, ResAmounts) param)
                => param.Item1.Value * param.Item2;
        }

        public static IReadOnlyChangingULong operator *(ulong scalar, IReadOnlyChangingULong readOnlyChangingULong)
            => new ScaleULong().TransformIntoReadOnlyChangingULong(param: (scalar, readOnlyChangingULong));

        public static IReadOnlyChangingULong operator *(IReadOnlyChangingULong readOnlyChangingUFloat, ulong scalar)
            => scalar * readOnlyChangingUFloat;

        public static IReadOnlyChangingUFloat operator *(UFloat scalar, IReadOnlyChangingULong readOnlyChangingULong)
            => new ScaleULong().TransformIntoReadOnlyChangingUFloat(param: (scalar, readOnlyChangingULong));

        public static IReadOnlyChangingUFloat operator *(IReadOnlyChangingULong readOnlyChangingULong, UFloat scalar)
            => scalar * readOnlyChangingULong;

        public static IReadOnlyChangingResAmounts operator *(IReadOnlyChangingULong changingScalar, ResAmounts resAmounts)
            => new ScaleULong().TransformIntoReadOnlyChangingResAmounts(param: (changingScalar, resAmounts));

        public static IReadOnlyChangingResAmounts operator *(ResAmounts resAmounts, IReadOnlyChangingULong changingScalar)
            => changingScalar * resAmounts;
    }
}
