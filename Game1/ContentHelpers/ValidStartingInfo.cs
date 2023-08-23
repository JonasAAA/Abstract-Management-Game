using Game1.Collections;

namespace Game1.ContentHelpers
{
    [Serializable]
    public readonly struct ValidStartingInfo
    {
        public static ValidStartingInfo CreateOrThrow(StartingInfo startingInfo)
            => CreateOrThrow
            (
                worldCenter: new(x: startingInfo.WorldCenter.X, y: startingInfo.WorldCenter.Y),
                cameraViewHeight: (UDouble)startingInfo.CameraViewHeight,
                powerPlantCosmicBody: startingInfo.PowerPlantCosmicBody,
                gearStorageCosmicBody: startingInfo.GearStorageCosmicBody,
                wireStorageCosmicBody: startingInfo.WireStorageCosmicBody,
                roofTileStorageCosmicBody: startingInfo.RoofTileStorageCosmicBody
            );

        public static ValidStartingInfo CreateOrThrow(MyVector2 worldCenter, UDouble cameraViewHeight, string? powerPlantCosmicBody, string? gearStorageCosmicBody,
            string? wireStorageCosmicBody, string? roofTileStorageCosmicBody)
        {
            if (cameraViewHeight <= 0)
                throw new ContentException("Starting camera view height must be positive");
            EfficientReadOnlyCollection<string?> notNullImportantCosmicBodyNames = new List<string?>()
            {
                powerPlantCosmicBody,
                gearStorageCosmicBody,
                wireStorageCosmicBody,
                roofTileStorageCosmicBody
            }.Where(cosmicBodyName => cosmicBodyName is not null).ToEfficientReadOnlyCollection();
            if (notNullImportantCosmicBodyNames.ToEfficientReadOnlyHashSet().Count != notNullImportantCosmicBodyNames.Count)
                throw new ArgumentException("${nameof(PowerPlantCosmicBody)}, {nameof(GearStorageCosmicBody)}, {nameof(WireStorageCosmicBody)}, {nameof(RoofTileStorageCosmicBody)} must be distinct");
            return new
            (
                worldCenter: worldCenter,
                cameraViewHeight: cameraViewHeight,
                powerPlantCosmicBody: powerPlantCosmicBody,
                gearStorageCosmicBody: gearStorageCosmicBody,
                wireStorageCosmicBody: wireStorageCosmicBody,
                roofTileStorageCosmicBody: roofTileStorageCosmicBody
            );
        }

        public MyVector2 WorldCenter { get; }
        public UDouble CameraViewHeight { get; }
        public string? PowerPlantCosmicBody { get; }
        public string? GearStorageCosmicBody { get; }
        public string? WireStorageCosmicBody { get; }
        public string? RoofTileStorageCosmicBody { get; }

        private ValidStartingInfo(MyVector2 worldCenter, UDouble cameraViewHeight, string? powerPlantCosmicBody, string? gearStorageCosmicBody,
            string? wireStorageCosmicBody, string? roofTileStorageCosmicBody)
        {
            WorldCenter = worldCenter;
            CameraViewHeight = cameraViewHeight;
            PowerPlantCosmicBody = powerPlantCosmicBody;
            GearStorageCosmicBody = gearStorageCosmicBody;
            WireStorageCosmicBody = wireStorageCosmicBody;
            RoofTileStorageCosmicBody = roofTileStorageCosmicBody;
        }

        public StartingInfo ToJsonable()
            => new()
            {
                WorldCenter = new()
                {
                    X = WorldCenter.X,
                    Y = WorldCenter.Y
                },
                CameraViewHeight = CameraViewHeight,
                PowerPlantCosmicBody = PowerPlantCosmicBody,
                GearStorageCosmicBody = GearStorageCosmicBody,
                WireStorageCosmicBody = WireStorageCosmicBody,
                RoofTileStorageCosmicBody = RoofTileStorageCosmicBody
            };
    }
}
