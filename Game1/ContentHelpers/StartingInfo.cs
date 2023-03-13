namespace Game1.ContentHelpers
{
    [Serializable]
    public readonly struct StartingInfo
    {
        public required string CosmicBodyName { get; init; }
        public required int PeopleCount { get; init; }
    }
}
