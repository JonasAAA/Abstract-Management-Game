namespace Game1.ChangingValues
{
    [Serializable]
    public class ChangingULongArray : IReadOnlyChangingULongArray
    {
        public ReadOnlyULongArray Value { get; set; }
    }
}
