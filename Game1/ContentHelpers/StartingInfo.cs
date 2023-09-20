using System.Text.Json.Serialization;

namespace Game1.ContentHelpers
{
    [Serializable]
    public readonly struct StartingInfo
    {
        [JsonPropertyOrder(0)] public required Vector2Info WorldCenter { get; init; }
        [JsonPropertyOrder(1)] public required double CameraViewHeight { get; init; }
        // SortedDictionary so that the key value pair always appear in the same order
        [JsonPropertyOrder(2)] public required SortedDictionary<StartingBuilding, string?> StartingBuildingLocations { get; init; }
    }
}
