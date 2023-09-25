namespace Game1.PrimitiveTypeWrappers
{
    /// <summary>
    /// Mechanical complexity. Operating costs increase and throughput decreases with complex a product/industry
    /// </summary>
    [Serializable]
    public readonly struct MechComplexity
    {
        private readonly ulong complexity;

        public MechComplexity(ulong complexity)
            => this.complexity = complexity;
    }
}
