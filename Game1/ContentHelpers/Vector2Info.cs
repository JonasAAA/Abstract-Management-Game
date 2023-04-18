using System.Text.Json.Serialization;

namespace Game1.ContentHelpers
{
    [Serializable]
    public readonly struct Vector2Info
    {
        [JsonPropertyOrder(0)] public required double X { get; init; }
        [JsonPropertyOrder(1)] public required double Y { get; init; }
    }
}
