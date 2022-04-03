using Game1.PrimitiveTypeWrappers;

namespace Game1.ChangingValues
{
    public interface IReadOnlyChangingULong : IReadOnlyChangingValue<ulong>
    {
        [Serializable]
        private readonly struct ScaleULong :
            ITransformer<(ulong, IReadOnlyChangingULong), ulong>,
            ITransformer<(UFloat, IReadOnlyChangingULong), UFloat>,
            ITransformer<(IReadOnlyChangingULong, ReadOnlyULongArray), ReadOnlyULongArray>
        {
            public ulong Transform((ulong, IReadOnlyChangingULong) param)
                => param.Item1 * param.Item2.Value;

            public UFloat Transform((UFloat, IReadOnlyChangingULong) param)
                => param.Item1 * param.Item2.Value;

            public ReadOnlyULongArray Transform((IReadOnlyChangingULong, ReadOnlyULongArray) param)
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

        public static IReadOnlyChangingULongArray operator *(IReadOnlyChangingULong changingScalar, ReadOnlyULongArray readOnlyULongArray)
            => new ScaleULong().TransformIntoReadOnlyChangingUlongArray(param: (changingScalar, readOnlyULongArray));

        public static IReadOnlyChangingULongArray operator *(ReadOnlyULongArray readOnlyULongArray, IReadOnlyChangingULong changingScalar)
            => changingScalar * readOnlyULongArray;
    }
}
