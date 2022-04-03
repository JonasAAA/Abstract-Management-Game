namespace Game1.ChangingValues
{
    [Serializable]
    public class ChangingResAmounts : IReadOnlyChangingResAmounts
    {
        public ResAmounts Value { get; set; }
    }
}
