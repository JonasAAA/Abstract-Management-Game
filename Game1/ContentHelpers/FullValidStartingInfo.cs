using Game1.Collections;

namespace Game1.ContentHelpers
{
    [Serializable]
    public readonly struct FullValidStartingInfo
    {
        public static Result<FullValidStartingInfo, TextErrors> Create(ValidStartingInfo startingInfo)
            => startingInfo.StartingBuildingToCosmicBody.SelectValues<Result<string, TextErrors>>
            (
                (startingBuilding, cosmicBodyNameOrNull) => cosmicBodyNameOrNull switch
                {
                    string cosmicBodyName => new(ok: cosmicBodyName),
                    null => new(errors: new(value: $"{startingBuilding} must be not null"))
                }
            ).AccumulateErrors().Select
            (
                startingBuildingToCosmicBody => new FullValidStartingInfo
                (
                    worldCenter: startingInfo.WorldCenter,
                    cameraViewHeight: startingInfo.CameraViewHeight,
                    startingBuildingToCosmicBody: startingBuildingToCosmicBody
                )
            );

        public MyVector2 WorldCenter { get; }
        public Length CameraViewHeight { get; }
        public EnumDict<StartingBuilding, string> StartingBuildingToCosmicBody { get; }

        private FullValidStartingInfo(MyVector2 worldCenter, Length cameraViewHeight, EnumDict<StartingBuilding, string> startingBuildingToCosmicBody)
        {
            WorldCenter = worldCenter;
            CameraViewHeight = cameraViewHeight;
            StartingBuildingToCosmicBody = startingBuildingToCosmicBody;
        }
    }
}
