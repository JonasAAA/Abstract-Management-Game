using System.Text.Json.Serialization;

namespace Game1.ContentHelpers
{
    [Serializable]
    public readonly record struct RawMatPropor
    {
        [JsonPropertyOrder(0)] public required int RawMaterial { get; init; }
        [JsonPropertyOrder(1)] public required int Percentage { get; init; }
    }
}
