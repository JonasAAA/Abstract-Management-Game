using Game1.PrimitiveTypeWrappers;

namespace Game1.ChangingValues
{
    public static class ExtensionMethods
    {
        private static readonly IReadOnlyDictionary<Type, Type> typeCorrespondance;

        static ExtensionMethods()
        {
            typeCorrespondance = new Dictionary<Type, Type>()
            {
                [typeof(UFloat)] = typeof(ReadOnlyChangingUFloatFromTransform<>),
                [typeof(ulong)] = typeof(ReadOnlyChangingULongFromTransform<>),
                [typeof(ReadOnlyULongArray)] = typeof(ReadOnlyChangingULongArrayFromTransform<>),
            };
        }

        public static IEnumerable<Type> GetRelatedGenericTypes(Type type)
        {
            foreach (var implementedInterface in type.GetInterfaces())
                if (implementedInterface.IsGenericType && implementedInterface.GetGenericTypeDefinition() == typeof(ITransformer<,>))
                {
                    Type[] paramTypes = implementedInterface.GetGenericArguments();
                    Debug.Assert(paramTypes.Length == 2);
                    Type knownType;
                    try
                    {
                        knownType = typeCorrespondance[paramTypes[1]].MakeGenericType(paramTypes[0]);
                    }
                    catch (KeyNotFoundException)
                    {
                        throw new Exception($"Missing ReadOnlyChanging...FromTranform corresponding to interface ITransform implementation with second generic parameter {paramTypes[1]}");
                    }
                    Debug.Assert(knownType != null);
                    yield return knownType;
                }
        }

        [Serializable]
        private readonly record struct ReadOnlyChangingUFloatFromTransform<TParam>(ITransformer<TParam, UFloat> Transform, TParam Param) : IReadOnlyChangingUFloat
        {
            public UFloat Value
                => Transform.Transform(param: Param);

            public override string ToString()
                => Value.ToString();
        }

        [Serializable]
        private readonly record struct ReadOnlyChangingULongFromTransform<TParam>(ITransformer<TParam, ulong> Transform, TParam Param) : IReadOnlyChangingULong
        {
            public ulong Value
                => Transform.Transform(param: Param);

            public override string ToString()
                => Value.ToString();
        }

        [Serializable]
        private readonly record struct ReadOnlyChangingULongArrayFromTransform<TParam>(ITransformer<TParam, ReadOnlyULongArray> Transform, TParam Param) : IReadOnlyChangingULongArray
        {
            public ReadOnlyULongArray Value
                => Transform.Transform(param: Param);

            public override string ToString()
                => Value.ToString();
        }

        public static IReadOnlyChangingUFloat TransformIntoReadOnlyChangingUFloat<TParam>(this ITransformer<TParam, UFloat> transformer, TParam param)
            => new ReadOnlyChangingUFloatFromTransform<TParam>(Transform: transformer, Param: param);

        public static IReadOnlyChangingULong TransformIntoReadOnlyChangingULong<TParam>(this ITransformer<TParam, ulong> transformer, TParam param)
            => new ReadOnlyChangingULongFromTransform<TParam>(Transform: transformer, Param: param);
    
        public static IReadOnlyChangingULongArray TransformIntoReadOnlyChangingUlongArray<TParam>(this ITransformer<TParam, ReadOnlyULongArray> transformer, TParam param)
            => new ReadOnlyChangingULongArrayFromTransform<TParam>(Transform: transformer, Param: param);
    }
}
