using System.Text.Json.Serialization;

namespace Game1.ContentHelpers
{
    [Serializable]
    public readonly record struct CosmicBodyInfo
    {
        [JsonPropertyOrder(0)] public required string Name { get; init; }
        [JsonPropertyOrder(1)] public required Vector2Info Position { get; init; }
        [JsonPropertyOrder(2)] public required double Radius { get; init; }
    }
}
