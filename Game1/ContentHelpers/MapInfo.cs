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

        public static bool IsFileReady(FilePath mapFullPath)
        {
            using StreamReader streamReader = new(mapFullPath.CreateFileStream(FilePath.FileAccess.Read));
            return !Algorithms.StreamStartsWith(streamReader: streamReader, tokens: fileNotReadyTokens);
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
#pragma warning disable CA1819 // Properties should not return arrays. The array here is needed for this to correspond to Json arrays
        [JsonPropertyOrder(0)] public bool NotReadyToUse { get; init; }
        [JsonPropertyOrder(1)] public required CosmicBodyInfo[] CosmicBodies { get; init; }
        [JsonPropertyOrder(2)] public required LinkInfo[] Links { get; init; }
        [JsonPropertyOrder(3)] public required StartingInfo StartingInfo { get; init; }
#pragma warning restore CA1819 // Properties should not return arrays
    }
}
