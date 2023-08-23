namespace Game1.ContentHelpers
{
    [Serializable]
    public readonly struct FullValidStartingInfo
    {
        public static Result<FullValidStartingInfo, TextErrors> Create(ValidStartingInfo startingInfo)
        {
            return Result.Lift
            (
                func: (arg1, arg2, arg3, arg4) => new FullValidStartingInfo
                (
                    worldCenter: startingInfo.WorldCenter,
                    cameraViewHeight: startingInfo.CameraViewHeight,
                    powerPlantCosmicBody: arg1,
                    gearStorageCosmicBody: arg2,
                    wireStorageCosmicBody: arg3,
                    roofTileStorageCosmicBody: arg4

                ),
                arg1: CosmicBodyNameOrNullToResult
                (
                    cosmicBodyName: startingInfo.PowerPlantCosmicBody,
                    cosmicBodyPurpose: nameof(StartingInfo.PowerPlantCosmicBody)
                ),
                arg2: CosmicBodyNameOrNullToResult
                (
                    cosmicBodyName: startingInfo.GearStorageCosmicBody,
                    cosmicBodyPurpose: nameof(StartingInfo.GearStorageCosmicBody)
                ),
                arg3: CosmicBodyNameOrNullToResult
                (
                    cosmicBodyName: startingInfo.WireStorageCosmicBody,
                    cosmicBodyPurpose: nameof(StartingInfo.WireStorageCosmicBody)
                ),
                arg4: CosmicBodyNameOrNullToResult
                (
                    cosmicBodyName: startingInfo.RoofTileStorageCosmicBody,
                    cosmicBodyPurpose: nameof(StartingInfo.RoofTileStorageCosmicBody)
                )
            );

            static Result<string, TextErrors> CosmicBodyNameOrNullToResult(string? cosmicBodyName, string cosmicBodyPurpose)
                => cosmicBodyName switch
                {
                    string powerPlantCosmicBody => new(ok: powerPlantCosmicBody),
                    null => new(errors: new(value: $"Starting {cosmicBodyPurpose} must be not null"))
                };
        }

        public MyVector2 WorldCenter { get; }
        public UDouble CameraViewHeight { get; }
        public string PowerPlantCosmicBody { get; }
        public string GearStorageCosmicBody { get; }
        public string WireStorageCosmicBody { get; }
        public string RoofTileStorageCosmicBody { get; }

        private FullValidStartingInfo(MyVector2 worldCenter, UDouble cameraViewHeight, string powerPlantCosmicBody, string gearStorageCosmicBody,
            string wireStorageCosmicBody, string roofTileStorageCosmicBody)
        {
            WorldCenter = worldCenter;
            CameraViewHeight = cameraViewHeight;
            PowerPlantCosmicBody = powerPlantCosmicBody;
            GearStorageCosmicBody = gearStorageCosmicBody;
            WireStorageCosmicBody = wireStorageCosmicBody;
            RoofTileStorageCosmicBody = roofTileStorageCosmicBody;
        }
    }
}
