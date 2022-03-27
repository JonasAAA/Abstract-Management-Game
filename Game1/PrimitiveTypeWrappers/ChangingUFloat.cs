namespace Game1.PrimitiveTypeWrappers
{
    [Serializable]
    public class ChangingUFloat : IReadOnlyChangingUFloat
    {
        public UFloat Value { get; set; }

        public ChangingUFloat(UFloat value)
            => Value = value;
    }
}
