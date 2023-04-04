namespace Game1.ContentHelpers
{
    [Serializable]
    public readonly struct ValidStartingInfo
    {
        public static ValidStartingInfo CreateOrThrow(StartingInfo startingInfo)
            => CreateOrThrow
            (
                houseCosmicBodyName: startingInfo.HouseCosmicBody,
                powerPlantCosmicBodyName: startingInfo.PowerPlantCosmicBody,
                worldCenter: new(x: startingInfo.WorldCenter.X, y: startingInfo.WorldCenter.Y),
                cameraViewHeight: (UDouble)startingInfo.CameraViewHeight
            );

        public static ValidStartingInfo CreateOrThrow(string? houseCosmicBodyName, string? powerPlantCosmicBodyName, MyVector2 worldCenter, UDouble cameraViewHeight)
        {
            if (cameraViewHeight <= 0)
                throw new ContentException("Starting camera view height must be positive");
            if (houseCosmicBodyName is not null && houseCosmicBodyName == powerPlantCosmicBodyName)
                throw new ContentException("Starting house cosmic body name must differ from starting power plant cosmic body name. Currently they are both \"{houseCosmicBodyName}\"");
            return new
            (
                houseCosmicBodyName: houseCosmicBodyName,
                powerPlantCosmicBodyName: powerPlantCosmicBodyName,
                worldCenter: worldCenter,
                cameraViewHeight: cameraViewHeight
            );
        }

        public string? HouseCosmicBody { get; }
        public string? PowerPlantCosmicBody { get; }
        public MyVector2 WorldCenter { get; }
        public UDouble CameraViewHeight { get; }

        private ValidStartingInfo(string? houseCosmicBodyName, string? powerPlantCosmicBodyName, MyVector2 worldCenter, UDouble cameraViewHeight)
        {
            HouseCosmicBody = houseCosmicBodyName;
            PowerPlantCosmicBody = powerPlantCosmicBodyName;
            WorldCenter = worldCenter;
            CameraViewHeight = cameraViewHeight;
        }

        public StartingInfo ToJsonable()
            => new()
            {
                HouseCosmicBody = HouseCosmicBody,
                PowerPlantCosmicBody = PowerPlantCosmicBody,
                WorldCenter = new()
                {
                    X = WorldCenter.X,
                    Y = WorldCenter.Y
                },
                CameraViewHeight = CameraViewHeight
            };
    }
}
