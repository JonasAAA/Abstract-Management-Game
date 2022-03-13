using System;

namespace Game1.PrimitiveTypeWrappers
{
    public static class ExtensionMethods
    {
        [Serializable]
        private record ReadOnlyChangingUFloatFromTransform<TValue>(ITransform<TValue, UFloat> Transform, IReadOnlyChangingValue<TValue> ReadOnlyChangingValue) : IReadOnlyChangingUFloat
        {
            public UFloat Value
                => Transform.Transform(ReadOnlyChangingValue.Value);
        }

        [Serializable]
        private record ReadOnlyChangingUFloatFromTransform<TParam, TValue>(ITransform<TParam, TValue, UFloat> Transform, TParam Param, IReadOnlyChangingValue<TValue> ReadOnlyChangingValue) : IReadOnlyChangingUFloat
        {
            public UFloat Value
                => Transform.Transform(Param, ReadOnlyChangingValue.Value);
        }

        [Serializable]
        private record ReadOnlyChangingULongFromTransform<TValue>(ITransform<TValue, ulong> Transform, IReadOnlyChangingValue<TValue> ReadOnlyChangingValue) : IReadOnlyChangingULong
        {
            public ulong Value
                => Transform.Transform(ReadOnlyChangingValue.Value);
        }

        [Serializable]
        private record ReadOnlyChangingULongFromTransform<TParam, TValue>(ITransform<TParam, TValue, ulong> Transform, TParam Param, IReadOnlyChangingValue<TValue> ReadOnlyChangingValue) : IReadOnlyChangingULong
        {
            public ulong Value
                => Transform.Transform(Param, ReadOnlyChangingValue.Value);
        }

        public static IReadOnlyChangingUFloat Transform<TValue>(this ITransform<TValue, UFloat> transform, IReadOnlyChangingValue<TValue> readOnlyChangingValue)
            => new ReadOnlyChangingUFloatFromTransform<TValue>(Transform: transform, ReadOnlyChangingValue: readOnlyChangingValue);

        public static IReadOnlyChangingUFloat Transform<TParam, TValue>(this ITransform<TParam, TValue, UFloat> transform, TParam param, IReadOnlyChangingValue<TValue> readOnlyChangingValue)
            => new ReadOnlyChangingUFloatFromTransform<TParam, TValue>(Transform: transform, Param: param, ReadOnlyChangingValue: readOnlyChangingValue);

        public static IReadOnlyChangingULong Transform<TParam, TValue>(this ITransform<TParam, TValue, ulong> transform, TParam param, IReadOnlyChangingValue<TValue> readOnlyChangingValue)
            => new ReadOnlyChangingULongFromTransform<TParam, TValue>(Transform: transform, Param: param, ReadOnlyChangingValue: readOnlyChangingValue);

        public static IReadOnlyChangingULong Transform<TValue>(this ITransform<TValue, ulong> transform, IReadOnlyChangingValue<TValue> readOnlyChangingValue)
            => new ReadOnlyChangingULongFromTransform<TValue>(Transform: transform, ReadOnlyChangingValue: readOnlyChangingValue);
    }
}
