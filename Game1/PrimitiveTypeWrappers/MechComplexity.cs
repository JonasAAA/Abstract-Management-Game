namespace Game1.PrimitiveTypeWrappers
{
    /// <summary>
    /// Mechanical complexity. Operating costs increase and throughput decreases with complex a product/industry
    /// </summary>
    [Serializable]
    public readonly struct MechComplexity(ulong complexity)
    {
        private readonly ulong complexity = complexity;
    }
}
