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
                [typeof(UDouble)] = typeof(ReadOnlyChangingUDoubleFromTransform<>),
                [typeof(ulong)] = typeof(ReadOnlyChangingULongFromTransform<>),
                [typeof(ResAmounts)] = typeof(ReadOnlyChangingResAmountsFromTransform<>),
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
        private readonly record struct ReadOnlyChangingUDoubleFromTransform<TParam>(ITransformer<TParam, UDouble> Transform, TParam Param) : IReadOnlyChangingUDouble
        {
            public UDouble Value
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
        private readonly record struct ReadOnlyChangingResAmountsFromTransform<TParam>(ITransformer<TParam, ResAmounts> Transform, TParam Param) : IReadOnlyChangingResAmounts
        {
            public ResAmounts Value
                => Transform.Transform(param: Param);

            public override string ToString()
                => Value.ToString();
        }

        public static IReadOnlyChangingUDouble TransformIntoReadOnlyChangingUDouble<TParam>(this ITransformer<TParam, UDouble> transformer, TParam param)
            => new ReadOnlyChangingUDoubleFromTransform<TParam>(Transform: transformer, Param: param);

        public static IReadOnlyChangingULong TransformIntoReadOnlyChangingULong<TParam>(this ITransformer<TParam, ulong> transformer, TParam param)
            => new ReadOnlyChangingULongFromTransform<TParam>(Transform: transformer, Param: param);
    
        public static IReadOnlyChangingResAmounts TransformIntoReadOnlyChangingResAmounts<TParam>(this ITransformer<TParam, ResAmounts> transformer, TParam param)
            => new ReadOnlyChangingResAmountsFromTransform<TParam>(Transform: transformer, Param: param);
    }
}
