using System;

namespace Game1
{
    public interface IReadOnlyChangingFloat
    {
        public float Value { get; }

        [Serializable]
        private class ScaledReadOnlyChangingFloat : IReadOnlyChangingFloat
        {
            public float Value
                => scalar * readOnlyChangingFloat.Value;

            private readonly float scalar;
            private readonly IReadOnlyChangingFloat readOnlyChangingFloat;

            public ScaledReadOnlyChangingFloat(float scalar, IReadOnlyChangingFloat readOnlyChangingFloat)
            {
                if (readOnlyChangingFloat is ScaledReadOnlyChangingFloat scaledReadOnlyChanginFloat)
                {
                    this.scalar = scalar * scaledReadOnlyChanginFloat.scalar;
                    this.readOnlyChangingFloat = scaledReadOnlyChanginFloat.readOnlyChangingFloat;
                    return;
                }
                this.scalar = scalar;
                this.readOnlyChangingFloat = readOnlyChangingFloat;
            }
        }

        public static IReadOnlyChangingFloat operator *(float scalar, IReadOnlyChangingFloat readOnlyChangingFloat)
            => new ScaledReadOnlyChangingFloat(scalar: scalar, readOnlyChangingFloat: readOnlyChangingFloat);

        public static IReadOnlyChangingFloat operator *(IReadOnlyChangingFloat readOnlyChangingFloat, float scalar)
            => scalar * readOnlyChangingFloat;

        public static IReadOnlyChangingFloat operator *(int scalar, IReadOnlyChangingFloat readOnlyChangingFloat)
            => (float)scalar * readOnlyChangingFloat;

        public static IReadOnlyChangingFloat operator *(IReadOnlyChangingFloat readOnlyChangingFloat, int scalar)
            => scalar * readOnlyChangingFloat;

        public static IReadOnlyChangingFloat operator /(IReadOnlyChangingFloat readOnlyChangingFloat, float divisor)
            => readOnlyChangingFloat * (1 / divisor);
    }
}
