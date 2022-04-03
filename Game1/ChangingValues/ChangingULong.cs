namespace Game1.ChangingValues
{
    [Serializable]
    public class ChangingULong : IReadOnlyChangingULong
    {
        public ulong Value { get; set; }

        public ChangingULong(ulong value)
            => Value = value;
    }
}
