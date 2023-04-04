using System.Text.Json.Serialization;

namespace Game1.ContentHelpers
{
    [Serializable]
    public readonly struct LinkInfo
    {
        [JsonPropertyOrder(0)] public required string From { get; init; }
        [JsonPropertyOrder(1)] public required string To { get; init; }
    }
}
