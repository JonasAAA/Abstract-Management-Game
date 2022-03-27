namespace Game1.PrimitiveTypeWrappers
{
    [Serializable]
    public class ChangingULong : IReadOnlyChangingULong
    {
        public ulong Value { get; set; }

        public ChangingULong(ulong value)
            => Value = value;
    }
}
