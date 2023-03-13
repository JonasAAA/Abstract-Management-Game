namespace Game1.ContentHelpers
{
    [Serializable]
    public readonly struct CosmicBodyInfo
    {
        public required string Name { get; init; }
        public required string ConsistsOf { get; init; }
        public required MyVector2Info HUDPosition { get; init; }
        public required double HUDRadius { get; init; }
    }
}
