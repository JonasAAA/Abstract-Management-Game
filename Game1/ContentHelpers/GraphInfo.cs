using System.IO;
using System.Text.Json;

namespace Game1.ContentHelpers
{
    [Serializable]
    public readonly struct GraphInfo
    {
        // required means that the property must be in json, as said here https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/required-properties
        // TODO: could create a json schema for the file and use it to validate file while someone is writing it
        // TODO: could use https://docs.microsoft.com/en-us/dotnet/api/system.text.json.serialization.jsonextensiondataattribute?view=net-7.0
        // to check for fields provided in json but deserialised
        public static GraphInfo LoadFrom(string fileName)
            => JsonSerializer.Deserialize<GraphInfo>
            (
                json: File.ReadAllText(fileName)
            );

        public required CosmicBodyInfo[] CosmicBodies { get; init; }
        public required LinkInfo[] Links { get; init; }
        public required StartingInfo StartingInfo { get; init; }
    }
}
