using System;

namespace Game1
{
    [Serializable]
    public class ChangingFloat : IReadOnlyChangingFloat
    {
        public float Value { get; set; }

        public ChangingFloat(float value)
            => Value = value;
    }
}
