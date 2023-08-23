using System.Text.Json.Serialization;

namespace Game1.ContentHelpers
{
    [Serializable]
    public readonly struct StartingInfo
    {
        [JsonPropertyOrder(0)] public required Vector2Info WorldCenter { get; init; }
        [JsonPropertyOrder(1)] public required double CameraViewHeight { get; init; }
        [JsonPropertyOrder(2)] public required string? PowerPlantCosmicBody { get; init; }
        [JsonPropertyOrder(3)] public required string? GearStorageCosmicBody { get; init; }
        [JsonPropertyOrder(3)] public required string? WireStorageCosmicBody { get; init; }
        [JsonPropertyOrder(3)] public required string? RoofTileStorageCosmicBody { get; init; }
    }
}
