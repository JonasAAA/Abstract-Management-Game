namespace Game1.PrimitiveTypeWrappers
{
    public interface IReadOnlyChangingULong : IReadOnlyChangingValue<ulong>
    {
        [Serializable]
        private readonly struct ScaleULong : ITransformer<(ulong, IReadOnlyChangingULong), ulong>, ITransformer<(UFloat, IReadOnlyChangingULong), UFloat>
        {
            public readonly ulong Transform((ulong, IReadOnlyChangingULong) param)
                => param.Item1 * param.Item2.Value;

            public readonly UFloat Transform((UFloat, IReadOnlyChangingULong) param)
                => param.Item1 * param.Item2.Value;
        }

        public static IReadOnlyChangingULong operator *(ulong scalar, IReadOnlyChangingULong readOnlyChangingULong)
            => new ScaleULong().TransformIntoReadOnlyChangingULong(param: (scalar, readOnlyChangingULong));

        public static IReadOnlyChangingULong operator *(IReadOnlyChangingULong readOnlyChangingUFloat, ulong scalar)
            => scalar * readOnlyChangingUFloat;

        public static IReadOnlyChangingUFloat operator *(UFloat scalar, IReadOnlyChangingULong readOnlyChangingULong)
            => new ScaleULong().TransformIntoReadOnlyChangingUFloat(param: (scalar, readOnlyChangingULong));

        public static IReadOnlyChangingUFloat operator *(IReadOnlyChangingULong readOnlyChangingULong, UFloat scalar)
            => scalar * readOnlyChangingULong;
    }
}
