using Game1.PrimitiveTypeWrappers;

namespace Game1.ChangingValues
{
    public interface IReadOnlyChangingUFloat : IReadOnlyChangingValue<UFloat>
    {
        [Serializable]
        private readonly struct ScaleUFloatByUFloat : ITransformer<(IReadOnlyChangingUFloat, UFloat), UFloat>
        {
            public UFloat Transform((IReadOnlyChangingUFloat, UFloat) param)
                => param.Item1.Value * param.Item2;
        }

        [Serializable]
        private readonly struct RoundUFloatDown : ITransformer<IReadOnlyChangingUFloat, ulong>
        {
            public ulong Transform(IReadOnlyChangingUFloat param)
                => (ulong)param.Value;
        }

        public static IReadOnlyChangingUFloat operator *(UFloat scalar, IReadOnlyChangingUFloat readOnlyChangingUFloat)
            => new ScaleUFloatByUFloat().TransformIntoReadOnlyChangingUFloat(param: (readOnlyChangingUFloat, scalar));

        public static IReadOnlyChangingUFloat operator *(IReadOnlyChangingUFloat readOnlyChangingUFloat, UFloat scalar)
            => scalar * readOnlyChangingUFloat;

        public static IReadOnlyChangingUFloat operator /(IReadOnlyChangingUFloat readOnlyChangingUFloat, UFloat divisor)
            => readOnlyChangingUFloat * (1 / divisor);

        public IReadOnlyChangingULong RoundDown()
            => new RoundUFloatDown().TransformIntoReadOnlyChangingULong(param: this);
    }
}
