using System.IO;
using System.Text.Json.Serialization;

namespace Game1.ContentHelpers
{
    /// <summary>
    /// Data may be incomplete and/or invalid
    /// </summary>
    [Serializable]
    public readonly struct MapInfo
    {
        private static readonly string[] fileNotReadyTokens = { "{", $"\"{nameof(NotReadyToUse)}\"", ":", "true" };

        public static bool IsFileReady(string mapFullPath)
        {
            using StreamReader streamReader = new(mapFullPath);
            return !Algorithms.StreamStartsWith(streamReader: streamReader, tokens: fileNotReadyTokens);
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        [JsonPropertyOrder(0)] public bool NotReadyToUse { get; init; }
        [JsonPropertyOrder(1)] public required CosmicBodyInfo[] CosmicBodies { get; init; }
        [JsonPropertyOrder(2)] public required LinkInfo[] Links { get; init; }
        [JsonPropertyOrder(3)] public required StartingInfo StartingInfo { get; init; }
    }
}
