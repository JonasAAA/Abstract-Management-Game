namespace Game1.ContentHelpers
{
    [Serializable]
    public readonly struct FullValidStartingInfo
    {
        public static Result<FullValidStartingInfo, IEnumerable<string>> Create(ValidStartingInfo startingInfo)
        {
            List<string> errorMessages = new();
            if (startingInfo.HouseCosmicBody is null)
                errorMessages.Add($"Starting {nameof(StartingInfo.HouseCosmicBody)} must be set");
            if (startingInfo.PowerPlantCosmicBody is null)
                errorMessages.Add($"Starting {nameof(StartingInfo.PowerPlantCosmicBody)} must be set");
            if (errorMessages.Count is 0)
                return new
                (
                    ok: new
                    (
                        houseCosmicBody: startingInfo.HouseCosmicBody!,
                        powerPlantCosmicBody: startingInfo.PowerPlantCosmicBody!,
                        worldCenter: startingInfo.WorldCenter,
                        cameraViewHeight: startingInfo.CameraViewHeight
                    )
                );
            return new(errors: errorMessages); 
        }

        public string HouseCosmicBody { get; }
        public string PowerPlantCosmicBody { get; }
        public MyVector2 WorldCenter { get; }
        public UDouble CameraViewHeight { get; }

        private FullValidStartingInfo(string houseCosmicBody, string powerPlantCosmicBody, MyVector2 worldCenter, UDouble cameraViewHeight)
        {
            HouseCosmicBody = houseCosmicBody;
            PowerPlantCosmicBody = powerPlantCosmicBody;
            WorldCenter = worldCenter;
            CameraViewHeight = cameraViewHeight;
        }
    }
}
