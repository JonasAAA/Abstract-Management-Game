using Game1.PrimitiveTypeWrappers;

namespace Game1.ChangingValues
{
    [Serializable]
    public class ChangingUFloat : IReadOnlyChangingUFloat
    {
        public UFloat Value { get; set; }

        public ChangingUFloat(UFloat value)
            => Value = value;
    }
}
