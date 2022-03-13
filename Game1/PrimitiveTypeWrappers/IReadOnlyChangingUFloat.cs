using System;

namespace Game1.PrimitiveTypeWrappers
{
    public interface IReadOnlyChangingUFloat : IReadOnlyChangingValue<UFloat>
    {
        [Serializable]
        private readonly struct ScaleUFloatByUFloat : ITransform<UFloat, UFloat, UFloat>
        {
            public readonly UFloat Transform(UFloat param, UFloat value)
                => param * value;
        }

        private readonly struct RoundUFloatDown : ITransform<UFloat, ulong>
        {
            public readonly ulong Transform(UFloat value)
                => (ulong)value;
        }

        public static IReadOnlyChangingUFloat operator *(UFloat scalar, IReadOnlyChangingUFloat readOnlyChangingUFloat)
            => new ScaleUFloatByUFloat().Transform(param: scalar, readOnlyChangingValue: readOnlyChangingUFloat);

        public static IReadOnlyChangingUFloat operator *(IReadOnlyChangingUFloat readOnlyChangingUFloat, UFloat scalar)
            => scalar * readOnlyChangingUFloat;

        public static IReadOnlyChangingUFloat operator /(IReadOnlyChangingUFloat readOnlyChangingUFloat, UFloat divisor)
            => readOnlyChangingUFloat * (1 / divisor);

        public IReadOnlyChangingULong RoundDown()
            => new RoundUFloatDown().Transform(readOnlyChangingValue: this);
    }
}
