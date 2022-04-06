namespace Game1.ChangingValues
{
    [Serializable]
    public class ChangingUDouble : IReadOnlyChangingUDouble
    {
        public UDouble Value { get; set; }

        public ChangingUDouble(UDouble value)
            => Value = value;
    }
}
