﻿using System.Text.Json.Serialization;

namespace Game1.ContentHelpers
{
    [Serializable]
    public readonly record struct CosmicBodyInfo
    {
#pragma warning disable CA1819 // Properties should not return arrays. The array here is needed for this to correspond to Json arrays
        [JsonPropertyOrder(0)] public required string Name { get; init; }
        [JsonPropertyOrder(1)] public required Vector2Info Position { get; init; }
        [JsonPropertyOrder(2)] public required double Radius { get; init; }
        [JsonPropertyOrder(3)] public required RawMatPropor[] Composition { get; init; }
#pragma warning restore CA1819 // Properties should not return arrays
    }
}
