namespace Game1.PrimitiveTypeWrappers
{
    [Serializable]
    public class ChangingULongArray : IReadOnlyChangingULongArray
    {
        public ConstULongArray Value { get; set; }
    }
}
