namespace Game1.ChangingValues
{
    public interface IReadOnlyChangingResAmounts : IReadOnlyChangingValue<ResAmounts>
    {
        [Serializable]
        private readonly struct ScaleResAmountsByULong :
            ITransformer<(ulong, IReadOnlyChangingResAmounts), ResAmounts>
        {
            public ResAmounts Transform((ulong, IReadOnlyChangingResAmounts) param)
                => param.Item1 * param.Item2.Value;
        }

        public static IReadOnlyChangingResAmounts operator *(ulong scalar, IReadOnlyChangingResAmounts readOnlyChangingResAmounts)
            => new ScaleResAmountsByULong().TransformIntoReadOnlyChangingResAmounts(param: (scalar, readOnlyChangingResAmounts));

        public static IReadOnlyChangingResAmounts operator *(IReadOnlyChangingResAmounts readOnlyChangingResAmounts, ulong scalar)
            => scalar * readOnlyChangingResAmounts;
    }
}
