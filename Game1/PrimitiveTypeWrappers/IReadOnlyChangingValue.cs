namespace Game1.PrimitiveTypeWrappers
{
    public interface IReadOnlyChangingValue<T>
    {
        public T Value { get; }

        //private class ValueFromTransform<TParam, TValue> : IReadOnlyChangingValue<T>
        //{
        //    public T Value
        //        => transform.Transform(param, readOnlyChangingValue.Value);

        //    private readonly ITransform<TParam, TValue, T> transform;
        //    private readonly TParam param;
        //    private readonly IReadOnlyChangingValue<TValue> readOnlyChangingValue;

        //    public ValueFromTransform(ITransform<TParam, TValue, T> transform, TParam param, IReadOnlyChangingValue<TValue> readOnlyChangingValue)
        //    {
        //        this.transform = transform;
        //        this.param = param;
        //        this.readOnlyChangingValue = readOnlyChangingValue;
        //    }
        //}

        //public static IReadOnlyChangingValue<T> Transform<TTransform, TParam, TValue>(TTransform transform, TParam param, IReadOnlyChangingValue<TValue> readOnlyChangingUFloat)
        //    where TTransform : struct, ITransform<TParam, TValue, UFloat>
        //    => new ValueFromTransform<TParam, TValue>(transform: transform, param: param, readOnlyChangingValue: readOnlyChangingUFloat);
    }
}
