namespace Game1.PrimitiveTypeWrappers
{
    public static class ExtensionMethods
    {
        public static IEnumerable<Type> GetRelatedGenericTypes(Type type)
        {
            foreach (var implementedInterface in type.GetInterfaces())
                if (implementedInterface.IsGenericType && implementedInterface.GetGenericTypeDefinition() == typeof(ITransformer<,>))
                {
                    var parameterType = implementedInterface.GetGenericArguments().First();
                    yield return typeof(ReadOnlyChangingUFloatFromTransform<>).MakeGenericType(parameterType);
                    yield return typeof(ReadOnlyChangingULongFromTransform<>).MakeGenericType(parameterType);
                }
        }

        [Serializable]
        private readonly record struct ReadOnlyChangingUFloatFromTransform<TParam>(ITransformer<TParam, UFloat> Transform, TParam Param) : IReadOnlyChangingUFloat
        {
            public UFloat Value
                => Transform.Transform(param: Param);
        }

        [Serializable]
        private readonly record struct ReadOnlyChangingULongFromTransform<TParam>(ITransformer<TParam, ulong> Transform, TParam Param) : IReadOnlyChangingULong
        {
            public ulong Value
                => Transform.Transform(param: Param);
        }

        public static IReadOnlyChangingUFloat TransformIntoReadOnlyChangingUFloat<TParam>(this ITransformer<TParam, UFloat> transformer, TParam param)
            => new ReadOnlyChangingUFloatFromTransform<TParam>(Transform: transformer, Param: param);

        public static IReadOnlyChangingULong TransformIntoReadOnlyChangingULong<TParam>(this ITransformer<TParam, ulong> transformer, TParam param)
            => new ReadOnlyChangingULongFromTransform<TParam>(Transform: transformer, Param: param);
    }
}
