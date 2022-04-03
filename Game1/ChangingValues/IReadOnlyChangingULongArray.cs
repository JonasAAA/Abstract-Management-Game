namespace Game1.ChangingValues
{
    public interface IReadOnlyChangingULongArray : IReadOnlyChangingValue<ReadOnlyULongArray>
    {
        [Serializable]
        private readonly struct ScaleULongArrayByULong :
            ITransformer<(ulong, IReadOnlyChangingULongArray), ReadOnlyULongArray>
        {
            public ReadOnlyULongArray Transform((ulong, IReadOnlyChangingULongArray) param)
                => param.Item1 * param.Item2.Value;
        }

        public static IReadOnlyChangingULongArray operator *(ulong scalar, IReadOnlyChangingULongArray readOnlyChangingULongArray)
            => new ScaleULongArrayByULong().TransformIntoReadOnlyChangingUlongArray(param: (scalar, readOnlyChangingULongArray));

        public static IReadOnlyChangingULongArray operator *(IReadOnlyChangingULongArray readOnlyChangingULongArray, ulong scalar)
            => scalar * readOnlyChangingULongArray;
    }
}
