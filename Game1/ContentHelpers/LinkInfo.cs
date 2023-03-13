namespace Game1.ContentHelpers
{
    [Serializable]
    public readonly struct LinkInfo
    {
        public required string From { get; init; }
        public required string To { get; init; }
    }
}
