using Game1.Collections;

namespace Game1.ContentHelpers
{
    [Serializable]
    public readonly struct ValidStartingInfo
    {
        public static ValidStartingInfo CreateOrThrow(StartingInfo startingInfo)
            => CreateOrThrow
            (
                worldCenter: new(x: SignedLength.CreateFromM(startingInfo.WorldCenter.X), y: SignedLength.CreateFromM(startingInfo.WorldCenter.Y)),
                cameraViewHeight: Length.CreateFromM((UDouble)startingInfo.CameraViewHeight),
                startingBuildingToCosmicBody: new
                (
                    startingBuilding =>
                    {
                        if (startingInfo.StartingBuildingLocations.TryGetValue(startingBuilding, out var cosmicBody))
                            return cosmicBody;
                        throw new ArgumentException($"All starting building locations must be set. That's not the case for {cosmicBody}");
                    }
                )
            );

        public static ValidStartingInfo CreateOrThrow(MyVector2 worldCenter, Length cameraViewHeight, EnumDict<StartingBuilding, string?> startingBuildingToCosmicBody)
        {
            if (cameraViewHeight.IsTiny())
                throw new ContentException("Starting camera view height must be positive");
            var notNullImportantCosmicBodyNames =
                startingBuildingToCosmicBody.Values
                .Where(cosmicBodyName => cosmicBodyName is not null)
                .ToEfficientReadOnlyCollection();
            if (notNullImportantCosmicBodyNames.ToEfficientReadOnlyHashSet().Count != notNullImportantCosmicBodyNames.Count)
                throw new ArgumentException($"{string.Join(", ", Enum.GetValues<StartingBuilding>())} must be distinct");
            return new
            (
                worldCenter: worldCenter,
                cameraViewHeight: cameraViewHeight,
                startingBuildingToCosmicBody: startingBuildingToCosmicBody
            );
        }

        public MyVector2 WorldCenter { get; }
        public Length CameraViewHeight { get; }
        public EnumDict<StartingBuilding, string?> StartingBuildingToCosmicBody { get; }

        private ValidStartingInfo(MyVector2 worldCenter, Length cameraViewHeight, EnumDict<StartingBuilding, string?> startingBuildingToCosmicBody)
        {
            WorldCenter = worldCenter;
            CameraViewHeight = cameraViewHeight;
            StartingBuildingToCosmicBody = startingBuildingToCosmicBody;
        }

        public StartingInfo ToJsonable()
            => new()
            {
                WorldCenter = new()
                {
                    X = WorldCenter.X.valueInM,
                    Y = WorldCenter.Y.valueInM
                },
                CameraViewHeight = CameraViewHeight.valueInM,
                StartingBuildingLocations = StartingBuildingToCosmicBody.ToSortedDictionary()
            };
    }
}
