using System;

namespace Game1.PrimitiveTypeWrappers
{
    public interface IReadOnlyChangingULong : IReadOnlyChangingValue<ulong>
    {
        [Serializable]
        private readonly struct ScaleULong : ITransform<ulong, ulong, ulong>, ITransform<UFloat, ulong, UFloat>
        {
            public readonly ulong Transform(ulong param, ulong value)
                => param * value;

            public readonly UFloat Transform(UFloat param, ulong value)
                => param * value;
        }

        public static IReadOnlyChangingULong operator *(ulong scalar, IReadOnlyChangingULong readOnlyChangingULong)
            => new ScaleULong().Transform(param: scalar, readOnlyChangingValue: readOnlyChangingULong);

        public static IReadOnlyChangingULong operator *(IReadOnlyChangingULong readOnlyChangingUFloat, ulong scalar)
            => scalar * readOnlyChangingUFloat;

        public static IReadOnlyChangingUFloat operator *(UFloat scalar, IReadOnlyChangingULong readOnlyChangingULong)
            => new ScaleULong().Transform(param: scalar, readOnlyChangingValue: readOnlyChangingULong);

        public static IReadOnlyChangingUFloat operator *(IReadOnlyChangingULong readOnlyChangingULong, UFloat scalar)
            => scalar * readOnlyChangingULong;
    }
}
